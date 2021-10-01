console.log('app declared');

var app = angular.module('spa', ['ui.router', 'ui.bootstrap', 'ngSanitize', 'ngResource', 'angularjsToast', 'slimForm', 'materialElements']);

bootstrapper.loadDependencies(
    {
        scripts: [
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js",
                integrity: "sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q",
                crossorigin: "anonymous",
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/angular-animate/1.8.2/angular-animate.min.js",
                integrity: "sha512-jZoujmRqSbKvkVDG+hf84/X11/j5TVxwBrcQSKp1W+A/fMxmYzOAVw+YaOf3tWzG/SjEAbam7KqHMORlsdF/eA==",
                crossorigin: "anonymous"
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/angular-touch/1.8.2/angular-touch.min.js",
                integrity: "sha512-mCJrnox//whq3scvuyMcN4ON5W8QC6XFLB618pcTbe/EI97qUowpP9Rdo/eQJS5uyOObK/7+tHnyKoBSdgi+Dg==",
                crossorigin: "anonymous"
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/angular-sanitize/1.8.2/angular-sanitize.min.js",
                integrity: "sha512-JkCv2gG5E746DSy2JQlYUJUcw9mT0vyre2KxE2ZuDjNfqG90Bi7GhcHUjLQ2VIAF1QVsY5JMwA1+bjjU5Omabw==",
                crossorigin: "anonymous"
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/angular-ui-router/1.0.29/angular-ui-router.js",
                integrity: "sha512-X4A/kmx6SitMDOtgnd2ijE39awKTnh3CqJ1vVhT8M9u3T+ikdMMGsBP05YHsJTfme2RsU28HyiQ9udRKjgPOuQ==",
                crossorigin: "anonymous",
                referrerpolicy: "no-referrer"
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/angular-resource/1.8.2/angular-resource.js",
                integrity: "sha512-C0Q5cBVjILTWCkbBlo1DhFwqGRhwheapSLmay077jcd4IefTe+kznyTd/cP/hDfL4i7iKI9LZEwTddQY7vWBVA==",
                crossorigin: "anonymous",
                referrerpolicy: "no-referrer",
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/angular-ui-bootstrap/2.5.6/ui-bootstrap-tpls.js",
                integrity: "sha512-Re9KhAaoh5qo/Cm/wtExVs7ETTKTx/81aXPHko2nWlUvTzELYhTwpp/DwUu+z8ul+DjtbJdPcmxEYwKewzG62w==",
                crossorigin: "anonymous"
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js",
                integrity: "sha512-qTXRIMyZIFb8iQcfjXWCO8+M5Tbc38Qi5WzdPOYZHIlZpzBHG3L3by84BBBOiRGiEb7KKtAOAs5qYdUiZiQNNQ==",
                crossorigin: "anonymous",
                referrerpolicy: "no-referrer"
            },
            {
                src: "https://cdnjs.cloudflare.com/ajax/libs/js-yaml/4.1.0/js-yaml.min.js",
                integrity: "sha512-CSBhVREyzHAjAFfBlIBakjoRUKp5h7VSweP0InR/pAJyptH7peuhCsqAI/snV+TwZmXZqoUklpXp6R6wMnYf5Q==",
                crossorigin: "anonymous",
                referrerpolicy: "no-referrer"
            },
            {
                src: "//cdn.jsdelivr.net/npm/angularjs-toast@latest/angularjs-toast.min.js",
                crossorigin: "anonymous"
            },
            'js/shared/app-config.js',
            'js/shared/app-provider.js',
            'js/shared/app-filter.js',
            'js/shared/app-service.js',
            'js/shared/app-controller.js',
            'js/shared/slimForm/slimForm.js',
            'js/shared/materialElements/materialElements.js'
        ],
        css: [
            {
                href: "https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.4.1/css/bootstrap.css",
                integrity: "sha512-mG7Xo6XLlQ13JGPQLgLxI7bz8QlErrsE9rYQDRgF+6AlQHm9Tn5bh/vaIKxBmM9mULPC6yizAhEmKyGgNHCIvg==",
                crossorigin: "anonymous"
            },
            {
                href: "https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.4.1/css/bootstrap-theme.css",
                integrity: "sha512-jLmtg/HHup28rUf0sXLUCyrZVMBvp+tp1kEqYJcSQuG26ytM6oEDn08vg7Scn23UnS59x13IijVJMdR8MJTGNA==",
                crossorigin: "anonymous"
            },
            {
                href: "https://cdnjs.cloudflare.com/ajax/libs/angular-ui-bootstrap/2.5.6/ui-bootstrap-csp.min.css",
                integrity: "sha512-3mC4Q7Z/awACW7Zf0QGvaU8dEXv862RQD6kmpNXTuiUV6X/sdl1QhiiN5z9x/iNpvMFsQ+NBD3TKGrFI3vP0QA==",
                crossorigin: "anonymous"
            },
            "//maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css",
            "//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap-glyphicons.css",
            {
                href: "//cdn.jsdelivr.net/npm/angularjs-toast@latest/angularjs-toast.min.css",
                crossorigin: "anonymous"
            },
            "//cdn.jsdelivr.net/npm/bootswatch@3.4.1/paper/bootstrap.min.css"
        ]
    }
);