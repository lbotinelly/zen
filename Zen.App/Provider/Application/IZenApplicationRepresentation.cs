using System.Collections.Generic;
using Zen.App.Provider.Group;

namespace Zen.App.Provider.Application
{
    public interface IZenApplicationRepresentation : IZenApplicationBase
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        List<ZenGroupAction> Groups { get; set; }
    }
}