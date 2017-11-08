using System.Collections.Generic;

namespace Hood.Models
{
    public class HeaderModel<TUser> where TUser : IHoodUser
    {
        public List<Content<TUser>> Pages { get; internal set; }
        public TUser User { get; internal set; }
        public AccountInfo<TUser> Subscription { get; internal set; }
    }
}
