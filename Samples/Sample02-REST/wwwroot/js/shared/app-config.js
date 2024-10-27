angular.module('spa')
    .config(function ($stateProvider, $httpProvider) {

        $stateProvider
            .state('landing', {
                url: "/",
                views: {
                    "": {
                        templateUrl: "state/index.html",
                        controller: "landingCtrl",
                    }
                }
            });

        $httpProvider.interceptors.push('BearerAuthInterceptor');
    })