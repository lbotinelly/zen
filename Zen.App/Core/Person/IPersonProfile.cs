using System.Collections.Generic;

namespace Zen.App.Core.Person
{
    public interface IPersonProfile : IPersonBase
    {
        List<string> Permissions { get; set; }

        void FromPerson(IPerson person);
    }
}