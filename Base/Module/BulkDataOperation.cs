using System.Collections.Concurrent;
using System.Collections.Generic;
using Zen.Base.Module.Data;

namespace Zen.Base.Module
{
    public class BulkDataOperation<T> where T : Data<T>
    {
        public ConcurrentDictionary<string, DataOperationControl<T>> Control = new ConcurrentDictionary<string, DataOperationControl<T>>();
        public List<T> Failure;
        public List<T> Success;
        public EActionType Type;
    }
}