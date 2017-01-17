using System.Collections.Generic;
using Hood.Infrastructure;
using Hood.Models.Api;

namespace Hood.Models
{
    public class EditPropertyModel
    {
        public PropertyListing Property { get; set; }
        public IList<ApplicationUser> Agents { get; internal set; }
        public OperationResult SaveResult { get; internal set; }
    }
    public class EditPropertyModelSend : PropertyListingApi
    {
    }
}