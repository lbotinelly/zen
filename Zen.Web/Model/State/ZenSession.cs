using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.Web.Model.State
{
    [Priority(Level = -99)]
    public class ZenSession : Data<ZenSession>, IZenSession
    {
        [Key]
        public string Id { get; set; }
        public IDictionary<string, byte[]> Store { get; set; }
        public DateTime? Creation { get; set; } = DateTime.Now;
        public DateTime? LastUpdate { get; set; }

        public void Set<T>(string key, T data)
        {
            if (data == null)
                if (Store.ContainsKey(key))
                {
                    Store.Remove(key);
                    return;
                }

            Store[key] = data.ToByteArray();
        }

        public T Get<T>(string key)
        {
            return !Store.ContainsKey(key) ? default : Store[key].FromByteArray<T>();
        }
    }
}