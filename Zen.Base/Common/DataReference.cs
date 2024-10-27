using Zen.Base.Module;

namespace Zen.Base.Common
{
    public class DataReference
    {
        public string Display;
        public string Key;

        #region Overrides of Object

        public override string ToString() { return Display.Equals(Key) ? Key : $"[{Key}] {Display}"; }

        #endregion
    }

    public class DataReference<T> : DataReference where T : Data<T>
    {
        public T GetModel() { return Key == null ? null : Data<T>.Get(Key); }
    }
}