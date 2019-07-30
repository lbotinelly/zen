using System.Collections.Generic;

namespace Zen.App.Provider.Person
{
    public interface IZenPersonProfile : IZenPersonBase
    {
        List<string> Permissions { get; set; }

        void FromPerson(IZenPerson person);
    }
}