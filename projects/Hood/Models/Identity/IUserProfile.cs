using Hood.Interfaces;
using System.Collections.Generic;

namespace Hood.Models
{
    /// <summary>
    /// IUserProfile is used for setting the user's profile and passing data to views, while excluding sensetive data from the IdentityUser classes.
    /// </summary>
    public interface IUserProfile : IName
    {
        string UserName { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        string Bio { get; set; }
        string CompanyName { get; set; }
        string Facebook { get; set; }
        string Instagram { get; set; }
        string JobTitle { get; set; }
        string LinkedIn { get; set; }
        string Twitter { get; set; }
        string TwitterHandle { get; set; }
        string WebsiteUrl { get; set; }
    }
}