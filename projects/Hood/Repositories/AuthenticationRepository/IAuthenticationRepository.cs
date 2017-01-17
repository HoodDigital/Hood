using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Hood.Infrastructure;
using Hood.Models;

namespace Hood.Services
{
    public interface IAuthenticationRepository
    {
        AccountInfo GetAccountInfo(string userId);
        ApplicationUser GetCurrentUser(bool cached = true, bool track = true);
        ApplicationUser GetUserById(string userId, bool cached = true, bool track = true);
        Task<ApplicationUser> GetUserByStripeId(string stripeId);
        OperationResult UpdateUser(ApplicationUser user);
        OperationResult DeleteAddress(int id);
        IList<IdentityRole> GetAllRoles();
        Address GetAddressById(int id);
        OperationResult UpdateAddress(Address address);
        OperationResult SetBillingAddress(string userId, int id);
        OperationResult SetDeliveryAddress(string userId, int id);
        void ResetBillingInfo();
        void ClearUserFromCache(string userId);
        void ClearUserFromCache(ApplicationUser user);
        OperationResult AddUserSubscription(UserSubscription newUserSub);
        OperationResult UpdateUserSubscription(UserSubscription newUserSub);
    }
}