using System.Collections.Generic;

namespace Hood.Models
{

    public class ContentBlockSettings
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public ContentBlockVariable[] Variables { get; set; }
    }

    public class ContentBlockVariable
    {
        public string Name { get; set; }
        public string VariableName { get; set; }
        public string Type { get; set; }
        public KeyValuePair<string, string>[] Values { get; set; }
        public string Value { get; set; }
    }

}
