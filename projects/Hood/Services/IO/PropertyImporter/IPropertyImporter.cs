using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IPropertyImporter
    {
        bool IsComplete();
        bool RunUpdate(HttpContext context);
        void Kill();
        PropertyDataImporterReport Report();
    }

    public class PropertyDataImporterReport
    {
        public int Total { get;set; }
        public int Updated { get;set; }
        public int Added { get; set; }
        public int Processed { get; set; }
        public int Deleted { get; set; }
        public int ToAdd { get; set; }
        public int ToUpdate { get; set; }
        public int ToDelete { get; set; }
        public double Complete { get;set; }
        public string StatusMessage { get;set; }
        public bool Running { get;set; }
        public List<string> Errors { get; internal set; }
        public List<string> Warnings { get; internal set; }
    }
}