using Newtonsoft.Json.Linq;

namespace Zen.App.Orchestrator
{
    public interface IZenPreference
    {
        string Id { get; set; }
        string PersonLocator { get; set; }
        string ApplicationCode { get; set; }
        string RawMarshalledValue { get; set; }

        JObject GetValues();

        IZenPreference GetCurrent();
        void Put(JObject pValue, string personLocator = null, string appLocator = null);
    }
}