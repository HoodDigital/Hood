using Hood.Models;
using Hood.Services;
using Hood.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Hood.Controllers;

namespace Hood.Demo.Controllers
{
    public class AccountController : Hood.Controllers.AccountController
    {
        public AccountController() 
            : base()
        { }
    }
}
