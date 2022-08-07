using System.Collections.Generic;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Hood.Models;

namespace Hood.Services
{
    public interface IAuth0Service
    {
        Task<Auth0Identity> CreateLocalAuthIdentity(string fullAuthUserId, Auth0User user, string picture);
        Task<User> CreateUserWithPassword(Auth0User user, string password);
        Task DeleteLocalAuthIdentity(string id);
        Task DeleteUser(string userId);
        Task<ManagementApiClient> GetClientAsync();
        Task<Ticket> GetEmailVerificationTicket(string accountId, string returnUrl);
        Task GetLocalAuthIdentity(string userId);
        Task<Auth0User> GetUserByAuth0Id(string id);
        Task<Auth0User> GetUserByAuth0UserId(string userId);
        Task<IList<User>> GetUserByEmail(string email);
        Task<User> GetUserById(string userId);
        Task<System.Collections.Generic.IPagedList<User>> GetUsers(string search = "", int page = 0, int pageSize = 50);
        Task UpdateLocalAuthIdentity(Auth0Identity user);
    }
}