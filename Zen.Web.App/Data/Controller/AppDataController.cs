using Zen.Base.Module;
using Zen.Web.Data.Controller;

namespace Zen.Web.App.Data
{
    public abstract class AppDataController<T> : DataController<T> where T : Data<T>
    {
        public override bool CheckPermissions(string permissions) => Zen.App.Current.Orchestrator?.Person?.HasAnyPermissions(permissions) == true;
    }
}
