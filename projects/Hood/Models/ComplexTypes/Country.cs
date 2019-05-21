using System.Collections.Generic;

namespace Hood.Models
{
    public class Country
    {
        public string Iso3 { get; set; }
        public string Iso2 { get; set; }
        public string IsoNumeric { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }

        public bool IsEurope
        {
            get
            {
                var europe = new List<string>()
                {
                    "AD",
                    "AT",
                    "BE",
                    "BG",
                    "HR",
                    "CY",
                    "CZ",
                    "DK",
                    "EE",
                    "FO",
                    "FI",
                    "FR",
                    "DE",
                    "GI",
                    "GR",
                    "GL",
                    "GG",
                    "VA",
                    "HU",
                    "IS",
                    "IE",
                    "IM",
                    "IL",
                    "IT",
                    "JE",
                    "LV",
                    "LI",
                    "LT",
                    "LU",
                    "MK",
                    "MT",
                    "MC",
                    "ME",
                    "NL",
                    "NO",
                    "PL",
                    "PT",
                    "RO",
                    "PM",
                    "SM",
                    "RS",
                    "SK",
                    "SI",
                    "ES",
                    "SJ",
                    "SE",
                    "CH",
                    "TR",
                    "GB",
                    };
                europe.ForEach(e => e = e.ToLower());
                if (europe.Contains(Iso2.ToLower()))
                    return true;
                else return false;
            }
        }

        public Country(string name, string fullName, string iso2, string iso3, string numeric, string currencySymbol, string currencyName)
        {
            Iso2 = iso2;
            Iso3 = iso2;
            IsoNumeric = numeric;
            Name = name;
            FullName = fullName;
            CurrencySymbol = currencySymbol;
            CurrencyName = currencyName;
        }
    }
}
