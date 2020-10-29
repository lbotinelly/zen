using System.Collections.Generic;

namespace Zen.Pebble.Geo.Shared
{
    public class GeoPolygon
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<LatLng> vertices = new List<LatLng>();
    }
}