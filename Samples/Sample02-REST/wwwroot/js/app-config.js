angular.module('spa')
    .config(function ($urlRouterProvider, appSettingsProvider) {
        appSettingsProvider.config.app.name = "Sample02";

        $urlRouterProvider.otherwise("/person");

    });