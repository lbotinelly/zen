var bootstrapper = {
    loadDependencies: function (dependencies) {

        for (let i = 0; i < dependencies.scripts.length; i++) {

            var scriptElement = dependencies.scripts[i];
            const node = document.createElement('script');


            if (!!scriptElement.src) {
                node.src = scriptElement.src;
                //node.integrity = scriptElement.integrity;
                node.crossorigin = scriptElement.crossorigin;
                node.referrerpolicy = scriptElement.referrerpolicy;
            }
            else {
                node.src = scriptElement;
            }

            console.log(node.src);

            node.type = 'text/javascript';
            node.async = false;
            document.getElementsByTagName('body')[0].appendChild(node);
        }

        for (let i = 0; i < dependencies.css.length; i++) {

            var stylesheetElement = dependencies.css[i];
            const node = document.createElement('link');
            node.rel = 'stylesheet';


            if (!!stylesheetElement.href) {
                node.href = stylesheetElement.href;
                //node.integrity = stylesheetElement.integrity;
                node.crossorigin = stylesheetElement.crossorigin;
                node.referrerpolicy = stylesheetElement.referrerpolicy;
            }
            else {
                node.href = stylesheetElement;
            }

            console.log(node.href);
            node.async = false;
            document.getElementsByTagName('head')[0].appendChild(node);
        }

    },
    start: function () {
        angular.element(document).ready(function () {
            angular.bootstrap(document, ["spa"]);
        });
    }
}