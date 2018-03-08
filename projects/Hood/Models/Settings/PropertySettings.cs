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
        public FTPImporterSettings FTPImporterSettings { get; set; }
        public int DefaultPageSize { get; set; }

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

    public class FTPImporterSettings
    {
        public bool UseFTP { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Filename { get; set; }
    }

    public static class PropertyDetails
    {

        public static Dictionary<string, string> PlanningTypes = new Dictionary<string, string>()
        {
            { "A1",  "Shops" },
            { "A2",  "Financial and professional services" },
            { "A3",  "Restaurants and cafes" },
            { "A4",  "Drinking establishments" },
            { "A5",  "Hot food takeaways" },
            { "B1",  "Business" },
            { "B2",  "General industrial" },
            { "B8",  "Storage or distribution" },
            { "C1",  "Hotels" },
            { "C2",  "Residential institutions" },
            { "C2A", "Secure Residential Institution" },
            { "C3",  "Dwellinghouses" },
            { "C4",  "Houses in multiple occupation" },
            { "D1",  "Non-residential institutions" },
            { "D2",  "Assembly and leisure" },
            { "SG",  "Sui Generis" },
            { "VAR", "Various / Subject to Planning" }
        };

        public static Dictionary<int, string> RentFrequency = new Dictionary<int, string>()
        {
            { 0,  "{0} weekly" },
            { 1,  "{0} monthly" },
            { 2,  "{0} quarterly" },
            { 3,  "{0} annually" },
            { 5,  "{0} pppw" },
            { 101,"At a passing rent of {0}" },
            { 102,"Offers in excess of {0}" },
            { 103,"Offers in the region of {0}" },
            { 104,"Offers invited" },
            { 105,"Upon Application" },
            { 106,"Not Applicable" }
        };

        public static Dictionary<int, string> Fees = new Dictionary<int, string>()
        {
            { 0,  "{0} weekly" },
            { 1,  "{0} monthly" },
            { 2,  "{0} quarterly" },
            { 3,  "{0} annually" },
            { 5,  "{0} pppw" },
            { 6,  "{0} deposit" }
        };

        public static Dictionary<int, string> PriceQualifiers = new Dictionary<int, string>()
        {
            { 0,  "{0}" },
            { 1,  "POA" },
            { 2,  "{0} (Guide)" },
            { 3,  "{0} (Fixed)" },
            { 4,  "Offers in Excess of {0}" },
            { 5,  "Offers in the region of {0}" },
            { 6,  "Sale by Tender" },
            { 7,  "From {0}" },
            { 8,  "Shared Ownership" },
            { 9,  "Offers Over {0}" },
            { 10, "Part Buy/Part Rent" },
            { 101, "{0}" },
            { 104,"Offers invited" },
            { 105,"Upon Application" },
            { 106,"Not Applicable" }
        };

        public static Dictionary<int, string> Furnished = new Dictionary<int, string>()
        {
            { 0, "Furnished" },
            { 1, "Part Furnished" },
            { 2, "Furnished" },
            { 3, "Not Specified" },
            { 4, "Furnished/Un Furnished" }
        };

        public static Dictionary<int, string> Status = new Dictionary<int, string>()
        {
            { 0, "Available" },
            { 1, "SSTC" },
            { 2, "SSTCM" },
            { 3, "Under Offer" },
            { 4, "Reserved" },
            { 5, "Let Agreed" },
            { 6, "Sold" },
            { 101,  "New instruction" },
            { 102,  "Price reduction" },
            { 103,  "Re-available" },
            { 104,  "Under offer" }
       };

        public static Dictionary<int, string> Tenures = new Dictionary<int, string>()
        {
            { 1,  "Freehold" },
            { 2,  "Leasehold " },
            { 3,  "Feudal" },
            { 4,  "Commonhold" },
            { 5,  "Share of Freehold" },
        };

        public static Dictionary<int, string> ListingTypes = new Dictionary<int, string>()
        {
            { 0,  "Not Specified" },
            { 1,  "Long Term" },
            { 2,  "Short Term" },
            { 3,  "Student" },
            { 4,  "Commercial" },
            { 5,  "Lease for sale" },
            { 6,  "Sub-lease" },
            { 7,  "Sale" }
        };

        public static Dictionary<int, string> LeaseStatuses = new Dictionary<int, string>()
        {
            { 0,  "Available" },
            { 1,  "Sold Subject To Contract" },
            { 2,  "Sold Subject to Conclusion of Missives" },
            { 3,  "Under Offer" },
            { 4,  "Reserved" },
            { 5,  "Let Agreed" },
            { 6,  "Sold" },
            { 7,  "Let" }
        };

        public static Dictionary<int, string> PropertyTypes = new Dictionary<int, string>()
        {
            { 0,  "Not Specified" },
            { 51, "Garages" },
            { 1, "Terraced" },
            { 52, "Farm House" },
            { 2, "End of Terrace" },
            { 53, "Equestrian" },
            { 3, "Semi-Detached" },
            { 56, "Duplex" },
            { 4, "Detached" },
            { 59, "Triplex" },
            { 5, "Mews" },
            { 62, "Longere" },
            { 6, "Cluster House" },
            { 65, "Gite" },
            { 7, "Ground Flat" },
            { 68, "Barn" },
            { 8, "Flat" },
            { 71, "Trulli" },
            { 9, "Studio" },
            { 74, "Mill" },
            { 10, "Ground Maisonette" },
            { 77, "Ruins" },
            { 11, "Maisonette" },
            { 80, "Restaurant" },
            { 12, "Bungalow" },
            { 83, "Cafe" },
            { 13, "Terraced Bungalow" },
            { 86, "Mill" },
            { 14, "Semi-Detached Bungalow" },
            { 89, "Trulli" },
            { 15, "Detached Bungalow" },
            { 92, "Castle" },
            { 16, "Mobile Home" },
            { 95, "Village House" },
            { 17, "Hotel" },
            { 101, "Cave House" },
            { 18, "Guest House" },
            { 104, "Cortijo" },
            { 19, "Commercial Property" },
            { 107, "Farm Land" },
            { 20, "Land" },
            { 110, "Plot" },
            { 21, "Link Detached House" },
            { 113, "Country House" },
            { 22, "Town House" },
            { 116, "Stone House" },
            { 23, "Cottage" },
            { 117, "Caravan" },
            { 24, "Chalet" },
            { 118, "Lodge" },
            { 27, "Villa" },
            { 119, "Log Cabin" },
            { 28, "Apartment" },
            { 120, "Manor House" },
            { 29, "Penthouse" },
            { 121, "Stately Home" },
            { 30, "Finca" },
            { 125, "Off-Plan" },
            { 43, "Barn Conversion" },
            { 128, "Semi-detached Villa" },
            { 44, "Serviced Apartments" },
            { 131, "Detached Villa" },
            { 45, "Parking" },
            { 134, "Bar" },
            { 46, "Sheltered Housing" },
            { 137, "Shop" },
            { 47, "Retirement Property" },
            { 140, "Riad" },
            { 48, "House Share" },
            { 141, "House Boat" },
            { 49, "Flat Share" },
            { 142, "Hotel Room" }
        };


    }

}

