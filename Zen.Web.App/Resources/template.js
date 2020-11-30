var loc = window.location.href + "";

if (loc.indexOf("http://") === 0) {
    window.location.href = loc.replace("http://", "https://");
} else {

    function injectJavascript(res) {
        document.write('\x3Cscript src="' + applyQuery(res, globalAppSettings.vTag) + '">\x3C/script>');
    };

    function injectPreconnect(res) {
        document.write('\x3Clink rel="preconnect" href="' + applyQuery(res, globalAppSettings.vTag) + '" />');
    };

    function injectCss(res) {
        document.write('\x3Clink href="' + applyQuery(res, globalAppSettings.vTag) + '" rel="stylesheet" />');
    };

    function applyQuery(url, query) {
        if (url.indexOf("?") > -1) {
            query = query.replace("?", "&");
        }

        var targetUrl = url + query;

        console.log(targetUrl);
        return targetUrl;
    };
}

[
    globalAppSettings.ResourceUri + "frontend/globalResourceTemplate.js"
].forEach(injectJavascript);
