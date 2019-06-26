using Hood.BaseTypes;
using Hood.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hood.Models
{
    public class PropertySettings : SaveableModel
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool ShowList { get; set; }
        public bool ShowItem { get; set; }
        public string Title { get; set; }
        public string Plural { get; set; }
        public string ListingTypes { get; set; }
        public string PriceDisplays { get; set; }
        public string RentDisplays { get; set; }
        public string PropertyType { get; set; }
        public string PlanningType { get; set; }
        public string FeesDisplay { get; set; }
        public string LeaseStatuses { get; set; }
        public string TriggerAuthKey { get; set; }
        public bool ShowBedrooms { get; set; }
        public bool ShowRent { get; set; }
        public bool ShowRentDecimals { get; set; }
        public int RentMinimum { get; set; }
        public int RentMaximum { get; set; }
        public int RentIncrement { get; set; }
        public bool ShowAskingPrice { get; set; }
        public bool ShowAskingPriceDecimals { get; set; }
        public int AskingPriceMinimum { get; set; }
        public int AskingPriceMaximum { get; set; }
        public int AskingPriceIncrement { get; set; }
        public bool ShowPremium { get; set; }
        public bool ShowPremiumDecimals { get; set; }
        public int PremiumMinimum { get; set; }
        public int PremiumMaximum { get; set; }
        public int PremiumIncrement { get; set; }
        public bool ShowFees { get; set; }
        public bool ShowFeesDecimals { get; set; }
        public int DefaultPageSize { get; set; }

        public PropertyImporterSettings FTPImporterSettings { get; set; }

        public bool IsBlmImporterEnabled
        {
            get
            {
                if (FTPImporterSettings == null) return false;
                if (FTPImporterSettings.UseFTP)
                {
                    if (!FTPImporterSettings.Server.IsSet() ||
                        !FTPImporterSettings.Password.IsSet() ||
                        !FTPImporterSettings.Filename.IsSet())
                        return false;
                }
                else
                {
                    if (!FTPImporterSettings.LocalFolder.IsSet() ||
                        !FTPImporterSettings.Filename.IsSet())
                        return false;
                }
                if (FTPImporterSettings.RequireUnzip)
                {
                    if (!FTPImporterSettings.ZipFile.IsSet())
                        return false;
                }
                return true;
            }
        }

        public PropertySettings()
        {
            Name = "Property";
            Plural = "Properties";
            Title = "Property";
            Slug = "property";
            ShowRent = true;
            ShowAskingPrice = true;
            ShowItem = true;
            ShowList = true;
            RentMinimum = 0;
            RentMaximum = 1000000;
            AskingPriceMinimum = 0;
            AskingPriceMaximum = 10000000;
            PremiumMinimum = 0;
            PremiumMaximum = 1000000;
            RentIncrement = 10000;
            AskingPriceIncrement = 50000;
            PremiumIncrement = 10000;
            Enabled = false;
            PriceDisplays = PropertyDetails.PriceQualifiers.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            RentDisplays = PropertyDetails.RentFrequency.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            FeesDisplay = PropertyDetails.Fees.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            PropertyType = PropertyDetails.PropertyTypes.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            PlanningType = PropertyDetails.PlanningTypes.OrderBy(c => c.Key).Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(string.Join(":", a.Key, a.Value)), sb => sb.ToString());
            ListingTypes = PropertyDetails.ListingTypes.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            LeaseStatuses = PropertyDetails.LeaseStatuses.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
        }

        public Dictionary<string, string> GetPlanningTypes()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (string s in Regex.Split(PlanningType, "\r\n|\r|\n"))
            {
                string[] ss = s.Split(':');
                if (ss.Length > 1)
                {
                    var key = s.Split(':')[0];
                    var itm = s.Split(':')[1];
                    if (key.Trim().IsSet() && itm.Trim().IsSet())
                        ret.Add(key, itm);
                }
            }
            return ret;
        }

        public List<string> GetListingTypes()
        {
            List<string> ret = new List<string>();
            foreach (string s in Regex.Split(ListingTypes, "\r\n|\r|\n"))
            {
                if (s.Trim().IsSet())
                    ret.Add(s);
            }
            return ret;
        }

        public List<string> GetLeaseStatuses()
        {
            List<string> ret = new List<string>();
            foreach (string s in Regex.Split(LeaseStatuses, "\r\n|\r|\n"))
            {
                if (s.Trim().IsSet())
                    ret.Add(s);
            }
            return ret;
        }

        public string GetPlanningFromType(string type)
        {
            if (!type.IsSet())
                return "Dwellinghouses";
            if (GetPlanningTypes().ContainsKey(type))
                return GetPlanningTypes()[type];
            return "Dwellinghouses";
        }
    }

}

