using System.Collections.Generic;
using Zen.App.Core.Group;

namespace Zen.App.Core.Application
{
    public interface IApplicationRepresentation : IApplicationBase
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        List<ZenGroupAction> Groups { get; set; }
    }
}