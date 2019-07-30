using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.CommonAttributes;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Data.Pipeline;
using Zen.Base.Module.Log;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

#pragma warning disable 693

namespace Zen.Base.Module
{
    public abstract class Data<T> where T : Data<T>
    {
        private static string _cacheKeyBase;
        private static readonly object _InitializationLock = new object();
        private bool _isDeleted;
        private bool? _isNew;

        #region Bootstrap

        static Data()
        {
            lock (_InitializationLock)
            {
                try
                {
                    // A new Data<> is born. Let's help it grow into a fully functional ORM connector. 

                    // First we prepare a registry containing all necessary information for it to operate.

                    ClassRegistration.TryAdd(typeof(T), new Tuple<Settings, DataConfigAttribute>(new Settings(), (DataConfigAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(DataConfigAttribute)) ?? new DataConfigAttribute()));

                    Info<T>.Settings.State.Status = Settings.EStatus.Initializing;

                    // Do we have a [Key] and a [Display]?

                    Info<T>.Settings.State.Step = "Starting TableData/Statements setup";

                    Info<T>.Settings.StorageName = Info<T>.Configuration?.Label ?? Info<T>.Configuration?.TableName ?? typeof(T).Name;

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

                    if (Info<T>.Settings.DisplayProperty?.Name != null && Info<T>.Settings.DisplayMemberName == null) Current.Log.Warn<T>($"Mismatched DisplayMemberName: {Info<T>.Settings.DisplayMemberName}. Ignoring.");

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

                    Info<T>.Settings.EnvironmentMapping = Attribute
                        .GetCustomAttributes(typeof(T), typeof(EnvironmentMappingAttribute))
                        .Select(i => (EnvironmentMappingAttribute)i)
                        .ToList();

                    Info<T>.Settings.EnvironmentCode =
                        // If a PersistentEnvironmentCode is defined, use it.
                        Info<T>.Configuration?.PersistentEnvironmentCode ??
                        // Otherwise let's check if there's a mapping defined for the current 'real' environment.
                        Info<T>.Settings.EnvironmentMapping?.FirstOrDefault(i => i.Origin == Current.Environment?.CurrentCode)?.Target ??
                        // Nothing? Let's just use the current environment then.
                        Current.Environment?.Current?.Code;

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

                    Info<T>.Settings.Adapter.SourceBundle = refType;

                    if (Info<T>.Settings.Adapter == null)
                    {
                        Info<T>.Settings.State.Set<T>(Settings.EStatus.CriticalFailure, "Null AdapterType");
                        return;
                    }

                    Info<T>.Settings.State.Step = "Setting up CypherKeys";
                    Info<T>.Settings.ConnectionCypherKeys =
                        Info<T>.Settings?.ConnectionCypherKeys ?? refType?.ConnectionCypherKeys;

                    Info<T>.Settings.State.Step = "Determining CredentialSets to use";
                    Info<T>.Settings.CredentialSet =
                        Factory.GetCredentialSetPerConnectionBundle(Info<T>.Settings.Bundle,
                                                                    Info<T>.Configuration?.CredentialSetType);

                    //if (Info<T>.Settings.CredentialSet != null)
                    //    Info<T>.Settings.Statistics["Settings.CredentialSet"] =
                    //        Info<T>.Settings.CredentialSet?.GetType().Name;

                    Info<T>.Settings.CredentialCypherKeys =
                        Info<T>.Configuration?.CredentialCypherKeys ??
                        Info<T>.Settings.CredentialSet?.CredentialCypherKeys;

                    // Now we're ready to talk to the outside world.

                    Info<T>.Settings.State.Step = "Checking Connection to storage";

                    Info<T>.Settings.Adapter.SetConnectionString<T>();

                    Info<T>.Settings.Adapter.Setup<T>(Info<T>.Settings);
                    Info<T>.Settings.Adapter.Initialize<T>();

                    foreach (var (key, value) in Info<T>.Settings.Statistics) Current.Log.KeyValuePair(key, value, Message.EContentType.StartupSequence);

                    Current.Log.KeyValuePair(typeof(T).FullName, $"Ready | {Info<T>.Settings.EnvironmentCode} + {refType.GetType().Name} + {Info<T>.Settings.Adapter.ReferenceCollectionName}", Message.EContentType.StartupSequence);
                }
                catch (Exception e)
                {
                    Info<T>.Settings.State.Status = Settings.EStatus.CriticalFailure;

                    Info<T>.Settings.State.Description = $"{typeof(T).FullName} ERR {Info<T>.Settings.State.Step} : {e.Message}";
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

        #region Overrides of Object

        public override string ToString() { return $"{GetDataKey()} : {GetDataDisplay() ?? this.ToJson()}"; }

        #endregion

        public static T New() { return (T)Activator.CreateInstance(typeof(T)); }

        private enum EMetadataScope
        {
            Collection
        }

        #region State tools

        public static class Info<T> where T : Data<T>
        {
            static Info() { _cacheKeyBase = typeof(T).FullName; }
            public static Settings Settings => ClassRegistration[typeof(T)].Item1;
            public static DataConfigAttribute Configuration => ClassRegistration[typeof(T)].Item2;
            public static string CacheKey(string key = "") { return _cacheKeyBase + ":" + key; }

            public static void TryFlushCachedCollection(Mutator mutator = null)
            {
                if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;

                var collectionKey = mutator?.KeyPrefix + _cacheKeyBase;
                Current.Cache.RemoveAll(collectionKey);
            }

            internal static void TryFlushCachedModel(T model, Mutator mutator = null)
            {
                if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;

                var key = mutator?.KeyPrefix + model.GetDataKey();
                Current.Cache.Remove(key);
            }
        }

        private static void ValidateState(EActionType? type = null)
        {
            if (Info<T>.Settings.State.Status != Settings.EStatus.Operational &&
                Info<T>.Settings.State.Status != Settings.EStatus.Initializing) throw new Exception($"Class is not operational: {Info<T>.Settings.State.Status}, {Info<T>.Settings.State.Description}");

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
        internal static readonly ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>> ClassRegistration
            = new ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>>();

        public static string GetDataKey(Data<T> oRef)
        {
            return oRef == null
                ? null
                : (oRef.GetType().GetProperty(Info<T>.Settings.KeyMemberName)?.GetValue(oRef, null) ?? "")
                .ToString();
        }

        public static string GetDataDisplay(Data<T> oRef)
        {
            return oRef == null
                ? null
                : (oRef.GetType().GetProperty(Info<T>.Settings.DisplayMemberName)?.GetValue(oRef, null) ?? "").ToString();
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
                refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
            }
        }

        public void SetDataDisplay(object value)
        {
            var oRef = this;
            if (value.IsNumeric()) value = Convert.ToInt64(value);
            var refProp = GetType().GetProperty(Info<T>.Settings.DisplayMemberName);
            refProp.SetValue(oRef, Convert.ChangeType(value, refProp.PropertyType));
        }

        public string GetDataKey() { return GetDataKey(this); }

        public string GetDataDisplay() { return GetDataDisplay(this); }

        #endregion

        #region Event handling

        private static T ProcBeforePipeline(EActionType action, EActionScope scope, Mutator mutator, T currentModel, T originalModel)
        {
            if (Info<T>.Settings?.Pipelines?.Before == null) return currentModel;

            foreach (var beforeActionPipeline in Info<T>.Settings.Pipelines.Before)
                try { currentModel = beforeActionPipeline.Process(action, scope, mutator, currentModel, originalModel); } catch (Exception e) { Current.Log.Add<T>(e); }

            return currentModel;
        }

        private static void ProcAfterPipeline(EActionType action, EActionScope scope, Mutator mutator, T currentModel, T originalModel)
        {
            if (Info<T>.Settings?.Pipelines?.After == null) return;

            foreach (var afterActionPipeline in Info<T>.Settings.Pipelines.After)
                try { afterActionPipeline.Process(action, scope, mutator, currentModel, originalModel); } catch (Exception e) { Current.Log.Add<T>(e); }
        }

        #endregion

        #region Static Data handlers

        public static IEnumerable<T> All() { return All<T>(); }

        public static IEnumerable<TU> All<TU>()
        {
            ValidateState(EActionType.Read);
            return Info<T>.Settings.Adapter.Query<T, TU>();
        }

        public static IEnumerable<T> Query(string statement) { return Query<T>(statement.ToModifier()); }

        public static IEnumerable<T> Query(Mutator mutator = null) { return Query<T>(mutator); }

        public static IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            ValidateState(EActionType.Read);
            mutator = Info<T>.Settings.GetInstancedModifier<T>().Value.BeforeQuery(EActionType.Read, mutator) ?? mutator;

            return Info<T>.Settings.Adapter.Where(predicate, mutator).ToList();
        }

        public static IEnumerable<TU> Query<TU>(string statement) { return Query<TU>(statement.ToModifier()); }

        public static IEnumerable<TU> Query<TU>(Mutator mutator = null)
        {
            ValidateState(EActionType.Read);

            mutator = Info<T>.Settings.GetInstancedModifier<T>().Value.BeforeQuery(EActionType.Read, mutator) ?? mutator;

            return Info<T>.Settings.Adapter.Query<T, TU>(mutator);
        }

        public static long Count(string statement) { return Count(statement.ToModifier()); }

        public static long Count(Mutator mutator = null)
        {
            ValidateState(EActionType.Read);

            mutator = Info<T>.Settings.GetInstancedModifier<T>().Value.BeforeCount(EActionType.Read, mutator) ?? mutator;

            return Info<T>.Settings.Adapter.Count<T>(mutator);
        }

        public static T Get(string key, Mutator mutator = null)
        {
            ValidateState(EActionType.Read);

            if (key == null) return null;

            if (Info<T>.Settings.KeyMemberName == null)
            {
                Current.Log.Warn<T>("Invalid operation; key not set");
                throw new MissingPrimaryKeyException("Key not set for " + typeof(T).FullName);
            }

            var fullKey = mutator?.KeyPrefix + key;

            var model = CacheFactory.FetchModel<T>(fullKey) ?? FetchModel(key, mutator);

            return model;
        }

        public static IEnumerable<T> Get(IEnumerable<string> keys)
        {
            ValidateState(EActionType.Read);

            if (keys == null) return null;
            if (Info<T>.Settings.KeyMemberName != null) return FetchSet(keys).Values;

            Current.Log.Warn<T>("Invalid operation; key not set");
            throw new MissingPrimaryKeyException("Key not set for " + typeof(T).FullName);
        }

        private static readonly object _bulkSaveLock = new object();

        public static BulkDataOperation<T> Save(IEnumerable<T> models, Mutator mutator = null) { return BulkExecute(EActionType.Update, models, mutator); }

        public static BulkDataOperation<T> Remove(IEnumerable<T> models, Mutator mutator = null) { return BulkExecute(EActionType.Remove, models, mutator); }

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

            Info<T>.Settings.Adapter.RemoveAll<T>(mutator);
            Info<T>.TryFlushCachedCollection();

            ProcAfterPipeline(EActionType.Remove, EActionScope.Collection, mutator, null, null);
        }

        #endregion

        #region Internal Model/Storage exchange

        private static BulkDataOperation<T> BulkExecute(EActionType type, IEnumerable<T> models, Mutator mutator = null)
        {
            ValidateState(type);

            var logStep = "";
            object logObj = null;

            var modelSet = models.ToList();

            var _timed = new TimeLog();

            if (modelSet.Count == 0) return null;

            var resultPackage = new BulkDataOperation<T> { Type = type };

            // First let's obtain any ServiceTokenGuid set by the user.

            lock (_bulkSaveLock)
            {
                try
                {
                    var successSet = new List<T>();
                    var failureSet = new List<T>();

                    _timed.Start($"{type} bulk [Warm-up]");
                    var logClicker = modelSet.GetClicker(_timed.CurrentMessage);

                    resultPackage.Control = new ConcurrentDictionary<string, DataOperationControl<T>>();

                    var paralelizableClicker = logClicker;

                    Parallel.ForEach(modelSet, new ParallelOptions { MaxDegreeOfParallelism = 5 }, item =>
                      {
                          paralelizableClicker.Click();

                          if (item.IsNew())
                          {
                              var tempKey = mutator?.KeyPrefix + item.ToJson().Sha512Hash();

                              if (resultPackage.Control.ContainsKey(tempKey))
                              {
                                  Current.Log.Warn<T>(_timed.Log($"    [Warm-up] duplicated key: {tempKey}"));
                                  failureSet.Add(item);
                              }
                              else { resultPackage.Control[tempKey] = new DataOperationControl<T> { Current = item, IsNew = true, Original = null }; }

                              return;
                          }

                          var modelKey = mutator?.KeyPrefix + item.GetDataKey();

                          if (resultPackage.Control.ContainsKey(modelKey))
                          {
                              Current.Log.Warn<T>(_timed.Log($"Repeated Identifier: {modelKey}. Data: {item.ToJson()}"));
                              return;
                          }

                          resultPackage.Control[modelKey] = new DataOperationControl<T> { Current = item };
                      });

                    logClicker.End();

                    _timed.Log($"{type} bulk  [Before]");

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
                        logClicker.Click();

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
                            else { originalModel = currentModel; }
                        }

                        if (canProceed)
                        {
                            logStep = "Adding model to process list";
                            logObj = currentModel;

                            successSet.Add(originalModel);

                            if (type == EActionType.Remove) { originalModel.BeforeRemove(); }
                            else
                            {
                                if (!originalModel.IsNew()) originalModel.BeforeUpdate();
                                else originalModel.BeforeInsert();

                                originalModel.BeforeSave();
                            }

                            controlItem.Value.Success = true;
                        }
                    }

                    logClicker.End();

                    logStep = _timed.Log($"{type} {successSet.Count} models");

                    if (type == EActionType.Remove) Info<T>.Settings.Adapter.BulkRemove(successSet);
                    else Info<T>.Settings.Adapter.BulkUpsert(successSet);

                    logClicker = modelSet.GetClicker($"{type} bulk   [After]");

                    logStep = _timed.Log("post-processing individual models");

                    Parallel.ForEach(resultPackage.Control.Where(i => i.Value.Success), new ParallelOptions { MaxDegreeOfParallelism = 5 }, controlModel =>
                      {
                          var key = controlModel.Key;
                          logClicker.Click();

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

                    logClicker.End();
                    _timed.End();

                    return resultPackage;
                }
                catch (Exception e)
                {
                    Current.Log.Add<T>(e);
                    var ex = new Exception($"{type} - Error while {logStep} {logObj?.ToJson()}: {e.Message}", e);

                    _timed.End();
                    throw ex;
                }
            }
        }

        internal static T FetchModel(string key, Mutator mutator = null) { return Info<T>.Settings.Adapter.Get<T>(key, mutator); }

        internal static Dictionary<string, T> FetchSet(IEnumerable<string> keys, bool ignoreCache = false, Mutator mutator = null)
        {
            //This function mixes cached models with partial queries to the adapter.
            var fetchKeys = keys.ToList();

            // First we create a map to accomodate all the requested records.
            var fetchMap = fetchKeys.ToDictionary(i => i, i => (T)null);

            //Then we proceed to probe the cache for individual model copies if the user didn't decided to ignore cache.
            var cacheKeyPrefix = mutator?.KeyPrefix;
            if (!ignoreCache)
                foreach (var key in fetchKeys)
                    fetchMap[key] = CacheFactory.FetchModel<T>(cacheKeyPrefix + key);

            //At this point we may have a map that's only partially populated. Let's then identify the missing models.
            var missedKeys = fetchMap.Where(i => i.Value == null).Select(i => i.Key).ToList();

            //Do a hard query on the missing keys.
            var cachedSet = Info<T>.Settings.Adapter.Get<T>(missedKeys, mutator).ToList();

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

            originalModel = Get(key, mutator);

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

            var localModel = (T)this;
            T storedModel = null;
            var isNew = IsNew(ref storedModel, mutator);

            var targetActionType = isNew ? EActionType.Insert : EActionType.Update;

            localModel = ProcBeforePipeline(targetActionType, EActionScope.Model, mutator, localModel, storedModel);

            if (localModel == null) return null;

            if (isNew) BeforeInsert();
            else BeforeUpdate();
            BeforeSave();

            var postKey = Info<T>.Settings.Adapter.Save(localModel).GetDataKey();

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

        public T Remove(Mutator mutator = null)
        {
            ValidateState(EActionType.Remove);

            var localModel = (T)this;
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

        public virtual Mutator BeforeQuery(EActionType read, Mutator mutator) { return null; }

        public virtual Mutator BeforeCount(EActionType read, Mutator mutator) { return null; }

        #endregion

        #region Common Attributes

        public static T GetByLocator(string locator, Mutator mutator = null)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global

            return Where(i => (i as IDataLocator).Locator == locator, mutator).FirstOrDefault();
        }

        public static T GetByCode(string code, Mutator mutator = null)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            return Where(i => (i as IDataCode).Code == code, mutator).FirstOrDefault();
        }

        #endregion

        public static IEnumerable<T> GetByLocator(IEnumerable<string> locators, Mutator mutator = null)
        {
            return Where(i => locators.Contains((i as IDataLocator).Locator), mutator);
        }
    }
}