using System.Collections.Generic;

namespace Hood.Models
{
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

