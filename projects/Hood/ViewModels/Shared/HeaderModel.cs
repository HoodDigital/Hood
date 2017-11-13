using System.Collections.Generic;

namespace Hood.Models
{
    public class HeaderModel
    {
        public List<Content> Pages { get; internal set; }
        public ApplicationUser User { get; internal set; }
        public AccountInfo Subscription { get; internal set; }
    }
}
