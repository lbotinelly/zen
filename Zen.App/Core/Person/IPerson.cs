using System;
using System.Collections.Generic;
using System.Linq;
using Zen.App.Core.Group;

namespace Zen.App.Core.Person
{
    public interface IPerson : IPersonBase
    {
        List<string> Permissions { get; set; }
        List<IGroup> Groups();
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        List<IPerson> ByGroup(string key);
    }

    public static class Extensions
    {
        private static readonly char[] PermissionExpressionDelimiters = { ',', ';', '\n' };
        private static string _IsAuthenticated = "$ISAUTHENTICATED";

        public static bool HasAnyPermissions(this IPerson person, string expression)
        {

            if (string.IsNullOrEmpty(expression)) return true;

            var permissionList = expression.Split(PermissionExpressionDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return person.HasAnyPermissions(permissionList);

        }

        public static bool HasAnyPermissions(this IPerson person, IEnumerable<string> terms)
        {
            terms = terms.ToList();

            if (terms.Contains(_IsAuthenticated)) if (Current.Orchestrator.Person!= null) return true;

            var appCodeMatrix = $"[{Current.Orchestrator.Application.Code}].[{{0}}]";

            var matchingPermissions = terms.Select(i => i.StartsWith('[') ? i : string.Format(appCodeMatrix, i)).ToList();

            return person.Permissions.Intersect(matchingPermissions).Any();

        }
    }
}