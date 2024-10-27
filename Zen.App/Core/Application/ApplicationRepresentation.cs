using System.Collections.Generic;
using Zen.App.Core.Group;

namespace Zen.App.Core.Application
{
    public class ApplicationRepresentation : IApplicationRepresentation
    {
        public string Id { get; set; }
        public string Locator { get; set; }
        public string Code { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public List<ZenGroupAction> Groups { get; set; }
    }
}