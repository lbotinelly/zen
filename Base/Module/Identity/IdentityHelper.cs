using System;

namespace Zen.Base.Module.Identity
{
    public static class IdentityExtensions
    {
        private static readonly char[] _delimiters = { ',', ';', ':', '\n' };

        public static bool HasAnyPermissions(string permissionList)
        {
            if (permissionList == null) return true;
            if (permissionList == "") return true;

            return Current.Authorization.CheckPermission(permissionList.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}