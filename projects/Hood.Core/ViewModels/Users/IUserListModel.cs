using System.Collections.Generic;
using Hood.Interfaces;
using Hood.Models;

namespace Hood.ViewModels
{
    public interface IUserListModel : IPagedList<UserProfile>, IPageableModel
    {
        string Role { get; set; }
        bool Unused { get; set; }
        bool Inactive { get; set; }
        bool Active { get; set; }
        bool PhoneUnconfirmed { get; set; }
        bool EmailUnconfirmed { get; set; }
        string Subscription { get; set; }
        List<string> RoleIds { get; set; }
        List<string> SubscriptionIds { get; set; }
    }
}
