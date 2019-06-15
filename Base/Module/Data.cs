using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Zen.Base.Assembly;
using Zen.Base.Extension;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Data.Pipeline;

#pragma warning disable 693

namespace Zen.Base.Module
{
    public abstract class Data<T> where T : Data<T>
    {
        #region State tools

        private static void ValidateEntityState()
        {
            if (Info<T>.Settings.State.Status != Settings.EStatus.Operational && Info<T>.Settings.State.Status != Settings.EStatus.Initializing)
                throw new Exception($"Class is not operational: {Info<T>.Settings.State.Status}, {Info<T>.Settings.State.Description}");
        }

        #endregion

        public static class Info<T> where T : Data<T>
        {
            public static Settings Settings => ClassRegistration[typeof(T)].Item1;
            public static DataConfigAttribute Configuration => ClassRegistration[typeof(T)].Item2;
        }

        #region Static references

        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>> ClassRegistration
            = new ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>>();

        public static string GetIdentifier(Data<T> oRef) { return oRef == null ? null : (oRef.GetType().GetProperty(Info<T>.Settings.IdentifierProperty)?.GetValue(oRef, null) ?? "").ToString(); }

        public static string GetLabel(Data<T> oRef) { return oRef == null ? null : (oRef.GetType().GetProperty(Info<T>.Settings.LabelProperty)?.GetValue(oRef, null) ?? "").ToString(); }

        private static string _cacheKeyBase;

        public static string CacheKey(string key = "")
        {
            if (_cacheKeyBase != null) return _cacheKeyBase + key;

            _cacheKeyBase = typeof(T) + ":";
            return _cacheKeyBase + key;
        }

        public static string CacheKey(Type t, string key = "") { return t.FullName + ":" + key; }

        #endregion

        #region Instanced references

        public void SetDataIdentifier(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);

            var refField = GetType().GetField(Info<T>.Settings.IdentifierProperty);
            if (refField != null) refField.SetValue(oRef, Convert.ChangeType(value, refField.FieldType));
            else
            {
                var refProp = GetType().GetProperty(Info<T>.Settings.IdentifierProperty);
                refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
            }
        }

