using Geocoding.Google;
using Hood.BaseTypes;
using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Hood.Identity.Policies.Active, Roles = "SuperUser,Admin")]
    public class SettingsController : BaseSettingsController
    {
        public SettingsController()
            : base()
        {
        }
    }
}
