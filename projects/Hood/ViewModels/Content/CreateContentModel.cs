using System;
namespace Hood.Models
{
    public class CreateContentModel
    {
        public int cpStatus { get; set; }
        public string cpTitle { get; set; }
        public string cpExcept { get; set; }
        public DateTime cpPublishDate { get; set; }
        public string cpType { get; set; }
        public int cpPublishHour { get; set; }
        public int cpPublishMinute { get; set; }
        public string cpCategory { get; set; }
    }
}