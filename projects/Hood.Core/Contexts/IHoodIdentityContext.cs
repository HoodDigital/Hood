using System.Threading.Tasks;
using Hood.Models;

namespace Hood.Contexts
{
    public interface IHoodIdentityContext
    {
        Task<IHoodIdentity> GetSiteAdmin();
    }
}