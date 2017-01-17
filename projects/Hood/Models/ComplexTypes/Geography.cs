using System;

namespace Hood.Models
{
    public class GeoCoordinate
    {
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
    }

    public enum GeoMeasurement : int
    {
        Miles = 0,
        Kilometers = 1,
        NauticalMiles = 2
    }


}
