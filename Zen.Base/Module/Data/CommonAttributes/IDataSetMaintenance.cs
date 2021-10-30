using System.Collections.Generic;

namespace Zen.Base.Module.Data.CommonAttributes
{
    public interface IDataSetMaintenance
    {
        void DoCollectionMaintenance<T>(IEnumerable<T> collection ) where T: Data<T>;
    }
}