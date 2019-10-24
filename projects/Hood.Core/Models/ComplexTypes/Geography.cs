using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class GeoCoordinate
    {
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsVald { get { return !((Latitude == 0) || (Longitude == 0)); } } 
    }

    public static class GeoCalculations
    {
        public const double EarthRadiusInMiles = 3956.0;
        public const double EarthRadiusInKilometers = 6367.0;

        public static double CalcDistance(this GeoCoordinate from, GeoCoordinate to)
        {
            return CalcDistance(from, to, GeoMeasurement.Miles);
        }

        public static double CalcDistance(GeoCoordinate from, GeoCoordinate to, GeoMeasurement unit)
        {
            double dist = (Math.Acos(
                Math.Sin(to.Latitude * Math.PI / 180.0) *
                Math.Sin(from.Latitude * Math.PI / 180.0) +
                Math.Cos(to.Latitude * Math.PI / 180.0) *
                Math.Cos(from.Latitude * Math.PI / 180.0) *
                Math.Cos((to.Longitude - from.Longitude) * Math.PI / 180.0)
            ) / Math.PI * 180.0) * 60 * 1.1515;
            if (unit == GeoMeasurement.Kilometers)
            {
                dist = dist * 1.609344;
            }
            else if (unit == GeoMeasurement.NauticalMiles)
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        public static GeoCoordinate GetCentralGeoCoordinate(IEnumerable<GeoCoordinate> geoCoordinates)
        {
            if (geoCoordinates.Count() == 1)
            {
                return geoCoordinates.Single();
            }

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Latitude * Math.PI / 180;
                var longitude = geoCoordinate.Longitude * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = geoCoordinates.Count();

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return new GeoCoordinate(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }
    }

    public enum GeoMeasurement : int
    {
        Miles = 0,
        Kilometers = 1,
        NauticalMiles = 2
    }


}
