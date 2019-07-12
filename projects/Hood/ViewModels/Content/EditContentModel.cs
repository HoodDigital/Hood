using Hood.BaseTypes;
using Hood.Models;
using System;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public partial class EditContentModel : SaveableModel
    {
        public Content Content { get; set; }
        public ContentType ContentType { get; set; }
        public List<ContentCategory> Categories { get; set; }
        public Dictionary<string, string> Templates { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; }
        public IList<ApplicationUser> Authors { get; internal set; }
    }
}