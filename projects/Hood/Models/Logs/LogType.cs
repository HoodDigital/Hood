using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Models
{
    public enum LogType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public enum LogSource
    {
        Identity,
        Security,
        Subscriptions,
        Api,
        Content, 
        Properties,
        Forums,
        Media,
        Themes,
        Importers,
        Exporters
    }
}
