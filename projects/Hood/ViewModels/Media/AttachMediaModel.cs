using System.Collections.Generic;

namespace Hood.Models
{
    public class AttachMediaModel
    {
        public string Id { get; set; }
        public string Entity { get; set; }
        public string Field { get; set; }
        public string Type { get; set; }
        public string Refresh { get; set; }
        public string Tag { get; set; }
        public int MediaId { get; set; }

        public List<string> Directories { get; set; }
    }
}