using System.Collections.Generic;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Hood.Models;

namespace Hood.Services
{
    public interface IAuth0Service
    {
        Task<User> CreateUserWithPassword(Auth0User user, string password);
        Task DeleteUser(string userId);
        Task<Ticket> GetEmailVerificationTicket(string accountId, string returnUrl);
        Task<User> GetUserById(string userId);
        Task<System.Collections.Generic.IPagedList<User>> GetUsers(string search = "", int page = 0, int pageSize = 50);
        Task<IList<User>> GetUsersByEmail(string email);
        Task LinkAccountAsync(Auth0Identity primaryAccount, string v);
        Task UnlinkAccountAsync(Auth0Identity primaryAccount, Auth0Identity identityToDisconnect);
        Task<Ticket> CreatePasswordChangeTicket(Auth0Identity remoteUser);
    }
}