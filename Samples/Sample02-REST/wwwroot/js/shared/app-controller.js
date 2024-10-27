angular.module('spa')
    .controller('mainCtrl', function ($scope, appSettings, menuService) {
        $scope.appConfig = appSettings.config;
        $scope.menuConfig = menuService.getMenu();
    })
    .controller('landingCtrl', function ($scope, $http, toast) {
        $scope.data = {};
        $scope.actions = {};

        $scope.concatenate = function (str, options) {

            if (!str) return null;

            return str?.replace(/(?:^\w|[A-Z]|\b\w)/g, function (word, index) {

                if (options?.lowerCamelCase)
                    return index === 0 ? word.toLowerCase() : word.toUpperCase();

                if (options?.upperCamelCase)
                    return word.toUpperCase();

                if (options?.lowerCase)
                    return word.toLowerCase();

                return word;

            }).replace(/\s+/g, '');
        }

        $scope.copyToClipboard = function (text_to_share) {

            console.log('>>>', text_to_share);

            toast.create({
                timeout: 5 * 1000,
                message: 'Hi there!',
                className: 'alert-success',
                dismissible: true
            });

            // create temp element
            var copyElement = document.createElement("span");
            copyElement.appendChild(document.createTextNode(text_to_share));
            copyElement.id = 'tempCopyToClipboard';
            angular.element(document.body.append(copyElement));

            // select the text
            var range = document.createRange();
            range.selectNode(copyElement);
            window.getSelection().removeAllRanges();
            window.getSelection().addRange(range);

            // copy & cleanup
            document.execCommand('copy');
            window.getSelection().removeAllRanges();
            copyElement.remove();

        }

        $scope.replacer = function (tpl, data) {
            return tpl.replace(/\$\(([^\)]+)?\)/g, function ($1, $2) {
                return data[$2] ?? '';
            });
        }

        $scope.cleanObject = function (obj) {
            for (var propName in obj) {
                if (obj[propName] === null || obj[propName] === undefined) {
                    delete obj[propName];
                }
            }
            return obj;
        }
    })
