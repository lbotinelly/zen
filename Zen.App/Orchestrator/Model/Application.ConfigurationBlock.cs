namespace Zen.App.Orchestrator.Model
{
    public partial class Application
    {
        public class ConfigurationBlock
        {
            public string Url { get; set; }  
            public string VersionTag { get; set; }
            public bool IsLegacy { get; set; }
        }
    }
}