using System;
using System.Collections.Generic;
using Zen.Pebble.Geo.Shared;

namespace Zen.Pebble.FlexibleData.Historical
{
    public class GeoBoundary : List<Historic<List<GeoPolygon>>>
    {
    }

    public class Geography
    {
        private static readonly char[] _delimiters = {',', ';', ' ', '\n'};
        public LatLng Position { get; set; }
        public GeoBoundary Boundaries { get; set; }

        public static implicit operator Geography(string coordinates)
        {
            var coordinateMembers = coordinates.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (coordinateMembers.Length != 2) throw new ArgumentException(nameof(coordinates));

            var latParse = double.TryParse(coordinateMembers[0], out var lat);
            if (!latParse) throw new ArgumentException($"Invalid latitude: {coordinateMembers[0]}");

            if (lat < -90 || lat > 90)
                throw new ArgumentOutOfRangeException("Latitude must be between -90 and 90 degrees inclusive.");

            var lngParse = double.TryParse(coordinateMembers[1], out var lng);
            if (!lngParse) throw new ArgumentException($"Invalid Longitude: {coordinateMembers[1]}");

            if (lng < -90 || lng > 90)
                throw new ArgumentOutOfRangeException("Longitude must be between -90 and 90 degrees inclusive.");

            return new Geography {Position = new LatLng {latitude = lat, longitude = lng}};
        }

        public static implicit operator string(Geography source)
        {
            return source.Position.ToString();
        }
    }
}