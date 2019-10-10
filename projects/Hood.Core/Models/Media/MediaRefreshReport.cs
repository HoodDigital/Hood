namespace Hood.Models
{
    public class MediaRefreshReport
    {
        public int Total { get; set; }
        public int Processed { get; set; }
        public double Complete { get; set; }
        public string StatusMessage { get; set; }
        public bool Running { get; set; }
        public bool Succeeded { get; internal set; }
        public bool HasRun { get; internal set; }
    }

}