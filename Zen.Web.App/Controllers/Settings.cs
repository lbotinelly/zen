using Microsoft.AspNetCore.Mvc;
using Zen.Web.App.Properties;
using Zen.Web.Common.Results;

namespace Zen.Web.App.Controllers
{
    [Route("api/app/settings")]
    public class Settings : ControllerBase
    {
        private static string _templateLoad;

        [Route("{variableName}/js")]
        [Route("{variableName}.js")]
        [HttpGet]
        public JavaScriptResult SettingsVariableJavaScript(string variableName) => JavaScriptSettingsPayload(variableName);

        internal JavaScriptResult JavaScriptSettingsPayload(string variableName)
        {
            var preRet = variableName + " = " + App.Settings.InternalGetFrameworkSettings() + ";";

            return new JavaScriptResult(preRet);
        }


        [Route("templateLoader.js")]
        [HttpGet]
        public JavaScriptResult GetTemplateInitialization()
        {
            if (_templateLoad == null) _templateLoad = Resources.template;

            return new JavaScriptResult(_templateLoad);
        }
    }
}