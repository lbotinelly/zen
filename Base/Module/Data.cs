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

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

#pragma warning disable 693

namespace Zen.Base.Module
{
    public abstract class Data<T> where T : Data<T>
    {
        private static string _cacheKeyBase;
        private static readonly object _InitializationLock = new object();
        private bool? _cachedIsNew;
        private bool _isDeleted;

        #region Bootstrap

        static Data()
        {
            lock (_InitializationLock)
            {
                try
                {
                    // A new Data<> is born. Let's help it grow into a fully functional ORM connector. 

                    // First we prepare a registry containing all necessary information for it to operate.

                    ClassRegistration.TryAdd(typeof(T),
                        new Tuple<Settings, DataConfigAttribute>(new Settings(),
                            (DataConfigAttribute)Attribute.GetCustomAttribute(typeof(T),
                                typeof(DataConfigAttribute))));

                    Info<T>.Settings.State.Status = Settings.EStatus.Initializing;

                    // Do we have a [Key] and a [Display]?

                    Info<T>.Settings.State.Step = "Starting TableData/Statements setup";

                    Info<T>.Settings.StorageName =
                        Info<T>.Configuration?.Label ?? Info<T>.Configuration?.TableName ?? typeof(T).Name;



                    Info<T>.Settings.KeyField = typeof(T).GetFields().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute), true));
                    Info<T>.Settings.KeyProperty = typeof(T).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute), true));

                    if (Info<T>.Settings.KeyField == null && Info<T>.Settings.KeyProperty == null && Info<T>.Configuration?.KeyName != null)
                    {
                        // A Key member name was provided. Does it really exist? Let's find out.
                        // (Some drivers need explicit key property declaration, like LiteDB.)

                        Info<T>.Settings.KeyField = typeof(T).GetFields()
                            .FirstOrDefault(i => i.Name == Info<T>.Configuration.KeyName);
                        Info<T>.Settings.KeyProperty = typeof(T).GetProperties()
                            .FirstOrDefault(i => i.Name == Info<T>.Configuration.KeyName);
                    }

                    Info<T>.Settings.KeyMemberName =
                        // If there's a [Key] attribute on a field, use its name;
                        Info<T>.Settings.KeyField?.Name ??
                        // If there's a [Key] attribute on a Property, use its name;
                        Info<T>.Settings.KeyProperty?.Name ??
                        // Otherwise pick from Config
                        Info<T>.Configuration?.KeyName;

                    if (Info<T>.Settings.KeyMemberName == null)
                    {

                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure, "No valid Key member found");
                        return;
                    }

                    if (Info<T>.Settings.KeyField == null && Info<T>.Settings.KeyProperty == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure, $"No member match for Key {Info<T>.Configuration?.KeyName}");
                        return;
                    }

                    Info<T>.Settings.DisplayField = typeof(T).GetFields().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(DisplayAttribute), true));
                    Info<T>.Settings.DisplayProperty = typeof(T).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(DisplayAttribute), true));

                    if (Info<T>.Settings.DisplayField == null && Info<T>.Settings.DisplayProperty == null && Info<T>.Configuration?.DisplayProperty != null)
                    {
                        // A Display member name was provided. Does it really exist? Let's find out.
                        Info<T>.Settings.DisplayField = typeof(T).GetFields().FirstOrDefault(i => i.Name == Info<T>.Configuration.DisplayProperty);
                        Info<T>.Settings.DisplayProperty = typeof(T).GetProperties().FirstOrDefault(i => i.Name == Info<T>.Configuration.DisplayProperty);
                    }

                    Info<T>.Settings.DisplayMemberName =
                        // If there's a [Key] attribute on a field, use its name;
                        Info<T>.Settings.DisplayField?.Name ??
                        // If there's a [Key] attribute on a Property, use its name;
                        Info<T>.Settings.DisplayProperty?.Name;

                    if (Info<T>.Settings.DisplayProperty?.Name != null && Info<T>.Settings.DisplayMemberName == null)
                        Current.Log.Warn<T>($"Mismatched DisplayMemberName: {Info<T>.Settings.DisplayMemberName}. Ignoring.");

                    // Do we have any pipelines defined?
                    var ps = typeof(T).GetCustomAttributes(true).OfType<PipelineAttribute>().ToList();

                    if (ps.Any())
                    {
                        Info<T>.Settings.Pipelines = new Settings.PipelineQueueHandler
                        {
                            Before = (from pipelineAttribute in ps
                                      from type in pipelineAttribute.Types
                                      where typeof(IBeforeActionPipeline).IsAssignableFrom(type)
                                      select (IBeforeActionPipeline)type.GetConstructor(new Type[] { })
                                          .Invoke(new object[] { }))
                                .ToList(),
                            After = (from pipelineAttribute in ps
                                     from type in pipelineAttribute.Types
                                     where typeof(IAfterActionPipeline).IsAssignableFrom(type)
                                     select (IAfterActionPipeline)type.GetConstructor(new Type[] { })
                                         .Invoke(new object[] { }))
                                .ToList()
                        };

                        // Now let's report what we've just done.

                        if (Info<T>.Settings.Pipelines.Before.Any())
                            Info<T>.Settings.Statistics["Settings.Pipelines.Before"] = Info<T>.Settings.Pipelines.Before
                                .Select(i => i.GetType().Name).Aggregate((i, j) => i + "," + j);
                        if (Info<T>.Settings.Pipelines.After.Any())
                            Info<T>.Settings.Statistics["Settings.Pipelines.After"] = Info<T>.Settings.Pipelines.After
                                .Select(i => i.GetType().Name).Aggregate((i, j) => i + "," + j);
                    }

                    Info<T>.Settings.State.Step = "Determining Environment";

                    // Next step: Record Environment Mapping data, if any.

                    Info<T>.Settings.Statistics["Current.Environment.Current.Code"] = Current.Environment.Current.Code;

                    Info<T>.Settings.EnvironmentMapping = Attribute
                        .GetCustomAttributes(typeof(T), typeof(EnvironmentMappingAttribute))
                        .Select(i => (EnvironmentMappingAttribute)i)
                        .ToList();

                    if (Info<T>.Settings.EnvironmentMapping.Any())
                        Info<T>.Settings.Statistics["Settings.EnvironmentMapping"] =
                            Info<T>.Settings.EnvironmentMapping.ToJson();

                    Info<T>.Settings.EnvironmentCode =
                        // If a PersistentEnvironmentCode is defined, use it.
                        Info<T>.Configuration?.PersistentEnvironmentCode ??
                        // Otherwise let's check if there's a mapping defined for the current 'real' environment.
                        Info<T>.Settings.EnvironmentMapping
                            ?.FirstOrDefault(i => i.Origin == Current.Environment.CurrentCode)?.Target ??
                        // Nothing? Let's just use the current environment then.
                        Current.Environment.CurrentCode;

                    Info<T>.Settings.State.Step = "Setting up Reference Bundle";
                    var refBundle = Info<T>.Configuration?.ConnectionBundleType ?? Current.GlobalConnectionBundleType;

                    if (refBundle == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure,
                            "No valid connection bundle found");
                        return;
                    }

                    var refType = (ConnectionBundlePrimitive)Activator.CreateInstance(refBundle);

                    Info<T>.Settings.Bundle = refType;
                    Info<T>.Settings.Bundle.Validate(ConnectionBundlePrimitive.EValidationScope.Database);

                    if (refType.AdapterType == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure,
                            "No AdapterType defined on bundle");
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
                    Info<T>.Settings.ConnectionCypherKeys =
                        Info<T>.Settings?.ConnectionCypherKeys ?? refType?.ConnectionCypherKeys;

                    Info<T>.Settings.State.Step = "Determining CredentialSets to use";
                    Info<T>.Settings.CredentialSet =
                        Factory.GetCredentialSetPerConnectionBundle(Info<T>.Settings.Bundle,
                            Info<T>.Configuration?.CredentialSetType);

                    if (Info<T>.Settings.CredentialSet != null)
                        Info<T>.Settings.Statistics["Settings.CredentialSet"] =
                            Info<T>.Settings.CredentialSet?.GetType().Name;

                    Info<T>.Settings.CredentialCypherKeys =
                        Info<T>.Configuration?.CredentialCypherKeys ??
                        Info<T>.Settings.CredentialSet?.CredentialCypherKeys;

                    // Now we're ready to talk to the outside world.

                    Info<T>.Settings.State.Step = "Checking Connection to storage";

                    Info<T>.Settings.Adapter.SetConnectionString<T>();

                    Info<T>.Settings.Adapter.Setup<T>(Info<T>.Settings);
                    Info<T>.Settings.Adapter.Initialize<T>();

                    Current.Environment.EnvironmentChanged += Environment_EnvironmentChanged;

                    foreach (var (key, value) in Info<T>.Settings.Statistics) Current.Log.Debug(key + " : " + value);

                    Info<T>.Settings.State.Set<T>(Settings.EStatus.Operational, "Ready");
                }
                catch (Exception e)
                {
                    Info<T>.Settings.State.Status = Settings.EStatus.CriticalFailure;

                    Info<T>.Settings.State.Description =
                        typeof(T).FullName + " ERR " + Info<T>.Settings.State.Step + " : " + e.Message;
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

        #region State tools

        public static class Info<T> where T : Data<T>
        {
            public static Settings Settings => ClassRegistration[typeof(T)].Item1;
            public static DataConfigAttribute Configuration => ClassRegistration[typeof(T)].Item2;
        }

        private static void ValidateState()
        {
            if (Info<T>.Settings.State.Status != Settings.EStatus.Operational &&
                Info<T>.Settings.State.Status != Settings.EStatus.Initializing)
                throw new Exception($"Class is not operational: {Info<T>.Settings.State.Status}, {Info<T>.Settings.State.Description}");
        }

        #endregion

        #region Static references

        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>> ClassRegistration
            = new ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>>();

        public static string GetKey(Data<T> oRef)
        {
            return oRef == null
                ? null
                : (oRef.GetType().GetProperty(Info<T>.Settings.KeyMemberName)?.GetValue(oRef, null) ?? "")
                .ToString();
        }

        public static string GetDisplay(Data<T> oRef)
        {
            return oRef == null ? null
                : (oRef.GetType().GetProperty(Info<T>.Settings.DisplayMemberName)?.GetValue(oRef, null) ?? "").ToString();
        }


        public static string CacheKey(string key = "")
        {
            if (_cacheKeyBase != null) return _cacheKeyBase + key;

            _cacheKeyBase = typeof(T) + ":";
            return _cacheKeyBase + key;
        }

        public static string CacheKey(Type t, string key = "")
        {
            return t.FullName + ":" + key;
        }

        #endregion

        #region Instanced references

        public void SetDataKey(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);

            var refField = GetType().GetField(Info<T>.Settings.KeyMemberName);
            if (refField != null)
            {
                refField.SetValue(oRef, Convert.ChangeType(value, refField.FieldType));
            }
            {
                var refProp = GetType().GetProperty(Info<T>.Settings.KeyMemberName);
                refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
            }
        }

        public void SetDataLabel(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);
            var refProp = GetType().GetProperty(Info<T>.Settings.DisplayMemberName);
            refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
        }

        public string GetDataKey()
        {
            return GetKey(this);
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
            catch (Exception e)
            {
                Current.Log.Add(e);
            }
        }

        private static T ProcBeforePipeline(EAction action, T current, T source)
        {
            if (current == null) return null;
            if (Info<T>.Settings?.Pipelines?.Before == null) return current;

            foreach (var beforeActionPipeline in Info<T>.Settings.Pipelines.Before)
                try
                {
                    if (current != null) current = beforeActionPipeline.Process(action, current, source);
                }
                catch (Exception e)
                {
                    Current.Log.Add<T>(e);
                }

            return current;
        }

        private static void ProcAfterPipeline(EAction action, T current, T source)
        {
            if (Info<T>.Settings?.Pipelines?.After == null) return;

            foreach (var afterActionPipeline in Info<T>.Settings.Pipelines.After)
                try
                {
                    afterActionPipeline.Process(action, current, source);
                }
                catch (Exception e)
                {
                    Current.Log.Add<T>(e);
                }
        }

        #endregion

        #region Static Data handlers

        public static IEnumerable<T> All()
        {
            ValidateState();
            return Info<T>.Settings.Adapter.All<T>();
        }

        public static IEnumerable<TU> All<TU>(string extraParms = null)
        {
            ValidateState();
            return Info<T>.Settings.Adapter.All<T, TU>(extraParms);
        }

        public static T Get(string Key)
        {
            if (Key == null) return null;

            if (Info<T>.Settings.KeyMemberName == null)
            {
                Current.Log.Warn<T>("Key not set");
                throw new MissingPrimaryKeyException("Key not set for " + typeof(T).FullName);
            }

            return Info<T>.Configuration?.UseCaching == true
                ? CacheFactory.FetchSingleResultByKey(GetFromDatabase, Key)
                : GetFromDatabase(Key);
        }

        internal static T GetFromDatabase(object Key)
        {
            return Info<T>.Settings.Adapter.Get<T>(Key.ToString());
        }

        #endregion

        #region Instanced data handlers

        public bool IsNew(ref T oProbe)
        {
            if (_cachedIsNew.HasValue) return _cachedIsNew.Value;

            var probe = GetKey(this);

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
            ValidateState();


            if (Info<T>.Configuration != null)
                if (Info<T>.Configuration.IsReadOnly)
                    throw new Exception("This entity is set as read-only.");

            if (_isDeleted) return null;
            var ret = "";

            T oldRec = null;
            var isNew = IsNew(ref oldRec);

            var rec = this;
            rec = ProcBeforePipeline(isNew ? EAction.Insert : EAction.Update, (T)rec, oldRec);

            if (rec == null) return null;

            BeforeSave();

            Info<T>.Settings.Adapter.Upsert(rec);

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

        public virtual void AfterInsert(string newKey)
        {
        }

        public virtual void AfterSave(string newKey)
        {
        }

        public virtual void BeforeSave()
        {
        }

        public virtual void BeforeInsert()
        {
        }

        public virtual void BeforeRemove()
        {
        }

        public virtual void AfterRemove()
        {
        }

        #endregion
    }
}