// Routes
angular.module('spa')
    .config(function ($stateProvider, menuServiceProvider) {

        menuServiceProvider
            .register({
                label: "People",
                sref: "person",
                icon: "zmdi-city"
            });

        $stateProvider
            .state('person', {
                parent: "landing",
                url: "^/person",
                templateUrl: "state/person/collection.html",
                controller: "personCollectionCtrl",
            })
            .state('person.detail', {
                url: "/:id/:verb",
                params: { verb: "view" },
                templateUrl: "state/person/model.html",
                controller: "personModelCtrl"
            });
    });

// Controllers
angular.module('spa')

    .service("personDataService",
        [
            "genericCollectionFactory", function (genericCollectionFactory) {
                var factory = genericCollectionFactory.register(
                    {
                        collection: "personData",
                        endpoint: "person/:id",
                        buffered: false,
                        paged: false
                    });

                this.data = function (locator) { return factory.data(locator); };
                this.resource = factory.resource();
                this.factory = factory;
            }
        ])

    .controller('personCollectionCtrl', function ($scope, personDataService) {


        $scope.config = {
            title: "People",
            isFullList: false,
            readOnly: true,
            data: {
                endpoint: "person/:id",
                locator: "Id",
                title: "Email",
                defaultSort: "Name",
                paged: true,
                collectionFactory: personDataService.factory
            },
            form: {
                labels: {
                    title: { hide: true },
                    count: { hide: true }
                },
                model: { header: { hide: true } }
            },
            sort: {},
            actions: {
                asButtons: true,
                edit: { items: {} },
                view: { items: {} },
                del: { items: {} }
            },
            noHeader: true
        };
    })
    .controller('personModelCtrl', function ($scope, toast, $location) {

        $scope.options = {
            templates: {
                json: {
                    name: 'JSON',
                    model: {
                        ContentType: 'application/json',
                        StatusCode: '200',
                        Payload: '{}',
                        Headers: '{}'

                    }
                },
                xml: {
                    name: 'XML',
                    model: {
                        ContentType: 'text/xml',
                        StatusCode: '200',
                        Payload: '<?xml version="1.0" ?>',
                        Headers: '{}'

                    },
                },
                moved: {
                    name: 'Moved',
                    model: {
                        ContentType: 'application/json',
                        StatusCode: '301',
                        Payload: '',
                        Headers: "{'Location':'https://example.org'}"
                    },
                },
            },
        };

        $scope.encodeURIComponent = function (i) {
            return encodeURIComponent(i);
        }


        $scope.applyTemplate = function (template) {

            angular.merge($scope.data.item, template);
        };
    });