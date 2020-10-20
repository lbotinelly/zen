using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Data;

namespace Zen.Base.Module
{
    public sealed class Set<T> : ISetSave where T : Data<T>
    {
        internal Dictionary<string, T> Cache = new Dictionary<string, T>();

        #region Overrides of Object

        public override string ToString()
        {
            return $"{typeof(T).Name}: {Cache.Count} items";
        }

        #endregion

        public void Commit()
        {
            Save();
        }

        public T Fetch(string identifier, bool ignoreCache = false)
        {
            T model = null;

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

            Cache[model.GetDataKey()] = model;
            return model;
        }

        public List<T> Save()
        {
            var tempSet = Cache.Values.ToList();
            tempSet.Save();
            return tempSet;
        }
    }
}