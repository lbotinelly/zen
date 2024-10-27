angular.module('spa')
    .filter('stringConcat', function () {
        return function (input, delimiter, size) {
            if (input) {

                var temp = input;
                if (size) temp = temp.slice(0, size);
                return temp.join(delimiter)
            }
            else {
                return '';
            }
        };
    })
    .filter("trust", ['$sce', function ($sce) {
        return function (htmlCode) {
            return $sce.trustAsHtml(htmlCode);
        }
    }])
    .filter("OmniQuickSearch",
        function () {
            return function (source, text) {
                if (!source) return false;
                if (!text) return true;

                return source.filter(function (item) {
                    if (!item._compiledContent) {

                        var ctx = "";

                        for (var key in item) {
                            if (item.hasOwnProperty(key)) {
                                if (typeof item[key] !== "object") ctx = ctx + item[key] + " ";
                            }
                        }

                        item._compiledContent = ctx.toLowerCase();
                    }

                    text = text.toLowerCase();

                    return item._compiledContent.indexOf(text) > -1;
                });
            };
        })
    .filter("genericQuickSearch",
        function () {
            return function (source, text) {
                if (!source) return false;
                if (!text) return source;

                text = text.toLowerCase();

                return source.filter(function (item) {
                    const canCache = (item === Object(item));

                    if (!canCache) return item.toLowerCase().indexOf(text) > -1;

                    if (!item.$_compiledJSON) item.$_compiledJSON = JSON.stringify(item).toLowerCase();
                    return item.$_compiledJSON.indexOf(text) > -1;
                });
            };
        })
    .filter("nl2p", function () {
        return function (text) {
            text = String(text).trim();
            return (text.length > 0 ? "<p>" + text.replace(/[\r\n]+/g, "</p><p>") + "</p>" : null);
        };
    })
    .filter("nl2br", function () {
        return function (text) {
            text = String(text).trim();

            if (text.length > 0) text = text.replace(/(?:\r\n|\r|\n)/g, "<br />");

            return text;
        };
    })
    .filter("unsafe", ["$sce", function ($sce) { return $sce.trustAsHtml; }])
    .filter("jsonObject", function () {

        return function (i) {
            if (Object.prototype.toString.call(i) !== "[object String]") return i;
            return angular.fromJson(i);
        }
    })

    .filter("prettyJson", function () {
        return function (i) {
            return angular.toJson(i, true);
        }
    })
    .filter('prettyJson2', [function () {
        return function (obj, pretty) {
            if (!obj) {


                return '';
            }
            var json = angular.toJson(obj, pretty);
            if (!pretty) {
                return json;
            }
            return json
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
                    var cls;
                    if (/^"/.test(match)) {
                        cls = /:$/.test(match) ? 'key' : 'string';
                    } else if (/true|false/.test(match)) {
                        cls = 'boolean';
                    } else if (/null/.test(match)) {
                        cls = 'null';
                    } else {
                        cls = 'number';
                    }
                    return '<span class="json-' + cls + '">' + match + '</span>';
                });
        };
    }])
    ;