using Zen.Base.Assembly;
using Zen.Base.Module.Cache;

namespace Zen.Base.Module.Data
{
    public class Info<T> : Data<T> where T : Data<T>
    {
        public string Key { get => GetDataKey(this); set => SetDataKey(value); }
        public string Display { get => GetDataDisplay(this); set => SetDataDisplay(value); }
    }
}