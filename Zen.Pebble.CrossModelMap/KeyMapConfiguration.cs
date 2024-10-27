using System.Collections.Generic;

namespace Zen.Pebble.CrossModelMap
{
    public class KeyMapConfiguration
    {
        public string Target { get; set; }
        public string Handler { get; set; } = "text";
        public List<string> AternateSources { get; set; }
    }
}