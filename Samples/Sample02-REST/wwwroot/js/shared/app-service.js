angular.module('spa')
    .factory("genericCollectionFactory",
        [
            "$http", "$log", "$timeout", "$rootScope", "$resource", function ($http, $log, $timeout, $rootScope, $resource) {


                $rootScope.stack = $rootScope.stack ?? {};

                angular.merge($rootScope.stack, { data: { sources: {}, factories: {} } });

                var map = $rootScope.stack.data.sources;

                var that = this;

                this.fetchSetQueue = function (collection) {
                    var queueCopy = map[collection].requestQueue.slice(0);

                    var processQueue = function (datsrc) {
                        angular.forEach(datsrc, function (dat, key) { that.processReturn(collection, queueCopy[key], dat); });


                        if (map[collection].announceChanges) {
                            var broadcastKey = "genericCollectionFactory::" + map[collection].collection + "::dataArrived";
                            $rootScope.$emit(broadcastKey);
                            console.log(broadcastKey);
                        }

                        that.checkScheduleMore(collection);
                    };

                    map[collection].requestQueue.length = 0;

                    var serializedQueueCopy = queueCopy.toString();

                    // console.log("FETCH [" + collection + "]: " + queueCopy.length + " [" + serializedQueueCopy + "]");

                    if (map[collection].resource) {
                        map[collection].resource.querySubset(serializedQueueCopy,
                            function (data) { processQueue(data); },
                            function (response) { that.checkScheduleMore(collection); });

                    } else {

                        $http.post(map[collection].endpoint, serializedQueueCopy)
                            .then(function (response) { processQueue(response.data); },
                                function (response) {
                                    console.log(response);
                                    that.checkScheduleMore(collection);
                                });
                    }
                };

                this.checkScheduleMore = function (collection) {
                    if (map[collection].requestQueue.length > 0) {
                        that.fetchSetQueue(collection);
                    } else {
                        delete map[collection].scheduledLoad;
                    }
                };

                this.processReturn = function (collection, locator, dat) {
                    var target = map[collection].items[locator];

                    if (!target) {
                        // $log.warn("[" + collection + "] locator not found: " + locator);
                        return;
                    }
                    if (map[collection].validateEntry) {

                        if (!map[collection].validateEntry(locator, dat)) {

                            console.log(collection, locator, "Invalid entry");

                            target.isPresent = false;
                            target._isValid = false;
                            return;
                        }
                    }

                    for (var k in dat) target[k] = dat[k];

                    target.isPresent = true;
                    target._isValid = true;

                    if (map[collection].postProcessor) {
                        try {
                            map[collection].postProcessor(target);
                        } catch (e) {
                            $log.warn(e);
                        }
                    }
                };

                this.queueItem = function (collection, locator) {
                    map[collection].requestQueue.push(locator);
                    if (!map[collection].scheduledLoad) {
                        map[collection].scheduledLoad = $timeout(function () { that.fetchSetQueue(collection); }, 50);
                    }
                };
                return {
                    register: function (config) {
                        config.datapoint = config.datapoint || config.endpoint;
                        config.buffered = !!config.buffered;
                        config.isBufferedSet = config.isBufferedSet || config.buffered;
                        config.isPaged = config.isPaged || config.paged;
                        config.col = config.col || config.collection;

                        if (typeof config.useExtendedResourceMethods === "undefined")
                            config.useExtendedResourceMethods = true;

                        if (config.col === undefined) return null;
                        if (config.col === null) return null;

                        // console.log("Registering collection: " + config.col + " => " + config.datapoint);

                        // ReSharper disable once InconsistentNaming
                        var _factoryInstance = {
                            collection: config.col,
                            configuration: config,
                            data: function (locator) {
                                var fThis = this;

                                locator = ("" + locator).trim().toLowerCase();

                                if (map[fThis.collection].forceIdentifierUpperCase) locator = locator.toUpperCase();

                                if (locator === "") return null;
                                if (locator === "undefined") return null;

                                if (map[fThis.collection].items[locator] === undefined) {

                                    // console.log(fThis.collection + ":" + locator + " NOT FOUND");

                                    map[fThis.collection].items[locator] = { Locator: locator, isPresent: false };

                                    if (map[fThis.collection].isBufferedSet) {
                                        that.queueItem(fThis.collection, locator);
                                    } else {

                                        var targetUrl = map[fThis.collection].getExEndpoint ||
                                            map[fThis.collection].endpoint;

                                        if (targetUrl.indexOf("{0}") !== -1)
                                            targetUrl = targetUrl.replace("{0}", locator);
                                        else if (targetUrl.indexOf(":id") !== -1)
                                            targetUrl = targetUrl.replace(":id", locator);

                                        $http.get(targetUrl)
                                            .then(function (response) {
                                                var dat = response.data;
                                                that.processReturn(fThis.collection, locator, dat);
                                            },
                                                function (response) {
                                                    map[fThis.collection].items[locator].isPresent = false;
                                                });
                                    }
                                }

                                var target = map[fThis.collection].items[locator];

                                // console.log(fThis.collection + ":" + locator + " " + JSON.stringify(target));

                                return target;
                            },
                            flush: function (locator) {
                                var fThis = this;
                                try {
                                    delete map[fThis.collection].items[locator];
                                } catch (e) { }

                            },
                            resource: function () {
                                var fThis = this;

                                map[fThis.collection].resource.fetch = function (locator) {
                                    locator = ("" + locator).trim().toLowerCase();
                                    that.queueItem(fThis.collection, locator);
                                };
                                map[fThis.collection].resource.flush = function (locator) {
                                    delete map[fThis.collection].items[locator];
                                };

                                map[fThis.collection].resource.setData = function () {
                                    map[fThis.collection].resource.query(function (data) { });
                                    return map[fThis.collection].items;
                                };

                                return map[this.collection].resource;
                            }
                        };

                        // Factory provider content
                        config.items = [];
                        config.requestQueue = [];

                        map[config.col] = config;

                        if (typeof config.datapoint === "string") {

                            if (config.datapoint.indexOf("/:id") === -1) map[config.col].endpoint = config.datapoint;
                            else {

                                map[config.col].baseEndpoint = config.datapoint.replace("/:id", "");

                                var initObj = {};

                                if (config.useExtendedResourceMethods) {

                                    initObj = {
                                        querySubset: {
                                            method: "POST",
                                            isArray: true,
                                            url: map[config.col].baseEndpoint + "/subset"
                                        }
                                    };

                                    if (config.getExEndpoint)
                                        initObj.getEx = { method: "GET", isArray: false, url: config.getExEndpoint };
                                    if (config.queryExEndpoint)
                                        initObj.queryEx = { method: "GET", isArray: true, url: config.queryExEndpoint };
                                    if (config.postExEndpoint)
                                        initObj.postEx = { method: "POST", url: config.postExEndpoint };
                                }

                                map[config.col].resource = $resource(config.datapoint, {}, initObj);
                            }
                        } else {
                            map[config.col].resource = config.datapoint;
                        }

                        return _factoryInstance;
                    }
                };
            }
        ])
    .service("genericCollectionService",
        [
            "$http", "$log", function ($http, $log) {
                var map = {};

                var resourceHooks = ["save", "remove", "delete"];

                this.register = function (collection, resource) {
                    if (collection === undefined) return;
                    if (collection === null) return;

                    map[collection] = { data: [] };

                    if (resource) {

                        map[collection].resource = {};

                        angular.forEach(Object.getOwnPropertyNames(resource),
                            function (fn) {
                                if (resourceHooks.indexOf(fn) > -1) {
                                    map[collection].resource[fn] = function () {
                                        console.log(collection +
                                            ": " +
                                            fn.toUpperCase() +
                                            " " +
                                            JSON.stringify(arguments[0]));
                                        resource[fn].apply(null, arguments);
                                        delete map[collection].data[arguments.id];
                                    };
                                } else {
                                    map[collection].resource[fn] = resource[fn];
                                }
                            });

                    }
                };
                this.data = function (collection, locator, endpoint, postProcessor) {
                    var processResponse = function (dat) {
                        var target = map[collection].data[locator];

                        for (var k in dat) target[k] = dat[k];

                        target.isPresent = true;

                        if (postProcessor) {
                            try {
                                postProcessor(target);
                            } catch (e) {
                                $log.warn(e);
                            }
                        }
                    };

                    if (locator === undefined) return null;
                    if (locator === null) return null;

                    if (collection === undefined) return null;
                    if (collection === null) return null;

                    if (!map[collection]) {
                        console.log(collection + " " + locator + ": Collection doesn't exist.");
                        return null;
                    }

                    locator = ("" + locator).trim().toLowerCase();

                    if (map[collection].data[locator] === undefined) {
                        map[collection].data[locator] = { Locator: locator, isPresent: false };

                        if (map[collection].resource === undefined) {

                            $http.get(endpoint)
                                .then(function (response) { processResponse(response.data); },
                                    function (response) { map[collection].data[locator].isPresent = false; });
                        } else {
                            map[collection].resource.get({ id: locator },
                                function (data) {
                                    console.log("CALLER");

                                    processResponse(data);
                                },
                                function (data) { map[collection].data[locator].isPresent = false; });
                        }
                    }

                    return map[collection].data[locator];
                };
                this.resource = function (collection) { return map[collection].resource; };
            }
        ])
    .factory('BearerAuthInterceptor', function ($window, $q) {
        return {
            request: function (config) {
                config.headers = config.headers || {};
                if ($window.localStorage.getItem('token')) {
                    // may also use sessionStorage
                    config.headers.Authorization = 'Bearer ' + $window.localStorage.getItem('token');
                }
                return config || $q.when(config);
            },
            response: function (response) {
                if (response.status === 401) {
                    //  Redirect user to login page / signup Page.
                }
                return response || $q.when(response);
            }
        };
    });