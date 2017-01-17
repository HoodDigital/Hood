using System;
namespace Hood.Models
{
    public class CreatePropertyModel
    {
        public int cpStatus { get; set; }
        public string cpTitle { get; set; }
        public DateTime cpPublishDate { get; set; }
        public string cpType { get; set; }
        public string cpPostcode { get; set; }
        public string cpNumber { get; set; }
        public string cpAddress1 { get; set; }
        public string cpAddress2 { get; set; }
        public string cpCity { get; set; }
        public string cpCounty { get; set; }
        public string cpCountry { get; set; }
        public double cpLatitude { get; set; }
        public double cpLongitude { get; set; }
        public int cpPublishHour { get; set; }
        public int cpPublishMinute { get; set; }
    }
}