using System.Collections.Generic;
using Zen.App.Provider.Person;

namespace Zen.App.Provider
{
    public interface IZenPerson : IZenPersonBase
    {
        List<string> Permissions { get; set; }
        List<IZenGroup> Groups();
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        List<IZenPerson> ByGroup(string key);
    }
}