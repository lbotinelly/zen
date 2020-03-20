using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.App.Orchestrator
{
    public class ZenPreference<T> : Data<T> where T : Data<T>, new()
    {
        public enum EScope
        {
            Hidden,
            Personal,
            Internal,
            Public
        }

        public JObject GetValues()
        {
            var co = ((IZenPreference) this).RawMarshalledValue ?? "{}";

            return JObject.Parse(co);
        }

        public T GetKeyValue(string key, string userLocator = null, string appLocator = null)
        {
            if (userLocator == null) userLocator = Current.Orchestrator?.Person?.Locator;
            if (appLocator == null) appLocator = Current.Orchestrator?.Application?.Code;

            var keySearch = $"['{key}']";

            T ret;

            try { ret = ((IZenPreference) Get(userLocator, appLocator)).GetValues().SelectToken(keySearch).ToObject<T>(); } catch (Exception) { ret = Activator.CreateInstance<T>(); }

            return ret;
        }

        public void SetKeyValue(string key, object content, EScope scope = EScope.Hidden, string userLocator = null, string appLocator = null)
        {
            if (userLocator == null) userLocator = Current.Orchestrator?.Person?.Locator;
            if (appLocator == null) appLocator = Current.Orchestrator?.Application?.Code;

            var obj = JObject.Parse(new Entry {Key = key, Scope = scope, Proposed = null, Value = content}.ToJson());

            Put(obj, userLocator, appLocator);
        }

        public T GetValue(string key)
        {
            var probe = GetValues().SelectToken(key);

            if (probe == null) return default;

            try { return (T) Convert.ChangeType(probe, typeof(T)); } catch { }

            // Last chance.

            return probe.ToString().FromJson<T>();
        }

        public void PatchValue(object content)
        {
            var c = GetValues();
            c.Merge(content, new JsonMergeSettings {MergeArrayHandling = MergeArrayHandling.Replace});

            ((IZenPreference) this).RawMarshalledValue = c.ToJson();
        }

        public T Get(string personLocator, string applicationCode = null)
        {
            if (applicationCode == null) applicationCode = Current.Orchestrator?.Application?.Code;

            var q = new {PersonLocator = personLocator, ApplicationCode = applicationCode}.ToJson();
            return Query(q).FirstOrDefault();
        }

        public void Patch(JObject pValue, string personLocator = null)
        {
            if (Current.Orchestrator?.Person?.Locator == null) throw new UnauthorizedAccessException("No user signed in.");

            var src = Get(Current.Orchestrator?.Person.Locator, Current.Orchestrator?.Application.Code);

            if (src == null)
            {
                src = new T();
                ((IZenPreference) src).PersonLocator = Current.Orchestrator?.Person.Locator;
            }

            var c = ((IZenPreference) src).GetValues();

            c.Merge(pValue, new JsonMergeSettings {MergeArrayHandling = MergeArrayHandling.Replace});
            ((IZenPreference) src).RawMarshalledValue = c.ToJson();
            src.Save();
        }

        public void Put(JObject pValue, string personLocator = null, string appLocator = null)
        {
            var refPerson = personLocator ?? Current.Orchestrator?.Person?.Locator;
            var refApp = appLocator ?? Current.Orchestrator?.Application?.Code;

            if (refPerson == null) throw new UnauthorizedAccessException("No user specified or signed in.");
            if (refApp == null) throw new InvalidDataException("No app specified or loaded.");

            var src = Get(refPerson, refApp);

            var prefSource = (IZenPreference) src;

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (src == null)
            {
                src = new T();
                prefSource = (IZenPreference) src;

                prefSource.PersonLocator = Current.Orchestrator?.Person.Locator;
                prefSource.ApplicationCode = refApp;
            }

            prefSource.RawMarshalledValue = pValue.ToJson();
            src.Save();
        }

        public IZenPreference GetCurrent()
        {
            if (Current.Orchestrator?.Person?.Locator == null) throw new UnauthorizedAccessException("No user signed in.");

            var src = Get(Current.Orchestrator?.Person.Locator, Current.Orchestrator?.Application.Code);

            if (src!= null) return (IZenPreference) src;

            src = new T();
            var prefSource = (IZenPreference) src;
            prefSource.PersonLocator = Current.Orchestrator?.Person.Locator;

            return prefSource;
        }

        public class Entry
        {
            public string Key { get; set; }
            public string Proposed { get; set; }
            public EScope Scope { get; set; }
            public object Value { get; set; }
        }
    }
}