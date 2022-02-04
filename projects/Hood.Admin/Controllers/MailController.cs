using Hood.Core;
using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using Hood.Models;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Hood.Identity.Policies.Active, Roles="SuperUser,Admin")]

    public class MailController : BaseMailController
    {
        public MailController()
            : base()
        {
        }
    }
}