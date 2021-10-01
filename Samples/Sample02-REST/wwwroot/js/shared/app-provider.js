angular.module('spa')
    .provider("appSettings", function () {
        this.config = {
            app: {
                version: { tag: "?v=.1" }
            },
            features: {
                swagger: true,
                serviceNow: {
                    link: "https://geisingerprod.service-now.com/nav_to.do?uri=%2Fcom.glideapp.servicecatalog_cat_item_view.do%3Fv%3D1%26sysparm_id%3Dd63ef383db0ec74041e073198c9619fc%26sysparm_link_parent%3D680f2716db27878041e073198c96194f%26sysparm_catalog%3De0d08b13c3330100c8b837659bba8fb4%26sysparm_catalog_view%3Dcatalog_Service_Catalog%26sysparm_view%3Dcatalogs_default"
                }
            },
            resources: { baseUri: "" }
        };

        //Only the properties on the object returned
        // from $get are available in the controller.
        this.$get = function () {
            var that = this;
            return {
                config: that.config,
            };
        };
    })
    .provider("menuService",
        function () {

            this.data = {
                items: {
                    all: [], map: [] },
                searchCallback: false,
                currCount: 0
            };

            var that = this;

            this.tryPush = function (item) {
                item.order = item.order || 100; //default position;

                //Eliminate similar srefs
                for (var ii in that.data.items.all) {
                    var i = that.data.items.all[ii];

                    var iKey = (i.sref || "") + (i.click || "");
                    var itemKey = (item.sref || "") + (item.click || "");

                    if (iKey === itemKey) {
                        for (var k in item) i[k] = item[k];
                        return null;
                    }
                }

                that.data.currCount++;
                item.insertionOrder = that.data.currCount;
                that.data.items.all.push(item);
                return item;
            };

            return ({
                register: function (item) {
                    that.tryPush(item);
                    return this;
                },
                setSearchCallback: function (i) { that.data.searchCallback = i; },
                $get: function () {
                    that.data.sortItems = function () {
                        that.data.items.sort(function (a, b) { return a.insertionOrder - b.insertionOrder; });

                        that.data.items.sort(function (a, b) { return a.order - b.order; });
                    };

                    that.data.assembleItemMap = function () {
                        that.data.items.map.length = 0;
                        that.data.group.map = {};

                        angular.forEach(that.data.items,
                            function (item) {
                                if (!items.group) {
                                    that.data.items.map.push(item);
                                } else {
                                    var refGroup = that.data.group.map[items.group];
                                    if (!refGroup) {

                                        var groupObj = {
                                            label: items.group,
                                            isGroup: true,
                                            icon: "zmdi-layers faded",
                                            children: []
                                        };

                                        // console.warn(groupObj);
                                        that.data.group.map[items.group] = groupObj;
                                        that.data.items.map.push(groupObj);
                                        refGroup = groupObj;
                                    }
                                    refGroup.children.push(item);
                                }
                            });
                    };

                    that.data.evaluate = function (item, doNotRefresh) {
                        if (!item) return;

                        //var accept = items.permissionSet ? authService.hasPermission(items.permissionSet) : true;
                        var accept = true;

                        if (accept) {
                            that.data.items.all.push(item);
                            if (!doNotRefresh) that.data.sortItems();
                            that.data.assembleItemMap();
                        }
                    };

                    that.data.evaluateAll = function () {
                        that.data.items.all.length = 0;

                        angular.forEach(that.data.items.all, function (item) { that.data.evaluate(item, true); });

                        that.data.sortItems();
                        that.data.assembleItemMap();
                    };

                    //authService.events.onChange(null, function () { that.data.evaluateAll(); });

                    return {
                        // Normal usage during Run() stage
                        register: function (item) { that.data.evaluate(that.tryPush(item)); },
                        evaluateAll: that.data.evaluateAll,
                        items: that.data.items,
                        showSearch: that.data.showSearch,
                        itemMap: that.data.itemMap,
                        search: false,
                        getMenu: function () {
                            return that;
                        }
                    };
                }

            });
        })
    ;