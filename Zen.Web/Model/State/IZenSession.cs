using System;
using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.Model.State
{
    public interface IZenSession: IDataId
    {
        DateTime? Creation { get; set; }
        DateTime? LastUpdate { get; set; }
        IDictionary<string, byte[]> Store { get; set; }
    }
}