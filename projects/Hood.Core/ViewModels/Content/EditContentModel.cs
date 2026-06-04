using System.Collections.Generic;
using Hood.BaseTypes;
using Hood.Models;

namespace Hood.ViewModels
{
    public class EditContentModel : SaveableModel
    {
        public Content Content { get; set; }
        public ContentType ContentType { get; set; }
        public List<ContentCategory> Categories { get; set; }
        public Dictionary<string, string> Templates { get; set; }
        public IList<ApplicationUser> Authors { get; internal set; }
    }
}
