using Hood.BaseTypes;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ApiViewModel : SaveableModel
    {
        public string Title { get; set; }
        public string Details { get; set; }
    }
}
