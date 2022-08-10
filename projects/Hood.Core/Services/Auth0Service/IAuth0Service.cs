using System.Collections.Generic;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Hood.Models;

namespace Hood.Services
{
    public interface IAuth0Service
    {
        Task AssignRolesToUser(string userId, string[] roleIds);
        Task<Ticket> CreatePasswordChangeTicket(Auth0Identity remoteUser);
        Task<User> CreateUserWithPassword(Auth0User user, string password);
        Task DeleteRole(string roleId);
        Task DeleteUser(string userId);
        Task<Ticket> GetEmailVerificationTicket(string accountId, string returnUrl);
        Task<Role> GetRoleById(string roleId);
        Task<Role> GetRoleByName(string roleName);
        Task<System.Collections.Generic.IPagedList<Role>> GetRoles(string search = "", int page = 0, int pageSize = 50);
        Task<System.Collections.Generic.PagedList<Role>> GetRolesForUser(string userId, int page = 0, int pageSize = 50);
        Task<User> GetUserById(string userId);
        Task<System.Collections.Generic.IPagedList<User>> GetUsers(string search = "", int page = 0, int pageSize = 50);
        Task<IList<User>> GetUsersByEmail(string email);
        Task LinkAccountAsync(Auth0Identity primaryAccount, string fullAuthUserId);
        Task RemoveRolesFromUser(string userId, string[] roleIds);
        Task UnlinkAccountAsync(Auth0Identity primaryAccount, Auth0Identity identityToDisconnect);
        Task<User> UpdateUser(string userId, UserUpdateRequest updateRequest);
        Task<Role> CreateRoleForLocalRole(Auth0Role role);
    }
}