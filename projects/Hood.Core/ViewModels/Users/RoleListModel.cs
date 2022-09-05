using System.Collections.Generic;
using Hood.Interfaces;

namespace Hood.ViewModels
{
    public class RoleListModel<TRole> : PagedList<TRole>, IPageableModel
    {        
        public RoleListModel()
        { }
    }
}
