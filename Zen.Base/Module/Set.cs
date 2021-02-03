using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Base.Module
{
    public sealed class Set<T> : ISetSave where T : Data<T>, IDataId
    {
        internal Dictionary<string, T> Cache = new Dictionary<string, T>();
        private readonly Dictionary<string, string> _checkSum = new Dictionary<string, string>();

        #region Overrides of Object

        public override string ToString() => $"{typeof(T).Name}: {Cache.Count} items";

        #endregion

        public void Commit() => Save();

        public T Fetch(T referenceModel)
        {
            return Fetch(referenceModel?.Id, referenceModel);
        }

        public T Fetch(string identifier, T model = null, bool ignoreCache = false)
        {

            if (identifier != null)
            {

                if (!ignoreCache)
                    if (Cache.ContainsKey(identifier))
                        return Cache[identifier];
                model = Data<T>.Get(identifier);
            }


            if (model == null)
            {
                model = typeof(T).CreateInstance<T>();
                if (identifier != null) model.SetDataKey(identifier);
            }
            else
            {
                // Store the checksum to avoid commiting pristine copies.
                _checkSum[model.GetDataKey()] = model.ToJson().Md5Hash();
            }

            Store(model);
            return model;
        }

        public T Store(T model)
        {
            Cache[model.GetDataKey()] = model;

            return model;
        }

        public List<T> Save()
        {
            var newModels = Cache.Where(i => !_checkSum.ContainsKey(i.Key)).Select(i=> i.Value).ToList();
            var dirtyModels = Cache.Where(i => _checkSum.ContainsKey(i.Key) && _checkSum[i.Key] != i.Value.ToJson().Md5Hash()).Select(i=> i.Value).ToList();

            var allChanges = new List<T>();

            allChanges.AddRange(newModels);
            allChanges.AddRange(dirtyModels);

            allChanges.Save();
            return allChanges;
        }
    }
}