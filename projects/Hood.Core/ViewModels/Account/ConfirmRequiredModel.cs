using Hood.BaseTypes;
using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class ConfirmRequiredModel : SaveableModel
    {
        [FromQuery(Name = "userId")]
        public string UserId { get; set; }
    }
}