(function () {
    var self = this;

    var dbg = true;
    var $stateProviderRef = null;

    if (typeof module !== "undefined" &&
        typeof exports !== "undefined" &&
        module.exports === exports) {
        module.exports = "slimForm";
    }

    var baseItemLink = function ($scope, $log, $state, $stateParams, $q) {

        var handleFormConfig = function (src) {
            if (!$scope._form) $scope._form = { config: {} };
            if (!$scope._form.config.autoHandled) {
                $scope._form.config.autoHandled = true;
                $scope._form.config.access = angular.fromJson(src("x-access")) || $scope._form.config.access;

                if ($scope._form.config.access) $scope._form.config.autoResolution = true;

            }
        };

        $scope.$watch("data", function () {
            if (!$scope.data) return;
            $scope.data.selectedLocator = $stateParams.id;
            $scope.state.mode = $stateParams.verb;
            $scope.data.newPermission = { verb: "NEW" };

            $scope.load = function () {
                delete $scope.data.error;
                $scope.data.item = {};
                $scope.data.original = {};

                $scope.data.selectedPayload = { 'id': $scope.data.selectedLocator };

                if ($scope.events.beforeLoad) $scope.events.beforeLoad($scope.data.selectedLocator, $stateParams);

                if (dbg) console.log("FETCH " + $scope.data.selectedLocator);

                if (!$scope.data.provider) return;

                var getMethod = $scope.data.provider.getEx || $scope.data.provider.get;

                $scope.data.item = {};


                getMethod($scope.data.selectedPayload, function (data, headers) {
                    if ($scope.events.afterLoad) {
                        var ret = $scope.events.afterLoad(data);
                        if (ret) data = ret;
                    }

                    handleFormConfig(headers);
                    $scope.handleDates(data);

                    if (dbg) console.log("load: Data", data);
                    if (dbg) console.log("load: headers", headers);

                    var copy = JSON.parse(JSON.stringify(data));

                    if (dbg) console.log("load: copy", copy);

                    $scope.handleDates(copy);

                    $scope.data.item = data;
                    $scope.data.original = copy; // Keep a copy of the original object

                    $scope.record.$setPristine();
                },
                    function (err) {
                        $scope.data.error = err;
                        console.warn("Error", err);
                    });
            };

            $scope.events.save = function () {


                // First, default form validation.

                if (!$scope.record.$valid) {
                    $log.warn("Verify the requirements for each field and try again.");
                    return;
                }

                // Now, custom:
                if ($scope.events.validate) {

                    var e = $scope.events.validate($scope.data.item);

                    if (!e.success) {

                        if (!e.message) e.message = "Validation unsuccessful.";

                        $log.warn(e.message);
                        return;
                    }
                }

                if ($scope.events.beforeSave) $scope.events.beforeSave($scope.data.item);

                const saveCall = $scope.data.provider.postEx
                    ? $scope.data.provider.postEx
                    : $scope.data.provider.save;

                return $q(function (resolve, reject) {
                    saveCall($scope.data.item,
                        function (data, headers, status) {
                            if ($scope.events.afterSave) $scope.events.afterSave(data);

                            $scope.data.item = data;
                            var idField = $scope.config.data.fetchLocator || $scope.config.data.locator;
                            var id = data[idField];
                            var name = data[$scope.config.data.title];

                            $scope.data.provider.flush(id);
                            $scope.data.provider.fetch(id);

                            $scope.loadAll();

                            // toaster.pop({ type: "success", title: name, body: "Item saved successfully." });

                            $state.go("^.detail", { id: id, verb: "view" });

                            resolve(data);
                        },
                        function (err) {
                            console.info(err);
                            reject(err);
                        });
                });
            };

            $scope.events.delete = function () {
                if ($scope.events.beforeDelete) $scope.events.beforeDelete($scope.data.selectedLocator);



                $scope.data.provider.delete({ id: $scope.data.selectedLocator },
                    function (data) {
                        if ($scope.events.afterDelete) $scope.events.afterDelete(data);

                        $scope.data.provider.flush(data[$scope.config.data.fetchLocator ||
                            $scope.config.data.locator]);

                        console.info("Delete successful.");

                        $scope.loadAll();
                        $state.go("^", {}, { reload: true });
                    });

            };

            $scope.revert = function () {
                $log.info($scope.record.$dirty ? "Changes reversed." : "Action cancelled.");

                $scope.data.item = JSON.parse(JSON.stringify($scope.data.original));
                $state.go($state.current, { verb: "view" });
            };

            $scope.evalShow = function (i) { return typeof i === "function" ? i() : i; };
            $scope.load();
        });
    };

    var baseListLink = function ($scope, $timeout, $filter, $http) {
        var runSearch = function () {
            if ($scope.data.postSearchFilter) $scope.data.postSearchFilter();
            $scope.loadAll();
        };

        $scope.$watch("searchTerm",
            function () {
                if ($scope.pagination)
                    if ($scope.pagination.query) {
                        $scope.pagination.query.q = $scope.searchTerm;
                        $scope.pagination.query.page = 0;
                        $("#slimFormListInnerContainer").scrollTop(0);
                    }

                if (!$scope.config.data) return;
                if ($scope.config.data.paged) { // Content is paged, so use Query engine
                    $scope.loadAll();
                } else {
                    if (!$scope.searchTerm) {
                        delete $scope.searchTerm;
                        return;
                    }
                    $scope.data.filteredItems =
                        $filter("genericQuickSearch")($scope.data.items, $scope.searchTerm);
                }
            });

        $scope.$watch("pagination", runSearch(), true);
    };

    var baseFormLink =
        function ($scope,
            $stateParams,
            $timeout,
            $state,
            $resource,
            genericCollectionFactory,
            $http,
            $rootScope,
            $log,
            element,
            attributes) {
            $scope.settings = $scope.$eval(attributes.settings) || $scope.$eval(attributes.settings);

            $scope.data = $scope.data || {};
            $scope.events = $scope.events || {};
            $scope.searchTerm = "";

            $scope.handleSetDates = function (set) {
                angular.forEach(set, function (item) { $scope.handleDates(item); });
            };
            $scope.handleDates = function (obj) {

                try {


                    for (var property in obj) {
                        if (obj.hasOwnProperty(property)) {
                            if (typeof obj[property] === "object") {
                                $scope.handleDates(obj[property]);
                            } else {
                                var src = obj[property];
                                var isValid = moment(src, moment.ISO_8601, true).isValid();

                                if (isValid) {
                                    var parsedDate = Date.parse(src);
                                    if (dbg)
                                        console.info(property + " is date (" + src + "). Converting", src, parsedDate);
                                    obj[property] = new Date(parsedDate);
                                }

                            }
                        }
                    }
                } catch (e) {

                }

            };

            $scope.functions = {
                dateTime: {
                    handle: $scope.handleDates,
                    handleSet: $scope.handleSetDates
                }
            };

            $scope.comparison = {
                source: "data.item",
                original: "data.original"
            };

            var evaluateAccess = function () {
                var acc = {
                    read: true,
                    write: true,
                    remove: true
                };

                if ($scope._form)
                    if ($scope._form.config) if ($scope._form.config.access) acc = $scope._form.config.access;

                if ($scope.config)
                    if ($scope.config.isReadOnly) {
                        acc.write = false;
                        acc.remove = false;
                    }

                $scope.access = acc;
            };

            $scope.filterHandler = {
                set: {},
                filter: {},
                add: function (q) {

                    if (this.set == null) this.set = {};

                    console.log("this = ?", this === this);
                    console.log("this", this);
                    console.log("this.set", this.set);
                    console.log("this.set[q.label]", this.set[q.label]);
                    console.log("q", q);

                    this.set[q.label] = q;
                    angular.merge(this.filter, q.filter);
                    $scope.pagination.query.filter = this.filter;
                    $log.info("Filtering by " + q.label + ": " + q.display);
                    $("#slimFormListInnerContainer").scrollTop(0);
                    $scope.pagination.query.page = 0;
                    $scope.loadAll();
                },
                addSimple: function (field, value, label, display) {
                    if (label === undefined) label = field;
                    if (display === undefined) display = value;

                    var f = {};

                    console.log(field, value);

                    f[field] = value;

                    var payload = { label: label, display: display, filter: f };

                    console.log('addSimple', payload);

                    this.add(payload);
                },

                clear: function () {
                    Object.keys(this.filter)
                        .forEach(function (key) { delete this.filter[key]; });
                    Object.keys(this.set)
                        .forEach(function (key) { delete this.set[key]; });

                    delete $scope.pagination.query.filter;

                    $scope.loadAll();
                },
                del: function (q) {

                    delete this.set[q];
                    delete this.filter[q];

                    if (Object.keys(this.filter).length)
                        $scope.pagination.query.filter = this.filter;
                    else delete $scope.pagination.query.filter;

                    if (dbg) console.log(q, this);

                    $scope.loadAll();
                }
            };

            // Function controls
            $scope.access = { read: true, write: true, remove: true };
            $scope.$watch("_form", function (i) { evaluateAccess(); }, true);
            $scope.$watch("config", function (i) { evaluateAccess(); }, true);

            $scope.state = { current: $stateParams };

            $scope.pagination = {
                query: {},
                pages: 0
            };

            var handleFormConfig = function (src) {
                if (!$scope._form) $scope._form = { config: {} };
                if (!$scope._form.config.autoHandled) {
                    $scope._form.config.autoHandled = true;
                    $scope._form.config.access = angular.fromJson(src("x-access"));

                    if ($scope._form.config.access) $scope._form.config.autoResolution = true;

                }
            };
            $scope.filteredItems = [];

            this.getFilteredItemsRef = function () { return $scope.filteredItems; };

            $scope.Init = function () {
                if ($scope.config.data) {

                    var initObj = {
                        collection: $scope.config.title + "Data",
                        endpoint: $scope.config.data.endpoint,
                        paged: !!$scope.config.data.paged,
                        pageSize: $scope.config.data.pageSize
                    };

                    if (initObj.paged) {
                        $scope.pagination.query = { page: 0, limit: initObj.pageSize || 20, sort: "name" };
                    }

                    if ($scope.config.data.collectionFactory) {
                        $scope.config.collectionFactory = $scope.config.data.collectionFactory;
                    } else {

                        if ($scope.config.data.getExEndpoint) initObj.getExEndpoint = $scope.config.data.getExEndpoint;
                        if ($scope.config.data.postExEndpoint)
                            initObj.postExEndpoint = $scope.config.data.postExEndpoint;
                        if ($scope.config.data.queryExEndpoint)
                            initObj.queryExEndpoint = $scope.config.data.queryExEndpoint;

                        if ($scope.config.data.validateEntry) initObj.validateEntry = $scope.config.data.validateEntry;

                        $scope.config.collectionFactory = genericCollectionFactory.register(initObj);
                    }

                    if ($scope.config.data.title)
                        $scope.pagination.query.sort =
                            $scope.config.data.defaultSort || $scope.config.data.title;
                }

                // SORT definition

                if (!$scope.config.sort) $scope.config.sort = {};

                $scope.config.sort.select = function (i) {
                    var marker = "+";
                    if ($scope.config.sort.term === "+" + i.parameter) marker = "-";

                    marker = marker + i.parameter;
                    $scope.config.sort.term = marker;

                    if ($scope.pagination) if ($scope.pagination.query) $scope.pagination.query.sort = marker;

                    if ($scope.config.data.paged) { // Content is paged, so use Query engine
                        $scope.loadAll();
                    }
                };

                if (!$scope.config.actions) $scope.config.actions = {};

                ["edit", "view", "del"].forEach(function (i) {
                    if (!$scope.config.actions[i]) $scope.config.actions[i] = {};
                    if (!$scope.config.actions[i].buttons) $scope.config.actions[i].buttons = {};
                    if (!$scope.config.actions[i].icons) $scope.config.actions[i].icons = {};
                });

                var saveLabel = "Save";

                if (!$scope.config.actions.edit.buttons.commit)
                    $scope.config.actions.edit.buttons.commit = {
                        label: saveLabel,
                        callback: function () { $scope.events.save(); },
                        canShow: function () {
                            var e = ($scope.record.$dirty);
                            return e;
                        }
                    };

                if (!$scope.config.actions.list) $scope.config.actions.list = {};

                if (!$scope.config.actions.list.mainButton)
                    $scope.config.actions.list.mainButton = {
                        icon: "glyphicon glyphicon-pencil",
                        label: "Create New",
                        callback: function () {
                            var pref = "";
                            if ($state.current.name.indexOf(".") !== -1) pref = $state.current.name.split(".")[0];

                            $state.go(pref + ".detail", { id: "new", verb: "edit" });
                        },
                        canShow: function () {
                            return true;
                        },
                        hide: false

                    };
                if (!$scope.config.actions.del.buttons.commit)
                    $scope.config.actions.del.buttons.commit = {
                        label: "Delete",
                        callback: function () { $scope.events.delete(); },
                        canShow: true
                    };

                $scope.canShowActionSet = function (state) {

                    if ($scope.config.actions.asButtons) return true;

                    var act = $scope.config.actions[state];

                    if (!act) return false;
                    if (!act.buttons) return false;

                    for (var i in act.buttons) {
                        if (act.buttons[i].canShow)
                            if (typeof act.buttons[i].canShow === "function") {
                                if (act.buttons[i].canShow()) {
                                    return true;
                                }
                            } else {
                                return !!act.buttons[i].canShow;
                            }
                    }

                    return false;
                };

                if (!$scope.config.actions.edit.canCommit) $scope.config.actions.edit.canCommit = function () { };

                if ($scope.config.collectionFactory) {
                    $scope.data = {
                        get: function (locator) { return $scope.config.collectionFactory.data(locator); },
                        provider: $scope.config.collectionFactory.resource()
                    };
                    $scope.loadAll();
                }

                if ($scope.onFormReady) $scope.onFormReady();
            };

            if ($scope.config.isInfiniteScroll) {

                var scrollElem = null;
                var scrollProcessing = false;

                $scope.beforeOnLoadAll = function () {
                    scrollProcessing = false;
                    if (!scrollElem) {
                        scrollElem = $("#slimFormListInnerContainer");

                        $(scrollElem)
                            .scroll(function () {
                                if (scrollProcessing) return;

                                if ($(this).scrollTop() + $(this).innerHeight() >=
                                    $(this)[0].scrollHeight) {
                                    if (dbg) console.log($scope);
                                    scrollProcessing = true;
                                    $scope.fetchPage($scope.pagination.query.page + 1);
                                }
                            });

                    }
                };

            }

            $scope.goUpState = function () { $state.go("^"); };

            $scope.fetchNextPage = function () {
                $scope.pagination.query.page++;
                $scope.loadAll();
            };

            $scope.fetchPage = function (i) {
                $scope.pagination.query.page = i;
                $scope.loadAll();
            };

            $scope.loadAll = function () {
                if (!$scope.data.provider) return;

                var queryFn = $scope.data.provider.queryEx || $scope.data.provider.query;

                queryFn($scope.pagination.query, function (data, headers) {
                    if ($scope.config.isInfiniteScroll) {
                        var qClone = angular.copy($scope.pagination.query);
                        delete qClone.page;

                        if (angular.toJson(qClone) !==
                            angular.toJson($scope.pagination.prevQuery)) {
                            $scope.data.map = {};
                            $scope.data.items = [];

                            $scope.pagination.prevQuery = qClone;
                        }
                    }

                    $scope.functions.dateTime.handleSet(data);

                    if (!$scope.data.map) $scope.data.map = {};
                    if (!$scope.data.items) $scope.data.items = [];

                    if (!$scope.config.isInfiniteScroll) $scope.data.items = data;
                    else {
                        angular.forEach(data, function (i) { if (!$scope.data.map[i[$scope.config.data.locator]]) $scope.data.items.push(i); });
                    }

                    angular.forEach(data, function (i) { $scope.data.map[i[$scope.config.data.locator]] = i; });

                    handleFormConfig(headers);

                    $scope.pagination.records = headers("X-Total-Count");
                    $scope.pagination.pages = headers("X-Total-Pages");

                    if ($scope.beforeOnLoadAll) $scope.beforeOnLoadAll();
                    if ($scope.onLoadAll) $scope.onLoadAll();
                });
            };

            $scope.setDirty = function () { $timeout(function () { $scope.record.$dirty = true; }, 0); };
            $scope.Init();
        };

    angular
        .module("slimForm", [])
        .config(["$stateProvider", function ($stateProvider) { $stateProviderRef = $stateProvider; }])
        .directive("slimFormList", ["$timeout", "$filter", "$http", "appSettings", function ($timeout, $filter, $http, appSettings) {
            return {
                restrict: "AEC",
                transclude: true,
                scope: true,
                templateUrl: appSettings.config.resources.baseUri +
                    "js/shared/slimForm/templates/slimform-list.html" +
                    appSettings.config.app.version.tag,
                link: function ($scope) { return baseListLink($scope, $timeout, $filter, $http); }
            };
        }
        ])
        .directive("slimFormItem", ["$log", "$state", "$stateParams", "$q", "appSettings", function ($log, $state, $stateParams, $q, appSettings) {
            return {
                restrict: "AEC",
                transclude: { 'mainButton': "?slimFormItemMainButton", },
                templateUrl: appSettings.config.resources.baseUri + "js/shared/slimForm/templates/slimform-item.html" + appSettings.config.app.version.tag,
                link: function ($scope) { return baseItemLink($scope, $log, $state, $stateParams, $q); }
            };
        }
        ])
        .directive("slimForm",
            function ($stateParams,
                $timeout,
                $state,
                $resource,
                genericCollectionFactory,
                $http,
                $rootScope,
                $log,
                appSettings) {
                return {
                    restrict: "AEC",
                    transclude: { 'header': "?slimFormHeader", },
                    templateUrl: appSettings.config.resources.baseUri + "js/shared/slimForm/templates/slimform.html" + appSettings.config.app.version.tag,
                    link: function ($scope, element, attributes) {
                        return baseFormLink(
                            $scope,
                            $stateParams,
                            $timeout,
                            $state,
                            $resource,
                            genericCollectionFactory,
                            $http,
                            $rootScope,
                            $log,
                            element, attributes
                        );
                    }
                };
            }
        )
})();