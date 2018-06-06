using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.Models
{
    public class ApiViewModel : ISaveableModel
    {
        public AlertType MessageType { get; set; }
        public string SaveMessage { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }
}
