using System;
using System.Collections.Generic;
using System.Text;
using Zen.App.Model.Audience;
using Zen.Base;

namespace Zen.App.Model.Communication
{
    public interface IScopedContent
    {
        // string Id { get; set; }
        DateTime CreationTime { get; set; }
        string Author { get; set; }
        AudienceDefinition Audience { get; set; }
        bool Active { get; set; }
    }
}
