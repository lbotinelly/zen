using System;
using System.Collections.Concurrent;
using Zen.Base.Extension;
using Zen.Base.Module.Data;

namespace Zen.Base.Module
{
    public abstract class Data<T> where T : Data<T>
    {
        #region Static references
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>> ClassRegistration = new ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>>();
        public static Settings Settings => ClassRegistration[typeof(T)].Item1;
        public static DataConfigAttribute TableData => ClassRegistration[typeof(T)].Item2;
        public static string GetIdentifier(Data<T> oRef) { return (oRef.GetType().GetProperty(Settings.IdentifierProperty)?.GetValue(oRef, null) ?? "").ToString(); }
        public static string GetLabel(Data<T> oRef) { return (oRef.GetType().GetProperty(Settings.LabelProperty)?.GetValue(oRef, null) ?? "").ToString(); }
        public static Settings GetSettings(Data<T> oRef) { return ClassRegistration[oRef.GetType()].Item1; }
        #endregion

        #region Instanced references

        internal void SetDataIdentifier(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);
            var refProp = GetType().GetProperty(Settings.IdentifierProperty);
            refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
        }

        internal void SetDataLabel(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);
            var refProp = GetType().GetProperty(Settings.LabelProperty);
            refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
        }

        #endregion
    }
}