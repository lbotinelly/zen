namespace Zen.Base.Common
{
    public class DataReference
    {
        public string Display;
        public string Key;

        #region Overrides of Object

        public override string ToString() { return Display.Equals(Key) ? Key: $"[{Key}] {Display}" ; }

        #endregion
    }
}