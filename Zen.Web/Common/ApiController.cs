using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Common
{
    [Controller]
    public abstract class ApiController
    {
        [ActionContext]
        public ActionContext ActionContext { get; set; }
    }
}