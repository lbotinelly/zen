using System.Collections.Generic;
using Zen.App.Core.Person;

namespace Zen.App.Core.Group {
    public interface IGroupAction : IGroupBase
    {
        List<IPersonAction> People { get; set; }
    }
}