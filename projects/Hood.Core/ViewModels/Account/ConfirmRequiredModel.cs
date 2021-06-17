using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class ConfirmRequiredModel
    {
        [FromQuery(Name = "userId")]
        public string UserId { get; set; }
    }
}