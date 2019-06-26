using Zen.Base.Module.Data.Pipeline.SetVersion.Attributes;

namespace Zen.Base.Module.Data.Pipeline.SetVersion {
    public class ConfigurationHandler<T> where T : Data<T>
    {
        public SetVersionPermissionAttribute Permissions { get; set; }

        public bool CanModify()
        {
            if (Permissions.Modify == null) return false;

            var res = Current.Person?.HasAnyPermissions(Permissions.Modify);
            return res.HasValue && res.Value;
        }
        public bool CanRead()
        {
            if (Permissions.Read == null && Permissions.Modify == null) return true;
            var res = Current.Person?.HasAnyPermissions(Permissions.Read ?? Permissions.Modify);
            return res.HasValue && res.Value;
        }

        public bool CanBrowse()
        {
            if (Permissions.Browse == null && Permissions.Modify == null) return true;
            var res = Current.Person?.HasAnyPermissions(Permissions.Browse ?? Permissions.Modify);
            return res.HasValue && res.Value;


        }
    }
}