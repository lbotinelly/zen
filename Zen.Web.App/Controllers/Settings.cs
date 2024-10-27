using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet]
        public JavaScriptResult SettingsVariableJavaScript(string variableName) => JavaScriptSettingsPayload(variableName);

        internal JavaScriptResult JavaScriptSettingsPayload(string variableName)
        {

            Base.Log.KeyValuePair(nameof(JavaScriptSettingsPayload),"New Instance");


            var zen_ver = "";

            if (Request.Cookies.ContainsKey("_zen_ver"))
                zen_ver = Request.Cookies["_zen_ver"];


            var currVer = Base.Host.ApplicationAssembly.GetName().Version.ToString();

            if (zen_ver != currVer)
            {
                Response.Cookies.Append("_zen_ver", currVer, new CookieOptions { IsEssential = true, Secure = true, Expires = new DateTimeOffset(2038, 1, 1, 0, 0, 0, TimeSpan.FromHours(0)) });
                Response.Headers["Clear-Site-Data"] = "cache";

                Base.Log.KeyValuePair("Client Zen Version", zen_ver + " > " + currVer);
            }

            var preRet = $"document.{variableName} = {App.Settings.InternalGetFrameworkSettings()};";

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