        public void SetDataLabel(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);
            var refProp = GetType().GetProperty(Info<T>.Settings.LabelProperty);
            refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
        }

        #endregion

        #region bootstrap

        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly object _accessLock = new object();
        private bool _isDeleted;
        private bool? _cachedIsNew;

        static Data()
        {
            lock (_accessLock)
            {
                try
                {
                    // A new Data<> is born. Let's help it grow into a fully functional ORM connector. 

                    // First we prepare a registry containing all necessary information for it to operate.

                    ClassRegistration.TryAdd(typeof(T),
                                             new Tuple<Settings, DataConfigAttribute>(
                                                 new Settings(),
                                                 (DataConfigAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(DataConfigAttribute))));

                    Info<T>.Settings.State.Status = Settings.EStatus.Initializing;

                    // Do we have a [Key] and a [Display]?

                    Info<T>.Settings.State.Step = "Starting TableData/Statements setup";

                    Info<T>.Settings.StorageName = Info<T>.Configuration?.Label ?? Info<T>.Configuration?.TableName ?? typeof(T).Name;

                    Info<T>.Settings.IdentifierProperty =
                        // If there's a [Key] attribute on a field, use its name;
                        typeof(T).GetFields().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute), true))?.Name ??
                        // If there's a [Key] attribute on a Property, use its name;
                        typeof(T).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute), true))?.Name ??
                        // Otherwise pick from Config
                        Info<T>.Configuration?.IdentifierColumnName;

                    if (Info<T>.Settings.IdentifierProperty == null)
                    {
                        Info<T>.Settings.State.Status = Settings.EStatus.CriticalFailure;
                        Info<T>.Settings.State.Description = "No valid Key property found";
                        return;
                    }

                    Info<T>.Settings.LabelProperty =
                        // If there's a [Key] attribute on a field, use its name;
                        typeof(T).GetFields().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(DisplayAttribute), true))?.Name ??
                        // If there's a [Key] attribute on a Property, use its name;
                        typeof(T).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(DisplayAttribute), true))?.Name ??
                        // Otherwise pick from Config
                        Info<T>.Configuration?.DisplayProperty;

                    // Do we have any pipelines defined?
                    var ps = typeof(T).GetCustomAttributes(true).OfType<PipelineAttribute>().ToList();

                    if (ps.Any())
                    {
                        Info<T>.Settings.Pipelines = new Settings.PipelineQueueHandler
                        {
                            Before = (from pipelineAttribute in ps
                                      from type in pipelineAttribute.Types
                                      where typeof(IBeforeActionPipeline).IsAssignableFrom(type)
                                      select (IBeforeActionPipeline)type.GetConstructor(new Type[] { }).Invoke(new object[] { }))
                                .ToList(),
                            After = (from pipelineAttribute in ps
                                     from type in pipelineAttribute.Types
                                     where typeof(IAfterActionPipeline).IsAssignableFrom(type)
                                     select (IAfterActionPipeline)type.GetConstructor(new Type[] { }).Invoke(new object[] { }))
                                .ToList()
                        };

                        // Now let's report what we've just done.

                        if (Info<T>.Settings.Pipelines.Before.Any()) Info<T>.Settings.Statistics["Settings.Pipelines.Before"] = Info<T>.Settings.Pipelines.Before.Select(i => i.GetType().Name).Aggregate((i, j) => i + "," + j);
                        if (Info<T>.Settings.Pipelines.After.Any()) Info<T>.Settings.Statistics["Settings.Pipelines.After"] = Info<T>.Settings.Pipelines.After.Select(i => i.GetType().Name).Aggregate((i, j) => i + "," + j);
                    }

                    Info<T>.Settings.State.Step = "Determining Environment";

                    // Next step: Record Environment Mapping data, if any.

                    Info<T>.Settings.Statistics["Current.Environment.Current.Code"] = Current.Environment.Current.Code;

                    Info<T>.Settings.EnvironmentMapping = Attribute.GetCustomAttributes(typeof(T), typeof(EnvironmentMappingAttribute))
                        .Select(i => (EnvironmentMappingAttribute)i)
                        .ToList();

                    if (Info<T>.Settings.EnvironmentMapping.Any()) Info<T>.Settings.Statistics["Settings.EnvironmentMapping"] = Info<T>.Settings.EnvironmentMapping.ToJson();

                    Info<T>.Settings.EnvironmentCode =
                        // If a PersistentEnvironmentCode is defined, use it.
                        Info<T>.Configuration?.PersistentEnvironmentCode ??
                        // Otherwise let's check if there's a mapping defined for the current 'real' environment.
                        Info<T>.Settings.EnvironmentMapping?.FirstOrDefault(i => i.Origin == Current.Environment.CurrentCode)?.Target ??
                        // Nothing? Let's just use the current environment then.
                        Current.Environment.CurrentCode;

                    Info<T>.Settings.State.Step = "Setting up Reference Bundle";
                    var refBundle = Info<T>.Configuration?.ConnectionBundleType ?? Current.GlobalConnectionBundleType;

                    if (refBundle == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure, "No valid connection bundle found");
                        return;
                    }

                    var refType = (ConnectionBundlePrimitive)Activator.CreateInstance(refBundle);

                    Info<T>.Settings.Bundle = refType;
                    Info<T>.Settings.Bundle.Validate(ConnectionBundlePrimitive.EValidationScope.Database);

                    if (refType.AdapterType == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure, "No AdapterType defined on bundle");
                        return;
                    }

                    Info<T>.Settings.Adapter = (DataAdapterPrimitive)Activator.CreateInstance(refType.AdapterType);

                    if (Info<T>.Settings.Adapter == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure, "Null AdapterType");
                        return;
                    }

                    Info<T>.Settings.Statistics["Settings.Bundle"] = refType.GetType().Name;

                    Info<T>.Settings.State.Step = "Setting up CypherKeys";
                    Info<T>.Settings.ConnectionCypherKeys = Info<T>.Settings?.ConnectionCypherKeys ?? refType?.ConnectionCypherKeys;

                    Info<T>.Settings.State.Step = "Determining CredentialSets to use";
                    Info<T>.Settings.CredentialSet = Factory.GetCredentialSetPerConnectionBundle(Info<T>.Settings.Bundle, Info<T>.Configuration?.CredentialSetType);

                    if (Info<T>.Settings.CredentialSet != null) Info<T>.Settings.Statistics["Settings.CredentialSet"] = Info<T>.Settings.CredentialSet?.GetType().Name;

                    Info<T>.Settings.CredentialCypherKeys = Info<T>.Configuration?.CredentialCypherKeys ?? Info<T>.Settings.CredentialSet?.CredentialCypherKeys;

                    Info<T>.Settings.Interceptor = Info<T>.Settings.Adapter.Interceptor;

                    // Now we're ready to talk to the outside world.

                    Info<T>.Settings.State.Step = "Checking Connection to storage";

                    Info<T>.Settings.Adapter.SetConnectionString<T>();

                    Info<T>.Settings.Interceptor.Setup<T>(Info<T>.Settings);
                    Info<T>.Settings.Interceptor.Initialize<T>();

                    Current.Environment.EnvironmentChanged += Environment_EnvironmentChanged;

                    foreach (var i in Info<T>.Settings.Statistics) Current.Log.Debug(i.Key + " : " + i.Value);

                    Info<T>.Settings.State.Set<T>(Settings.EStatus.Operational, "Ready");
                }
                catch (Exception e)
                {
                    Info<T>.Settings.State.Status = Settings.EStatus.CriticalFailure;

                    Info<T>.Settings.State.Description = typeof(T).FullName + " ERR " + Info<T>.Settings.State.Step + " : " + e.Message;
                    Info<T>.Settings.State.Stack = new StackTrace(e, true).FancyString();

                    var refEx = e;
                    while (refEx.InnerException != null)
                    {
                        refEx = e.InnerException;
                        Info<T>.Settings.State.Description += " / " + refEx.Message;
                    }

                    Current.Log.Warn(Info<T>.Settings.State.Description);
                }
            }
        }

        #endregion

        #region Events

        private static void Environment_EnvironmentChanged(object sender, EventArgs e)
        {
            //Target environment changed; pick the proper connection strings.

            HandleConfigurationChange();
        }

        private static void HandleConfigurationChange()
        {
            try
            {
                Current.Log.Maintenance(typeof(T) + ": Configuration changed");
                Info<T>.Settings.Adapter.SetConnectionString<T>();
            }
            catch (Exception e) { Current.Log.Add(e); }
        }

        private static T ProcBeforePipeline(EAction action, T current, T source)
        {
            if (current == null) return null;
            if (Info<T>.Settings?.Pipelines?.Before == null) return current;

            foreach (var beforeActionPipeline in Info<T>.Settings.Pipelines.Before)
                try { if (current != null) current = beforeActionPipeline.Process(action, current, source); } catch (Exception e) { Current.Log.Add<T>(e); }

            return current;
        }

        private static void ProcAfterPipeline(EAction action, T current, T source)
        {
            if (Info<T>.Settings?.Pipelines?.After == null) return;

            foreach (var afterActionPipeline in Info<T>.Settings.Pipelines.After)
                try { afterActionPipeline.Process(action, current, source); } catch (Exception e) { Current.Log.Add<T>(e); }
        }



        #endregion

        #region Static Data handlers

        public static IEnumerable<T> All()
        {
            ValidateEntityState();
            return Info<T>.Settings.Interceptor.All<T>();
        }

        public static IEnumerable<TU> All<TU>(string extraParms = null)
        {
            ValidateEntityState();
            return Info<T>.Settings.Interceptor.All<T, TU>(extraParms);
        }

        public static T Get(string identifier)
        {
            if (identifier == null) return null;

            if (Info<T>.Settings.IdentifierProperty == null)
            {
                Current.Log.Warn<T>("Identifier not set");
                throw new MissingPrimaryKeyException("Identifier not set for " + typeof(T).FullName);
            }

            return Info<T>.Configuration?.UseCaching == true ? CacheFactory.FetchSingleResultByKey(GetFromDatabase, identifier) : GetFromDatabase(identifier);
        }

        internal static T GetFromDatabase(object identifier)
        {
            return Info<T>.Settings.Interceptor.Get<T>(identifier.ToString());
        }

        #endregion

        #region Instanced data handlers

        public bool IsNew(ref T oProbe)
        {
            if (_cachedIsNew.HasValue) return _cachedIsNew.Value;

            var probe = GetIdentifier(this);

            if (string.IsNullOrEmpty(probe))
            {
                _cachedIsNew = true;
                return _cachedIsNew.Value;
            }

            oProbe = Get(probe);

            _cachedIsNew = oProbe == null;
            return _cachedIsNew.Value;
        }



        public string Save()
        {
            ValidateEntityState();

            if (Info<T>.Configuration != null)
                if (Info<T>.Configuration.IsReadOnly) throw new Exception("This entity is set as read-only.");

            if (_isDeleted) return null;
            var ret = "";

            T oldRec = null;
            var isNew = IsNew(ref oldRec);

            var rec = this;
            rec = ProcBeforePipeline(isNew ? EAction.Insert : EAction.Update, (T)rec, oldRec);

            if (rec == null) return null;

            BeforeSave();

            Info<T>.Settings.Interceptor.Upsert(rec);

            if (Info<T>.Configuration?.UseCaching == true)
                if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
                {
                    Current.Cache.Remove(CacheKey(ret));
                    var types = Management.GetGenericsByBaseClass(typeof(T));
                    foreach (var t in types)
                    {
                        var key = CacheKey(t, ret);
                        Current.Cache.Remove(key);
                    }
                }

            if (isNew) AfterInsert(ret);
            AfterSave(ret);

            _cachedIsNew = null;

            if (Info<T>.Settings?.Pipelines?.After == null) return ret;

            rec = Get(ret);
            ProcAfterPipeline(isNew ? EAction.Insert : EAction.Update, (T)rec, oldRec);

            return ret;
        }

        #endregion

        #region Event hooks
        public virtual void AfterInsert(string newIdentifier) { }
        public virtual void AfterSave(string newIdentifier) { }
        public virtual void BeforeSave() { }
        public virtual void BeforeInsert() { }
        public virtual void BeforeRemove() { }
        public virtual void AfterRemove() { }
        #endregion
    }
}