using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Extension;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.CommonAttributes;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Data.Pipeline;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

#pragma warning disable 693

namespace Zen.Base.Module
{
    public abstract class Data<T> where T : Data<T>
    {
        internal static string _cacheKeyBase;
        private static readonly object _InitializationLock = new object();
        private bool _isDeleted;
        private bool? _isNew;

        #region Bootstrap

        static Data()
        {
            lock (_InitializationLock)
            {
                Settings<T> _settings = null;

                try
                {
                    // A new Data<> is born. Let's help it grow into a fully functional ORM connector. 

                    // First we prepare a registry containing all necessary information for it to operate.

                    TypeConfigurationCache<T>.ClassRegistration.TryAdd(typeof(T), new Tuple<Settings<T>, DataConfigAttribute>(new Settings<T>(), (DataConfigAttribute) Attribute.GetCustomAttribute(typeof(T), typeof(DataConfigAttribute)) ?? new DataConfigAttribute()));

                    _settings = Info<T>.Settings;

                    _cacheKeyBase = typeof(T).FullName;

                    _settings.State.Status = EDataStatus.Initializing;

                    // Do we have a [Key] and a [Display]?

                    _settings.State.Step = "Starting TableData/Statements setup";

                    // _settings.StorageName = Info<T>.Configuration?.Label ?? Info<T>.Configuration?.SetName ?? typeof(T).Name;

                    var sourceType = typeof(T);

                    while (sourceType?.IsGenericType == true) sourceType = sourceType.GenericTypeArguments.FirstOrDefault();

                    _settings.TypeName = sourceType?.Name;

                    _settings.TypeQualifiedName = sourceType?.FullName;

                    if (_settings.TypeQualifiedName != _settings.TypeName)
                        if (sourceType?.FullName != null)
                            _settings.TypeNamespace = sourceType?.FullName.Substring(0, _settings.TypeQualifiedName.Length - _settings.TypeName.Length - 1);

                    _settings.StorageCollectionName = Info<T>.Configuration?.Label ?? Info<T>.Configuration?.SetName ?? _settings.TypeName;
                    _settings.FriendlyName = Info<T>.Configuration?.FriendlyName ?? _settings.StorageCollectionName;

                    _settings.KeyField = typeof(T).GetFields().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute), true));
                    _settings.KeyProperty = typeof(T).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute), true));

                    if (_settings.KeyField == null && _settings.KeyProperty == null && Info<T>.Configuration?.KeyName != null)
                    {
                        // A Key member name was provided. Does it really exist? Let's find out.
                        // (Some drivers need explicit key property declaration, like LiteDB.)

                        _settings.KeyField = typeof(T).GetFields()
                            .FirstOrDefault(i => i.Name == Info<T>.Configuration.KeyName);
                        _settings.KeyProperty = typeof(T).GetProperties()
                            .FirstOrDefault(i => i.Name == Info<T>.Configuration.KeyName);
                    }

                    _settings.KeyMemberName =
                        // If there's a [Key] attribute on a field, use its name;
                        _settings.KeyField?.Name ??
                        // If there's a [Key] attribute on a Property, use its name;
                        _settings.KeyProperty?.Name ??
                        // Otherwise pick from Config
                        Info<T>.Configuration?.KeyName;

                    _settings.Silent = Info<T>.Configuration?.Silent == true;

                    if (_settings.KeyMemberName == null)
                    {
                        _settings.State.Set(EDataStatus.CriticalFailure, "No valid Key member found");
                        return;
                    }

                    if (_settings.KeyField == null && _settings.KeyProperty == null)
                    {
                        _settings.State.Set(EDataStatus.CriticalFailure, $"No member match for Key {Info<T>.Configuration?.KeyName}");
                        return;
                    }

                    _settings.DisplayField = typeof(T).GetFields().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(DisplayAttribute), true));
                    _settings.DisplayProperty = typeof(T).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(DisplayAttribute), true));

                    if (_settings.DisplayField == null && _settings.DisplayProperty == null && Info<T>.Configuration?.DisplayProperty != null)
                    {
                        // A Display member name was provided. Does it really exist? Let's find out.
                        _settings.DisplayField = typeof(T).GetFields().FirstOrDefault(i => i.Name == Info<T>.Configuration.DisplayProperty);
                        _settings.DisplayProperty = typeof(T).GetProperties().FirstOrDefault(i => i.Name == Info<T>.Configuration.DisplayProperty);
                    }

                    _settings.DisplayMemberName =
                        // If there's a [Key] attribute on a field, use its name;
                        _settings.DisplayField?.Name ??
                        // If there's a [Key] attribute on a Property, use its name;
                        _settings.DisplayProperty?.Name;

                    if (!_settings.Silent)
                        if (_settings.DisplayProperty?.Name != null && _settings.DisplayMemberName == null)
                            Current.Log.Warn<T>($"Mismatched DisplayMemberName: {_settings.DisplayMemberName}. Ignoring.");

                    // Member definitions

                    // Start with properties...
                    var memberMap = new Dictionary<string, MemberAttribute>();
                    foreach (var property in typeof(T).GetProperties())
                    {
                        var targetMember = (MemberAttribute) Attribute.GetCustomAttribute(property, typeof(MemberAttribute)) ?? new MemberAttribute {TargetName = property.Name};
                        targetMember.SourceName = property.Name;
                        targetMember.Interface = EMemberType.Property;
                        targetMember.Type = property.PropertyType;

                        memberMap.Add(property.Name, targetMember);
                    }

                    // and then add Fields.
                    foreach (var field in typeof(T).GetFields())
                    {
                        var targetMember = (MemberAttribute) Attribute.GetCustomAttribute(field, typeof(MemberAttribute)) ?? new MemberAttribute {TargetName = field.Name};
                        targetMember.SourceName = field.Name;
                        targetMember.Interface = EMemberType.Field;
                        targetMember.Type = field.FieldType;

                        memberMap.Add(field.Name, targetMember);
                    }

                    _settings.Members = memberMap;

                    // Do we have any pipelines defined?

                    _settings.Pipelines = new PipelineQueueHandler
                    {
                        Before = typeof(T).GetCustomAttributes(true).OfType<IBeforeActionPipeline>().ToList(),
                        After = typeof(T).GetCustomAttributes(true).OfType<IAfterActionPipeline>().ToList()
                    };

                    // Now let's report what we've just done.

                    //if (_settings.Pipelines.Before.Any())
                    //    _settings.Statistics["Settings.Pipelines.Before"] = _settings.Pipelines.Before
                    //        .Select(i =>i.Descriptor).Aggregate((i, j) => i + "," + j);
                    //if (_settings.Pipelines.After.Any())
                    //    _settings.Statistics["Settings.Pipelines.After"] = _settings.Pipelines.After
                    //        .Select(i => i.Descriptor).Aggregate((i, j) => i + "," + j);

                    _settings.State.Step = "Determining Environment";

                    // Next step: Record Environment Mapping data, if any.

                    _settings.EnvironmentMapping = Attribute
                        .GetCustomAttributes(typeof(T), typeof(DataEnvironmentMappingAttribute))
                        .Select(i => (DataEnvironmentMappingAttribute) i)
                        .ToList();

                    var persistentEnvironmentCode = Info<T>.Configuration?.PersistentEnvironmentCode;
                    var environmentMappingCode = _settings.EnvironmentMapping?.FirstOrDefault(i => i.Origin == Current.Environment?.CurrentCode)?.Target;
                    var currentEnvironmentCode = Current.Environment?.Current?.Code;

                    _settings.EnvironmentCode =
                        // If a PersistentEnvironmentCode is defined, use it.
                        persistentEnvironmentCode ??
                        // Otherwise let's check if there's a mapping defined for the current 'real' environment.
                        environmentMappingCode ??
                        // Nothing? Let's just use the current environment then.
                        currentEnvironmentCode;

                    _settings.State.Step = "Setting up Reference Bundle";
                    var refBundle = Info<T>.Configuration?.ConnectionBundleType ?? Instances.ServiceProvider.GetService<IConnectionBundleProvider>().DefaultBundleType;

                    if (refBundle == null)
                    {
                        _settings.State.Set(EDataStatus.CriticalFailure, "No valid connection bundle found");
                        return;
                    }

                    var connectionBundleInstance = refBundle.CreateInstance<IConnectionBundle>();

                    _settings.Adapter = GetDataAdapter();

                    if (_settings.Adapter == null)
                    {
                        _settings.Bundle = connectionBundleInstance;
                        _settings.Bundle.Validate(EValidationScope.Database);

                        if (connectionBundleInstance.AdapterType == null)
                        {
                            _settings.State.Set(EDataStatus.CriticalFailure, "No AdapterType defined on bundle");
                            return;
                        }

                        _settings.Adapter = connectionBundleInstance.AdapterType.CreateGenericInstance<T, DataAdapterPrimitive<T>>();

                        _settings.Adapter.SourceBundle = connectionBundleInstance;

                        if (_settings.Adapter == null)
                        {
                            _settings.State.Set(EDataStatus.CriticalFailure, "Null AdapterType");
                            return;
                        }

                        _settings.State.Step = "Setting up CypherKeys";
                        _settings.ConnectionCypherKeys =
                            _settings?.ConnectionCypherKeys ?? connectionBundleInstance?.ConnectionCypherKeys;

                        _settings.State.Step = "Determining CredentialSets to use";
                        _settings.CredentialSet = _settings.Bundle.GetCredentialSet(Info<T>.Configuration?.CredentialSetType);

                        //if (_settings.CredentialSet!= null)
                        //    _settings.Statistics["Settings.CredentialSet"] =
                        //        _settings.CredentialSet?.GetType().Name;

                        _settings.CredentialCypherKeys =
                            Info<T>.Configuration?.CredentialCypherKeys ??
                            _settings.CredentialSet?.CredentialCypherKeys;

                        // Now we're ready to talk to the outside world.

                        _settings.State.Step = "Checking Connection to storage";

                        _settings.Adapter.SetConnectionString();

                        _settings.Adapter.Setup(_settings);

                        _settings.State.Step = "Initializing adapter";
                        _settings.Adapter.Initialize();
                    }

                    if (!_settings.Silent)
                        foreach (var (key, value) in _settings.Statistics)
                            Current.Log.KeyValuePair(key, value, Message.EContentType.StartupSequence);

                    _settings.State.Step = "Wrapping up initialization";

                    if (!_settings.Silent)
                    {
                        Events.AddLog($"{_settings.TypeQualifiedName}", $"Ready | {_settings.EnvironmentCode} + {connectionBundleInstance.GetType().Name} + {_settings.Adapter.ReferenceCollectionName}");

                        var moreInfo = new List<string>();

                        if (_settings.Pipelines.Before.Any())
                            moreInfo.Add("Pre: " + _settings.Pipelines.Before
                                             .Select(i => i.PipelineName).Aggregate((i, j) => i + ", " + j));

                        if (_settings.Pipelines.After.Any())
                            moreInfo.Add("Post: " + _settings.Pipelines.After
                                             .Select(i => i.PipelineName).Aggregate((i, j) => i + ", " + j));

                        if (!moreInfo.Any()) return;

                        var pipelineInfo = moreInfo.Aggregate((i, j) => i + "; " + j);
                        Events.AddLog($"{_settings.TypeQualifiedName}", $" More | {pipelineInfo}");
                    }
                }
                catch (Exception e)
                {
                    _settings.State.Status = EDataStatus.CriticalFailure;

                    _settings.State.Description = $"{typeof(T).FullName} ERR {_settings.State.Step} : {e.Message}";
                    _settings.State.Stack = new StackTrace(e, true).FancyString();

                    var refEx = e;
                    while (refEx.InnerException != null)
                    {
                        refEx = e.InnerException;
                        _settings.State.Description += " / " + refEx.Message;
                    }

                    if (!_settings.Silent) Current.Log.Warn(_settings.State.Description);
                }
            }
        }

        #endregion

        public static string GetName() => Info<T>.Settings.FriendlyName;

        #region Overrides of Object
        public override string ToString() => $"{GetDataKey()} : {GetDataDisplay() ?? this.ToJson()}";
        #endregion

        public static T New() => (T) Activator.CreateInstance(typeof(T));

        public static IEnumerable<T> GetByLocator(IEnumerable<string> locators, Mutator mutator = null)
        {
            var model = Where(i => locators.Contains((i as IDataLocator).Locator), mutator).ToList();
            model.AfterGet();

            return model;
        }

        private enum EMetadataScope
        {
            Collection
        }

        #region State tools

        public static DataAdapterPrimitive<T> GetDataAdapter() => null;

        private static void ValidateState(EActionType? type = null)
        {
            var settings = Info<T>.Settings;

            if (settings.State.Status != EDataStatus.Operational && settings.State.Status != EDataStatus.Initializing) throw new Exception($"{typeof(T).FullName} | Class is not operational: {settings.State.Status}, {settings.State.Description}");

            switch (type)
            {
                case null: break;
                case EActionType.Read: break;
                case EActionType.Insert:
                case EActionType.Update:
                case EActionType.Remove:
                    if (Info<T>.Configuration != null)
                        if (Info<T>.Configuration.IsReadOnly)
                            throw new Exception("This model is set as read-only.");
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion

        #region Static references

        // ReSharper disable once StaticMemberInGenericType

        public static string GetDataKey(Data<T> oRef) =>
            oRef == null
                ? null
                : (oRef.GetType().GetProperty(Info<T>.Settings.KeyMemberName)?.GetValue(oRef, null) ?? "")
                .ToString();

        public static string GetDataDisplay(Data<T> oRef)
        {
            if (oRef == null) return null;
            if (Info<T>.Settings.DisplayMemberName == null) return null;

            return (oRef.GetType().GetProperty(Info<T>.Settings.DisplayMemberName)?.GetValue(oRef, null) ?? "").ToString();
        }

        #endregion

        #region Instanced operations

        public void SetDataKey(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);

            var refField = GetType().GetField(Info<T>.Settings.KeyMemberName);
            if (refField != null) refField.SetValue(oRef, Convert.ChangeType(value, refField.FieldType));
            {
                var refProp = GetType().GetProperty(Info<T>.Settings.KeyMemberName);
                refProp?.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
            }
        }

        public void SetDataDisplay(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);
            var refProp = GetType().GetProperty(Info<T>.Settings.DisplayMemberName);
            refProp?.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
        }

        public string GetDataKey() => GetDataKey(this);

        public string GetDataDisplay() => GetDataDisplay(this);

        public string GetFullIdentifier() => Info<T>.Settings.TypeQualifiedName + ":" + GetDataKey();

        #endregion

        #region Event handling

        private static T ProcBeforePipeline(EActionType action, EActionScope scope, Mutator mutator, T currentModel, T originalModel)
        {
            if (Info<T>.Settings?.Pipelines?.Before == null) return currentModel;

            foreach (var beforeActionPipeline in Info<T>.Settings.Pipelines.Before)
                try
                {
                    currentModel = beforeActionPipeline.Process(action, scope, mutator, currentModel, originalModel);
                }
                catch (Exception e)
                {
                    if (!Info<T>.Settings.Silent) Current.Log.Add<T>(e);
                }

            return currentModel;
        }

        private static void ProcAfterPipeline(EActionType action, EActionScope scope, Mutator mutator, T currentModel, T originalModel)
        {
            if (Info<T>.Settings?.Pipelines?.After == null) return;

            foreach (var afterActionPipeline in Info<T>.Settings.Pipelines.After)
                try
                {
                    afterActionPipeline.Process(action, scope, mutator, currentModel, originalModel);
                }
                catch (Exception e)
                {
                    if (!Info<T>.Settings.Silent) Current.Log.Add<T>(e);
                }
        }

        #endregion

        #region Static Data handlers

        public static IEnumerable<T> All() => All<T>().AfterGet();

        public static IEnumerable<TU> All<TU>()
        {
            ValidateState(EActionType.Read);
            return Info<T>.Settings.Adapter.Query<TU>();
        }

        public static IEnumerable<T> Query(string statement) => Query<T>(statement.ToModifier()).ToList().AfterGet();

        public static IEnumerable<T> Query(Mutator mutator = null) => Query<T>(mutator).AfterGet();

        public static IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            ValidateState(EActionType.Read);

            var settings = Info<T>.Settings;

            mutator = settings.GetInstancedModifier().Value.BeforeQuery(EActionType.Read, mutator) ?? mutator;

            try
            {
                return settings.Adapter.Where(predicate, mutator).ToList().AfterGet();
            }
            catch (FormatException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static IEnumerable<TU> Query<TU>(string statement) => Query<TU>(statement.ToModifier());

        public static IEnumerable<TU> Query<TU>(Mutator mutator = null)
        {
            ValidateState(EActionType.Read);

            mutator = Info<T>.Settings.GetInstancedModifier().Value.BeforeQuery(EActionType.Read, mutator) ?? mutator;

            return Info<T>.Settings.Adapter.Query<TU>(mutator);
        }

        public static long Count(string statement) => Count(statement.ToModifier());

        public static long Count(Mutator mutator = null)
        {
            ValidateState(EActionType.Read);

            mutator = Info<T>.Settings.GetInstancedModifier().Value.BeforeCount(EActionType.Read, mutator) ?? mutator;

            return Info<T>.Settings.Adapter.Count(mutator);
        }

        public static T Get(string key, Mutator mutator = null, bool bypassCache = false)
        {
            ValidateState(EActionType.Read);

            if (key == null) return null;

            if (Info<T>.Settings.KeyMemberName == null)
            {
                if (!Info<T>.Settings.Silent) Current.Log.Warn<T>("Invalid operation; key not set");
                throw new MissingPrimaryKeyException("Key not set for " + typeof(T).FullName);
            }

            var fullKey = mutator?.KeyPrefix + key;

            if (!Info<T>.Configuration.UseCaching || bypassCache) return FetchModel(key, mutator);

            return CacheFactory.FetchModel<T>(fullKey) ?? FetchModel(key, mutator);
        }

        public static Dictionary<string, T> GetMap(IEnumerable<string> keys)
        {
            ValidateState(EActionType.Read);

            if (keys == null) return null;

            var keyList = keys.ToList();

            if (!keyList.Any()) return new Dictionary<string, T>();

            keyList = keyList.Distinct().ToList();

            if (Info<T>.Settings.KeyMemberName != null) return FetchSet(keyList);

            if (!Info<T>.Settings.Silent) Current.Log.Warn<T>("Invalid operation; key not set");
            throw new MissingPrimaryKeyException("Key not set for " + typeof(T).FullName);
        }

        public static IEnumerable<T> Get(IEnumerable<string> keys) => GetMap(keys)?.Values;

        private static readonly object _bulkSaveLock = new object();

        public static BulkDataOperation<T> Save(IEnumerable<T> models, Mutator mutator = null, bool rawMode = false) => BulkExecute(EActionType.Update, models, mutator, rawMode);

        public static BulkDataOperation<T> Remove(IEnumerable<T> models, Mutator mutator = null, bool rawMode = false) => BulkExecute(EActionType.Remove, models, mutator, rawMode);

        public static T Remove(string Key, Mutator mutator = null)
        {
            var targetModel = Get(Key, mutator);
            targetModel?.Remove(mutator);
            return targetModel;
        }

        public static void RemoveAll(Mutator mutator = null)
        {
            ValidateState(EActionType.Remove);

            ProcBeforePipeline(EActionType.Remove, EActionScope.Collection, mutator, null, null);

            Info<T>.Settings.Adapter.RemoveAll(mutator);
            Info<T>.TryFlushCachedCollection();

            ProcAfterPipeline(EActionType.Remove, EActionScope.Collection, mutator, null, null);
        }

        #endregion

        #region Internal Model/Storage exchange

        private static BulkDataOperation<T> BulkExecute(EActionType type, IEnumerable<T> models, Mutator mutator = null, bool rawMode = false)
        {
            ValidateState(type);

            var logStep = "";
            object logObj = null;

            if (models == null) return null;

            var modelSet = models.ToList();
            var modelCount = modelSet.Count;

            var silent = modelCount == 1 || Info<T>.Settings.Silent;

            var _timed = new TimeLog();

            if (modelSet.Count == 0) return null;

            var resultPackage = new BulkDataOperation<T> {Type = type};

            // First let's obtain any ServiceTokenGuid set by the user.

            lock (_bulkSaveLock)
            {
                try
                {
                    var successSet = new List<T>();
                    var failureSet = new List<T>();
                    Clicker logClicker = null;

                    _timed.Start($"{type} bulk [Warm-up]", !silent);
                    if (!silent) logClicker = modelSet.GetClicker(_timed.CurrentMessage, !silent);

                    resultPackage.Control = new ConcurrentDictionary<string, DataOperationControl<T>>();

                    var paralelizableClicker = logClicker;

                    if (!rawMode)
                    {
                        Parallel.ForEach(modelSet, new ParallelOptions {MaxDegreeOfParallelism = 5}, item =>
                        {
                            paralelizableClicker?.Click();

                            if (item.IsNew())
                            {
                                var tempKey = mutator?.KeyPrefix + item.ToJson().Sha512Hash();

                                if (resultPackage.Control.ContainsKey(tempKey))
                                {
                                    if (!silent) Current.Log.Warn<T>(_timed.Log($"    [Warm-up] duplicated key: {tempKey}"));
                                    failureSet.Add(item);
                                }
                                else
                                {
                                    resultPackage.Control[tempKey] = new DataOperationControl<T> {Current = item, IsNew = true, Original = null};
                                }

                                return;
                            }

                            var modelKey = mutator?.KeyPrefix + item.GetDataKey();

                            if (resultPackage.Control.ContainsKey(modelKey))
                            {
                                if (!silent) Current.Log.Warn<T>(_timed.Log($"Repeated Identifier: {modelKey}. Data: {item.ToJson()}"));
                                return;
                            }

                            resultPackage.Control[modelKey] = new DataOperationControl<T> {Current = item};
                        });

                        logClicker?.End();

                        _timed.Log($"{type} bulk  [Before]", false);

                        logClicker = modelSet.GetClicker(_timed.CurrentMessage);

                        logStep = "obtaining original keys";
                        var originalKeys = resultPackage.Control.Where(controlItem => !controlItem.Value.IsNew).Select(controlPair => controlPair.Key).ToList();

                        logStep = "obtaining original models";
                        var originalSet = Get(originalKeys).ToList();

                        logStep = _timed.Log("Populating Control structure");
                        var originalMap = originalSet.ToDictionary(i => i.GetDataKey(), i => i).ToList();

                        foreach (var item in originalMap) resultPackage.Control[item.Key].Original = item.Value;

                        logStep = "processing Control structure";

                        foreach (var controlItem in resultPackage.Control)
                        {
                            if (!silent) logClicker.Click();

                            var currentModel = controlItem.Value.Current;
                            var originalModel = controlItem.Value.Original;

                            var canProceed = true;

                            logStep = "checking if model is new";

                            if (!controlItem.Value.IsNew)
                            {
                                logObj = currentModel;

                                originalModel = type == EActionType.Remove ? ProcBeforePipeline(EActionType.Remove, EActionScope.Model, mutator, currentModel, originalModel) : ProcBeforePipeline(controlItem.Value.IsNew ? EActionType.Insert : EActionType.Update, EActionScope.Model, mutator, currentModel, originalModel);

                                if (originalModel == null)
                                {
                                    failureSet.Add(currentModel);
                                    controlItem.Value.Success = false;
                                    controlItem.Value.Message = "Failed ProcBeforePipeline";
                                    canProceed = false;
                                }
                            }
                            else
                            {
                                if (type == EActionType.Remove) // So we're removing a New object. Just ignore.
                                {
                                    failureSet.Add(currentModel);
                                    controlItem.Value.Success = false;
                                    controlItem.Value.Message = $"Invalid {type} operation: Record is New()";
                                    canProceed = false;
                                }
                                else
                                {
                                    originalModel = currentModel;
                                }
                            }

                            if (canProceed)
                            {
                                logStep = "Adding model to process list";
                                logObj = currentModel;

                                successSet.Add(originalModel);

                                if (type == EActionType.Remove)
                                {
                                    originalModel.BeforeRemove();
                                }
                                else
                                {
                                    if (!originalModel.IsNew()) originalModel.BeforeUpdate();
                                    else originalModel.BeforeInsert();

                                    originalModel.BeforeSave();
                                }

                                controlItem.Value.Success = true;
                            }
                        }

                        if (!silent) logClicker.End();

                        logStep = _timed.Log($"{type} {successSet.Count} models");

                        if (type == EActionType.Remove) Info<T>.Settings.Adapter.BulkRemove(successSet);
                        else Info<T>.Settings.Adapter.BulkUpsert(successSet);

                        if (!silent) logClicker = modelSet.GetClicker($"{type} bulk   [After]");

                        logStep = _timed.Log("post-processing individual models");

                        Parallel.ForEach(resultPackage.Control.Where(i => i.Value.Success), new ParallelOptions {MaxDegreeOfParallelism = 5}, controlModel =>
                        {
                            var key = controlModel.Key;
                            if (!silent) logClicker.Click();

                            if (type == EActionType.Remove)
                            {
                                controlModel.Value.Current.AfterRemove();
                                ProcAfterPipeline(EActionType.Remove, EActionScope.Model, mutator, controlModel.Value.Current, controlModel.Value.Original);
                            }
                            else
                            {
                                if (controlModel.Value.IsNew) controlModel.Value.Current.AfterInsert(key);
                                else controlModel.Value.Current.AfterUpdate(key);

                                controlModel.Value.Current.AfterSave(key);

                                ProcAfterPipeline(controlModel.Value.IsNew ? EActionType.Insert : EActionType.Update, EActionScope.Model, mutator, controlModel.Value.Current, controlModel.Value.Original);
                            }

                            CacheFactory.FlushModel<T>(key);
                        });

                        resultPackage.Success = successSet;
                        resultPackage.Failure = failureSet;

                        logStep = _timed.Log($"{type} bulk operation complete. Success: {resultPackage.Success.Count} | Failure: {resultPackage.Failure.Count}");

                        if (!silent) logClicker.End();
                        _timed.End(false);
                    }
                    else //RawMode means no triggers. AT ALL.
                    {
                        if (type == EActionType.Remove) Info<T>.Settings.Adapter.BulkRemove(modelSet);
                        else Info<T>.Settings.Adapter.BulkUpsert(modelSet);
                    }

                    return resultPackage;
                }
                catch (Exception e)
                {
                    if (!silent) Current.Log.Add<T>(e);
                    var ex = new Exception($"{type} - Error while {logStep} {logObj?.ToJson()}: {e.Message}", e);

                    _timed.End();
                    throw ex;
                }
            }
        }

        internal static T FetchModel(string key, Mutator mutator = null)
        {
            var model = Info<T>.Settings.Adapter.Get(key, mutator);
            model?.AfterGet();

            var fullKey = mutator?.KeyPrefix + model?.GetDataKey();

            if (!string.IsNullOrEmpty(fullKey)) CacheFactory.StoreModel(fullKey, model);

            return model;
        }

        internal static Dictionary<string, T> FetchSet(IEnumerable<string> keys, bool bypassCache = false, Mutator mutator = null)
        {
            //This function mixes cached models with partial queries to the adapter.
            var fetchKeys = keys.ToList();

            // First we create a map to accomodate all the requested records.
            var fetchMap = fetchKeys.ToDictionary(i => i, i => (T) null);

            //Then we proceed to probe the cache for individual model copies if the user didn't decided to ignore cache.
            var cacheKeyPrefix = mutator?.KeyPrefix;
            if (!bypassCache)
                foreach (var key in fetchKeys)
                    fetchMap[key] = CacheFactory.FetchModel<T>(cacheKeyPrefix + key);

            //At this point we may have a map that's only partially populated. Let's then identify the missing models.
            var missedKeys = fetchMap.Where(i => i.Value == null).Select(i => i.Key).ToList();

            //Do a hard query on the missing keys.
            var cachedSet = Info<T>.Settings.Adapter.Get(missedKeys, mutator).ToList();

            // Post-fetch hook
            foreach (var model in cachedSet) model.AfterGet();

            // Now we fill the map with the missing models that we sucessfully fetched.
            foreach (var model in cachedSet.Where(model => model != null))
            {
                var key = cacheKeyPrefix + model.GetDataKey();
                fetchMap[key] = model;

                // And we take the time to populate the cache with the model.
                CacheFactory.StoreModel(key, model);
            }

            //Let's return the map and give the developer the chance to handle unbound keys.

            return fetchMap;
        }

        #endregion

        #region Instanced data handlers

        public bool IsNew(ref T originalModel, Mutator mutator = null)
        {
            if (_isNew.HasValue) return _isNew.Value;

            var key = GetDataKey(this);

            if (string.IsNullOrEmpty(key))
            {
                _isNew = true;
                return _isNew.Value;
            }

            var keyExists = Info<T>.Settings.Adapter.KeyExists(key, mutator);

            if (!keyExists)
            {
                _isNew = true;
                return _isNew.Value;
            }

            originalModel = Get(key, mutator, true);

            _isNew = originalModel == null;
            return _isNew.Value;
        }

        public bool IsNew(Mutator mutator = null)
        {
            T storedModel = null;
            return IsNew(ref storedModel, mutator);
        }

        public T Update(Mutator mutator = null)
        {
            if (_isDeleted) return null;

            T storedModel = null;
            var isNew = IsNew(ref storedModel, mutator);
            return isNew ? null : Save(mutator);
        }

        public T Save(Mutator mutator = null)
        {
            if (_isDeleted) return null;

            ValidateState(EActionType.Update);

            var localModel = (T) this;
            T storedModel = null;
            var isNew = IsNew(ref storedModel, mutator);

            var targetActionType = isNew ? EActionType.Insert : EActionType.Update;

            localModel = ProcBeforePipeline(targetActionType, EActionScope.Model, mutator, localModel, storedModel);

            if (localModel == null) return null;

            if (isNew) BeforeInsert();
            else BeforeUpdate();
            BeforeSave();

            var postKey = Info<T>.Settings.Adapter.Upsert(localModel).GetDataKey();

            Info<T>.TryFlushCachedModel(localModel);

            if (isNew) AfterInsert(postKey);
            else AfterUpdate(postKey);
            AfterSave(postKey);

            _isNew = null;

            if (Info<T>.Settings?.Pipelines?.After == null) return localModel;

            localModel = Get(postKey, mutator);
            ProcAfterPipeline(targetActionType, EActionScope.Model, mutator, localModel, storedModel);

            return localModel;
        }

        public T Insert(Mutator mutator = null)
        {
            if (_isDeleted) return null;

            ValidateState(EActionType.Insert);

            var localModel = (T) this;

            var targetActionType = EActionType.Insert;

            localModel = ProcBeforePipeline(targetActionType, EActionScope.Model, mutator, localModel, null);

            if (localModel == null) return null;

            BeforeInsert();
            BeforeSave();

            var postKey = Info<T>.Settings.Adapter.Insert(localModel).GetDataKey();

            Info<T>.TryFlushCachedModel(localModel);

            AfterInsert(postKey);
            AfterSave(postKey);

            _isNew = null;

            if (Info<T>.Settings?.Pipelines?.After == null) return localModel;

            localModel = Get(postKey, mutator);
            ProcAfterPipeline(targetActionType, EActionScope.Model, mutator, localModel, null);

            return localModel;
        }

        public T Remove(Mutator mutator = null)
        {
            ValidateState(EActionType.Remove);

            var localModel = (T) this;
            if (_isDeleted) return null;

            T storedModel = null;
            if (IsNew(ref storedModel)) return null;

            localModel = ProcBeforePipeline(EActionType.Remove, EActionScope.Model, mutator, localModel, storedModel);

            if (localModel == null) return null;

            BeforeRemove();

            Info<T>.Settings.Adapter.Remove(localModel, mutator);
            Info<T>.TryFlushCachedModel(localModel, mutator);

            AfterRemove();

            _isDeleted = true;

            if (Info<T>.Settings?.Pipelines?.After == null) return localModel;

            ProcAfterPipeline(EActionType.Remove, EActionScope.Model, mutator, localModel, storedModel);

            return storedModel; // Return last 'incarnation' of the model.
        }

        #endregion

        #region Event hooks and Behavior modifiers

        public virtual void BeforeUpdate() { }

        public virtual void AfterUpdate(string newKey) { }

        public virtual void BeforeInsert() { }

        public virtual void AfterInsert(string newKey) { }

        public virtual void BeforeSave() { }

        public virtual void AfterSave(string newKey) { }

        public virtual void BeforeRemove() { }

        public virtual void AfterRemove() { }

        public virtual void AfterGet() { }
        public virtual void AfterSetUpdate() { }
        public virtual Mutator BeforeQuery(EActionType read, Mutator mutator) => null;

        public virtual Mutator BeforeCount(EActionType read, Mutator mutator) => null;

        #endregion

        #region Common Attributes

        public static T GetByLocator(string locator, Mutator mutator = null)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global

            var model = Where(i => (i as IDataLocator).Locator == locator, mutator).FirstOrDefault();
            model?.AfterGet();

            return model;
        }

        public static T GetByCode(string code, Mutator mutator = null)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var model = Where(i => (i as IDataCode).Code == code, mutator).FirstOrDefault();
            model?.AfterGet();

            return model;
        }

        #endregion
    }
}