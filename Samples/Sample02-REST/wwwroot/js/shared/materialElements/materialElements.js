(function () { // Immediately-invoked function

    var dbg = true;

    if (typeof module !== "undefined" &&
        typeof exports !== "undefined" &&
        module.exports === exports) {
        module.exports = "materialElements";
    }

    angular
        .module("materialElements", [])
        .service("materialElementsHelper", ["$parse", "$timeout", "$rootScope", function ($parse, $timeout, $rootScope) {
            var formatDefinition = {
                "Select": { controllerId: "materialFieldSelectController" },
                "Image": { controllerId: "materialFieldImageController" },
                "Geoposition": { controllerId: "materialFieldGeopositionController" },
            };

            this.linkHandler = function (scope, element, attrs, ctrl) {

                scope._loading = true;

                var deregisterStateChangeStart = $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {

                    if (scope.debugMode) console.warn(scope.elemId, "$stateChangeStart", toParams, fromParams);

                    delete scope.localModel;
                    delete scope.modelCopy;
                    ctrl.$setPristine();
                    scope._loading = true;

                    deregisterStateChangeStart();
                });

                scope.sourceCtrl = ctrl;

                //warm-up

                var fieldId = attrs.ngModel
                    .replace(/\./g, "_")
                    .replace(/__/g, "_")
                    .toLowerCase();

                var comp = scope.$parent.comparison;

                if (comp)
                    if (attrs.ngModel.indexOf(comp.source) === 0) {

                        var origObjName = "$parent." + comp.original + attrs.ngModel.substring(comp.source.length);

                        scope.$watch("$parent.data.original", function (i) {

                            var origContent = scope.$eval(origObjName);

                            if (origContent) {
                                scope.modelCopy = origContent;
                                scope.modelType = (typeof scope.modelCopy);
                            }
                        });
                    }

                var idx = (scope.$parent.$index ||
                    scope.$parent.$parent.$index ||
                    scope.$parent.$parent.$parent.$index ||
                    scope.$parent.$parent.$parent.$parent.$index);

                if (idx) fieldId = fieldId + idx;

                if (scope.debugMode) console.log("fieldId", fieldId);

                scope.elemId = "meId_" + fieldId;

                scope.attrs = attrs;

                scope.required = "required" in attrs;

                if (!("compare" in attrs)) {
                    scope.compare = true;
                }

                scope.multiple = "multiple" in attrs;
                scope.debugMode = "debugMode" in attrs;

                if ("filterSetting" in attrs) scope.filterSetting = attrs.filterSetting;
                if ("defaultValue" in attrs) scope.defaultValue = scope.$eval(attrs.defaultValue);
                if ("iconSet" in attrs) scope.iconSet = scope.$eval(attrs.iconSet);
                if ("ngMin" in attrs) scope.ngMin = attrs.ngMin;
                if ("ngMax" in attrs) scope.ngMax = attrs.ngMax;
                if ("ngMaxlength" in attrs) scope.ngMaxlength = attrs.ngMaxlength;
                if ("ngMinlength" in attrs) scope.ngMinlength = attrs.ngMinlength;
                if ("ngPattern" in attrs) scope.ngPattern = attrs.ngPattern;

                scope.format = attrs.format;

                scope.processor = {
                    display: function (source) { return "[" + source + "]"; }
                };

                scope.readOnly = "readonly" in attrs;

                scope.modeSource = attrs.mode || "auto";

                var tmpPlaceholder = (scope.label || attrs.ngModel);
                if (!scope.placeholder) scope.placeholder = "Enter " + tmpPlaceholder;

                var descrType = function (obj) {
                    return ({}).toString.call(obj).match(/\s([a-zA-Z]+)/)[1].toLowerCase();
                };

                if (scope.debugMode) console.log("Link for ", attrs.format, scope.elemId, " DONE");

                // Parsers change how view values will be saved in the model.
                ctrl.$parsers.push(function (val) {

                    if (scope.debugMode) console.log(scope.elemId, "$parsers", val, descrType(val), scope.indexType, ctrl.$parsers);

                    if (val === "") val = null;

                    if (scope.indexType === "number") {
                        return parseInt(val, 10);
                    }

                    return val;
                });

                // Formatters change how model values will appear in the view.
                ctrl.$formatters.push(function (val) {

                    if (scope.debugMode) console.log(scope.elemId, "$formatters", val, descrType(val), scope.indexType, ctrl.$formatters);

                    if (val === null) return undefined;

                    if (scope.indexType === "number") return "" + val;
                    return val;
                });

                scope.setModel = function (model) {
                    ctrl.$modelValue = model;
                    scope.ngModel = model;
                }

                scope.clear = function () {

                    if (scope.multiple) {
                        scope.localModel = [];

                    } else {
                        scope.localModel = null;
                    }

                    if (scope.onClear) {
                        $timeout(function () { scope.onClear(); }, 0);
                    }
                };

                scope.setFilter = function (display) {
                    if (scope.filterSetting)
                        if (scope.$parent.$parent.$parent.filterHandler) {
                            var q = {
                                label: scope.label,
                                display: display || scope.localModel,
                                filter: {}
                            };

                            q.filter[scope.filterSetting] = scope.localModel;

                            scope.$parent.$parent.$parent.filterHandler.add(q);
                        }
                };

                scope.ensureModelSet = function () {
                    if (scope.debugMode) console.log(scope.elemId, "ensureModelSet");

                    $timeout(function () {
                        scope.setModel(scope.localModel);
                    }, 0);
                }

                scope.getFilteredTags = function (q) {
                    return scope.selectSource.filter(i => i[scope.displayProperty].toLowerCase().indexOf(q.toLowerCase()) > -1);
                }

                scope.newGuid = function () {
                    function s4() {
                        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
                    }

                    return s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4();
                };

                scope.$watch("modeSource", function () {
                    if (scope.modeSource !== "auto") scope.mode = scope.modeSource || "edit";

                    scope.isEdit = (scope.mode === "edit");
                    scope.isView = !scope.isEdit;
                });
                scope.$watch("$parent.state.mode", function (val) {
                    if (scope.modeSource === "auto") scope.mode = val || "edit";
                }, true);

                scope.$watch("mode",
                    function (val) {
                        scope.isEdit = (val === "edit");
                        scope.isView = !scope.isEdit;
                    },
                    true);

                scope.$watch("switchStateList",
                    function (v) {
                        if (v == null) return;
                        scope.switchStates = angular.fromJson(v);
                    });

                scope.$watch(function () { return ctrl.$modelValue; }, function (newVal) {

                    if (scope.debugMode) console.log(scope.elemId, "$watch", "$modelValue", newVal);

                    // console.log("scope.$watch", newVal, ctrl.$modelValue, ctrl.$viewValue, scope.localModel);
                    if (newVal != undefined) {
                        if (typeof scope.localModel != "undefined")
                            if (scope.onChange) scope.onChange(ctrl.$modelValue);

                        if (ctrl.$dirty) return;

                        if (scope.indexType === "number") {
                            if (newVal !== null) newVal = "" + newVal;
                        }

                        if (scope.localModel !== newVal) {
                            scope.localModel = newVal;
                            ctrl.$setPristine();
                            ctrl.$render();

                            if (scope.compare)
                                if (!scope.modelCopy) {
                                    scope.modelCopy = angular.fromJson(angular.toJson(newVal));
                                }
                        }
                    }
                });

                //set the value of ngModel when the local date property changes
                scope.$watch("localModel", function (newVal, oldVal) {

                    if (scope._loading) return;

                    if (scope.debugMode) console.log("localModel", newVal, oldVal);
                    ctrl.$setViewValue(newVal);
                    ctrl.$render();
                });

                scope._loading = false;
            };
        }
        ])
        .directive("materialField",
            function (appSettings) {
                return {
                    restrict: "AEC",
                    transclude: true,
                    templateUrl: appSettings.config.resources.baseUri + "js/shared/materialElements/templates/materialField.html" + appSettings.config.app.version.tag,
                    controller: ["$scope", function ($scope) { $scope.canShowContent = function () { return $scope.mode === "edit" || !!$scope.localModel || !!$scope.switchStates; }; }]
                };
            })
        .directive("materialInput",
            [
                "materialElementsHelper", "appSettings", function (materialElementsHelper, appSettings) {
                    return {
                        restrict: "AEC",
                        replace: true,
                        require: "?^ngModel", // There may be no model attached, so allow for null reference.
                        scope: {
                            onSelect: "&",
                            label: "@?",
                            type: "@?",
                            ngMin: "@?",
                            ngMax: "@?",
                            ngModel: "=",
                            localModel: "@?",
                            placeholder: "@?",
                            selectSource: "=?",
                            keyProperty: "@?",
                            displayProperty: "@?",
                            elementClassProperty: "@?",
                            switchStateList: "@?",
                            compare: "=",
                            onChange: "=",
                            onClear: "&?",
                            onIconClick: "&?"
                        },
                        templateUrl: function (e, a) {
                            return appSettings.config.resources.baseUri + "js/shared/materialElements/templates/material" + a.format + ".html" + appSettings.config.app.version.tag;
                        },
                        link: materialElementsHelper.linkHandler,
                        controller: materialElementsHelper.controllerHandler
                    };
                }
            ])
        .directive("materialCollection",
            [
                "materialElementsHelper", "appSettings", function (materialElementsHelper, appSettings) {
                    return {
                        restrict: "AEC",
                        replace: true,
                        transclude: {
                            'actions': "?materialCollectionActions",
                            'label': "?materialCollectionLabel",
                        },
                        require: "^ngModel",
                        scope: {
                            onSelect: "&",
                            label: "@?",
                            type: "@?",
                            placeholder: "@?",
                            selectSource: "=?",
                            keyProperty: "@?",
                            displayProperty: "@?",
                            switchStateList: "@?",
                            stateDescriptor: "=?",
                            newModel: "=?",
                            newCollection: "=?",
                            $source: "=?ngModel",
                            ngModel: "=?ngModel",
                            onEnter: "&",
                            onDelete: "&",
                            canDelete: "@?",
                            onNewEntry: "&",
                        },

                        templateUrl: appSettings.config.resources.baseUri + "js/shared/materialElements/templates/materialCollection.html" + appSettings.config.app.version.tag,
                        link: materialElementsHelper.linkHandler,
                        controller: ["$scope", "$attrs", function ($scope, $attrs) {
                            $scope.attributes = $attrs;

                            $scope.static = "staticCollection" in $attrs;
                            $scope.focusItem = null;
                            $scope.getItemMode = function (i) { return $scope.focusItem === i ? "edit" : "view"; };

                            $scope.setFocusItem = function (i) {
                                if ($scope.focusItem) delete $scope.focusItem.__colSelected;

                                $scope.focusItem = i;
                                $scope.$selectedItem = i;

                                if (i) i.__colSelected = true;
                            };

                            $scope.validate = function () {
                                if ($scope.onEnter) {

                                    if ($scope.focusItem) $scope.onEnter({ source: $scope.focusItem });
                                    else $scope.onEnter();
                                }
                                $scope.setFocusItem();
                            };

                            $scope.addNewItem = function () {
                                $scope.validate();
                                var newObjSrc = $scope.attrs.newModel;

                                var newObj = JSON.parse(newObjSrc);
                                $scope.setFocusItem(newObj);

                                $scope.localModel.push(newObj);

                                if ($scope.onNewEntry) $scope.onNewEntry({ source: newObj });
                            };

                            $scope.removeItem = function (i, model) {
                                console.log("REMOVING: TRY ", i, model);

                                if ($scope.onDelete && 0) {
                                    console.log("REMOVING: HAS CUSTOM DELETE", $scope.onDelete, $attrs);
                                    if (!$scope.onDelete({ source: model })) return;
                                }

                                console.log("REMOVING ", i);
                                $scope.internalRemoveItem(i);
                            };

                            $scope.internalRemoveItem = function (i) { $scope.localModel.splice(i, 1); };
                        }
                        ]

                    };
                }
            ])
        .directive("materialSetSource", function () {
            return function (scope, element, attr) {

                scope.materialSet = {
                    actions: {
                        add: function () { console.log("ADDING!"); },
                        del: function (index) {
                            scope.materialSet.source.splice(index, 1);
                        },
                        moveUp: function (index) {
                            var temp = scope.materialSet.source[index];
                            scope.materialSet.source[index] = scope.materialSet.source[index - 1];
                            scope.materialSet.source[index - 1] = temp;
                        },
                        moveDown: function (index) {
                            var temp = scope.materialSet.source[index];
                            scope.materialSet.source[index] = scope.materialSet.source[index + 1];
                            scope.materialSet.source[index + 1] = temp;
                        }
                    }
                };

                scope.materialSet.source = scope.$eval(attr.materialSetSource);

                console.log(scope.materialSet.source);
            };
        })
        .controller("materialFieldSelectController",
            [
                "$scope", "$filter", function ($scope, $filter) {
                    $scope.$watch("selectSource",
                        function (newVal) {
                            if (!newVal) return;

                            if (newVal.length > 0) {

                                var probe = newVal[0];

                                var isScalar = false;

                                $scope.valueType = (typeof probe);

                                if (typeof probe === "string" || probe instanceof String) isScalar = true;
                                if (typeof probe === "number" || probe instanceof Number) isScalar = true;

                                if (isScalar) {
                                    $scope.arraySource = true;
                                    $scope.displayProperty = ("v");
                                    $scope.keyProperty = ("v");
                                } else {
                                    $scope.displayProperty = ($scope.displayProperty || "item");
                                    $scope.keyProperty = ($scope.keyProperty || "item");
                                }

                                if ($scope.keyProperty !== "item") {
                                    $scope.keyPropertyLocal = $scope.keyProperty.replace("item.", "");
                                    $scope.displayPropertyLocal = $scope.displayProperty.replace("item.", "");
                                    var itemProbe = probe[$scope.keyPropertyLocal];
                                    if (itemProbe) $scope.indexType = (typeof itemProbe);
                                }
                            }
                        });

                    $scope.$watchGroup(["localModel", "selectSource"],
                        function (i) {
                            if ($scope.arraySource) return;
                            if (!$scope.localModel) return;
                            if (!$scope.selectSource) return;

                            var fo = {};

                            if ($scope.localModel) $scope.localModel = $scope.localModel + '';

                            fo[$scope.keyPropertyLocal] = $scope.localModel;
                            var fi = $filter("filter")($scope.selectSource, fo);
                            if (fi) $scope.selectedItem = fi[0];
                        });
                }
            ])
        .controller("materialFieldDateTimeController",
            [
                "$scope", "$filter", function ($scope, $filter) {
                    $scope.$watchGroup(["localModel"],
                        function (i) {
                            if ($scope.localModel) {
                                if (!$scope.localDatePart) $scope.localDatePart = new Date(i);
                                if (!$scope.localTimePart) $scope.localTimePart = new Date(i);
                            }

                            if ($scope.localModel === null) {
                                $scope.localDatePart = null;
                                $scope.localTimePart = null;
                            }
                        });

                    $scope.$watchGroup(["localDatePart", "localTimePart"],
                        function (i) {
                            var refDate = null;
                            var mustSet = false;

                            if ($scope.localModel) {
                                refDate = new Date($scope.localModel);
                            } else {
                                refDate = new Date();
                            }

                            if ($scope.localTimePart) {
                                mustSet = true;
                                refDate.setHours($scope.localTimePart.getHours());
                                refDate.setMinutes($scope.localTimePart.getMinutes());
                                refDate.setSeconds(0);
                            }

                            if ($scope.localDatePart) {
                                mustSet = true;
                                refDate.setFullYear($scope.localDatePart.getFullYear());
                                refDate.setMonth($scope.localDatePart.getMonth());
                                refDate.setDate($scope.localDatePart.getDate());
                            }

                            if (mustSet) $scope.localModel = refDate;
                        });
                }
            ])
        .run(["$templateCache", "$http", "appSettings", function ($templateCache, $http, appSettings) {
            $http.get(appSettings.config.resources.baseUri + "js/shared/materialElements/templates/materialSet.html" + appSettings.config.app.version.tag)
                .then(function (response) { $templateCache.put("materialSet.html", response.data); },
                    function (errorResponse) { console.log("Cannot load the file template"); });
        }]);
})();