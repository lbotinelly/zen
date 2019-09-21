using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Common.Results
{
    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            Content = script;
            ContentType = "application/javascript";
        }
    }
}