using Hood.Infrastructure;
using System.Collections.Generic;

namespace Hood.Models
{
    public class EditPropertyModel
    {
        public PropertyListing Property { get; set; }
        public IList<ApplicationUser> Agents { get; internal set; }
        public OperationResult SaveResult { get; internal set; }
    }

    public class EditPropertyModelSend : PropertyListing
    {
    }
}