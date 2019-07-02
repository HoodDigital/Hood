using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class HeaderModel
    {
        public List<Content> Pages { get; internal set; }
        public ApplicationUser User { get; internal set; }
        public AccountInfo Subscription { get; internal set; }
    }
}
