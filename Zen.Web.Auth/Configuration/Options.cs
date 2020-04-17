using System.Collections.Generic;

namespace Zen.Web.Auth.Configuration
{
    public class Options
    {
        public enum EMode
        {
            StandAlone,
            Client
        }

        public EMode Mode { get; set; } = EMode.Client;

        public List<string> WhitelistedProviders { get; set; } 
        public Dictionary<string, Dictionary<string, string>> Provider { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public void Evaluate()
        {
            if ((Provider?.Count == 0) & (Mode == EMode.StandAlone)) Mode = EMode.Client; // No providers means that we need to rely on external auth.
        }
    }
}