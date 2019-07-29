using System.Collections.Generic;
using Zen.App.Provider.Person;

namespace Zen.App.Provider.Group {
    public interface IZenGroupAction : IZenGroupBase
    {
        List<IZenPersonAction> People { get; set; }
    }
}