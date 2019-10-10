using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

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

    public class Countries
    {
        public static Country GetCountry(string name)
        {
            var country = All.Where(c => c.Name == name).FirstOrDefault();
            return country;
        }

        public static List<Country> All
        {
            get
            {
                List<Country> dictionary = new List<Country>
                {
                    new Country("Afghanistan", "Afghanistan", "AF", "AFG", "4", "AFN", "Afghani"),
                    new Country("Albania", "Albania", "AL", "ALB", "8", "ALL", "Lek"),
                    new Country("Algeria", "Algeria", "DZ", "DZA", "12", "DZD", "Algerian Dinar"),
                    new Country("American Samoa", "American Samoa", "AS", "ASM", "16", "USD", "US Dollar"),
                    new Country("Andorra", "Andorra", "AD", "AND", "20", "EUR", "Euro"),
                    new Country("Angola", "Angola", "AO", "AGO", "24", "AOA", "Kwanza"),
                    new Country("Anguilla", "Anguilla", "AI", "AIA", "660", "XCD", "East Caribbean Dollar"),
                    new Country("Antarctica", "Antarctica", "AQ", "ATA", "10", "", "No universal currency"),
                    new Country("Antigua & Barbuda", "Antigua and Barbuda", "AG", "ATG", "28", "XCD", "East Caribbean Dollar"),
                    new Country("Argentina", "Argentina", "AR", "ARG", "32", "ARS", "Argentine Peso"),
                    new Country("Armenia", "Armenia", "AM", "ARM", "51", "AMD", "Armenian Dram"),
                    new Country("Aruba", "Aruba", "AW", "ABW", "533", "AWG", "Aruban Florin"),
                    new Country("Australia", "Australia", "AU", "AUS", "36", "AUD", "Australian Dollar"),
                    new Country("Austria", "Austria", "AT", "AUT", "40", "EUR", "Euro"),
                    new Country("Azerbaijan", "Azerbaijan", "AZ", "AZE", "31", "AZN", "Azerbaijanian Manat"),
                    new Country("Bahamas", "Bahamas", "BS", "BHS", "44", "BSD", "Bahamian Dollar"),
                    new Country("Bahrain", "Bahrain", "BH", "BHR", "48", "BHD", "Bahraini Dinar"),
                    new Country("Bangladesh", "Bangladesh", "BD", "BGD", "50", "BDT", "Taka"),
                    new Country("Barbados", "Barbados", "BB", "BRB", "52", "BBD", "Barbados Dollar"),
                    new Country("Belarus", "Belarus", "BY", "BLR", "112", "BYR", "Belarussian Ruble"),
                    new Country("Belgium", "Belgium", "BE", "BEL", "56", "EUR", "Euro"),
                    new Country("Belize", "Belize", "BZ", "BLZ", "84", "BZD", "Belize Dollar"),
                    new Country("Benin", "Benin", "BJ", "BEN", "204", "XOF", "CFA Franc BCEAO"),
                    new Country("Bermuda", "Bermuda", "BM", "BMU", "60", "BMD", "Bermudian Dollar"),
                    new Country("Bhutan", "Bhutan", "BT", "BTN", "64", "INR", "Indian Rupee"),
                    new Country("Bolivia", "Bolivia - Plurinational State of", "BO", "BOL", "68", "BOB", "Boliviano"),
                    new Country("Caribbean Netherlands", "Bonaire - Sint Eustatius and Saba", "BQ", "BES", "535", "USD", "US Dollar"),
                    new Country("Bosnia", "Bosnia and Herzegovina", "BA", "BIH", "70", "BAM", "Convertible Mark"),
                    new Country("Botswana", "Botswana", "BW", "BWA", "72", "BWP", "Pula"),
                    new Country("Bouvet Island", "Bouvet Island", "BV", "BVT", "74", "NOK", "Norwegian Krone"),
                    new Country("Brazil", "Brazil", "BR", "BRA", "76", "BRL", "Brazilian Real"),
                    new Country("British Indian Ocean Territory", "British Indian Ocean Territory", "IO", "IOT", "86", "USD", "US Dollar"),
                    new Country("Brunei", "Brunei Darussalam", "BN", "BRN", "96", "BND", "Brunei Dollar"),
                    new Country("Bulgaria", "Bulgaria", "BG", "BGR", "100", "BGN", "Bulgarian Lev"),
                    new Country("Burkina Faso", "Burkina Faso", "BF", "BFA", "854", "XOF", "CFA Franc BCEAO"),
                    new Country("Burundi", "Burundi", "BI", "BDI", "108", "BIF", "Burundi Franc"),
                    new Country("Cambodia", "Cambodia", "KH", "KHM", "116", "KHR", "Riel"),
                    new Country("Cameroon", "Cameroon", "CM", "CMR", "120", "XAF", "CFA Franc BEAC"),
                    new Country("Canada", "Canada", "CA", "CAN", "124", "CAD", "Canadian Dollar"),
                    new Country("Cape Verde", "Cape Verde", "CV", "CPV", "132", "CVE", "Cabo Verde Escudo"),
                    new Country("Cayman Islands", "Cayman Islands", "KY", "CYM", "136", "KYD", "Cayman Islands Dollar"),
                    new Country("Central African Republic", "Central African Republic", "CF", "CAF", "140", "XAF", "CFA Franc BEAC"),
                    new Country("Chad", "Chad", "TD", "TCD", "148", "XAF", "CFA Franc BEAC"),
                    new Country("Chile", "Chile", "CL", "CHL", "152", "CLP", "Chilean Peso"),
                    new Country("China", "China", "CN", "CHN", "156", "CNY", "Yuan Renminbi"),
                    new Country("Christmas Island", "Christmas Island", "CX", "CXR", "162", "AUD", "Australian Dollar"),
                    new Country("Cocos (Keeling) Islands", "Cocos (Keeling) Islands", "CC", "CCK", "166", "AUD", "Australian Dollar"),
                    new Country("Colombia", "Colombia", "CO", "COL", "170", "COP", "Colombian Peso"),
                    new Country("Comoros", "Comoros", "KM", "COM", "174", "KMF", "Comoro Franc"),
                    new Country("Congo - Brazzaville", "Congo", "CG", "COG", "178", "XAF", "CFA Franc BEAC"),
                    new Country("Congo - Kinshasa", "Congo - the Democratic Republic of the", "CD", "COD", "180", "", ""),
                    new Country("Cook Islands", "Cook Islands", "CK", "COK", "184", "NZD", "New Zealand Dollar"),
                    new Country("Costa Rica", "Costa Rica", "CR", "CRI", "188", "CRC", "Costa Rican Colon"),
                    new Country("Croatia", "Croatia", "HR", "HRV", "191", "HRK", "Croatian Kuna"),
                    new Country("Cuba", "Cuba", "CU", "CUB", "192", "CUP", "Cuban Peso"),
                    new Country("Curaçao", "Curaçao", "CW", "CUW", "531", "ANG", "Netherlands Antillean Guilder"),
                    new Country("Cyprus", "Cyprus", "CY", "CYP", "196", "EUR", "Euro"),
                    new Country("Czech Republic", "Czech Republic", "CZ", "CZE", "203", "CZK", "Czech Koruna"),
                    new Country("Côte d’Ivoire", "Côte d'Ivoire", "CI", "CIV", "384", "XOF", "CFA Franc BCEAO"),
                    new Country("Denmark", "Denmark", "DK", "DNK", "208", "DKK", "Danish Krone"),
                    new Country("Djibouti", "Djibouti", "DJ", "DJI", "262", "DJF", "Djibouti Franc"),
                    new Country("Dominica", "Dominica", "DM", "DMA", "212", "XCD", "East Caribbean Dollar"),
                    new Country("Dominican Republic", "Dominican Republic", "DO", "DOM", "214", "DOP", "Dominican Peso"),
                    new Country("Ecuador", "Ecuador", "EC", "ECU", "218", "USD", "US Dollar"),
                    new Country("Egypt", "Egypt", "EG", "EGY", "818", "EGP", "Egyptian Pound"),
                    new Country("El Salvador", "El Salvador", "SV", "SLV", "222", "USD", "US Dollar"),
                    new Country("Equatorial Guinea", "Equatorial Guinea", "GQ", "GNQ", "226", "XAF", "CFA Franc BEAC"),
                    new Country("Eritrea", "Eritrea", "ER", "ERI", "232", "ERN", "Nakfa"),
                    new Country("Estonia", "Estonia", "EE", "EST", "233", "EUR", "Euro"),
                    new Country("Ethiopia", "Ethiopia", "ET", "ETH", "231", "ETB", "Ethiopian Birr"),
                    new Country("Falkland Islands", "Falkland Islands (Malvinas)", "FK", "FLK", "238", "FKP", "Falkland Islands Pound"),
                    new Country("Faroe Islands", "Faroe Islands", "FO", "FRO", "234", "DKK", "Danish Krone"),
                    new Country("Fiji", "Fiji", "FJ", "FJI", "242", "FJD", "Fiji Dollar"),
                    new Country("Finland", "Finland", "FI", "FIN", "246", "EUR", "Euro"),
                    new Country("France", "France", "FR", "FRA", "250", "EUR", "Euro"),
                    new Country("French Guiana", "French Guiana", "GF", "GUF", "254", "EUR", "Euro"),
                    new Country("French Polynesia", "French Polynesia", "PF", "PYF", "258", "XPF", "CFP Franc"),
                    new Country("French Southern Territories", "French Southern Territories", "TF", "ATF", "260", "EUR", "Euro"),
                    new Country("Gabon", "Gabon", "GA", "GAB", "266", "XAF", "CFA Franc BEAC"),
                    new Country("Gambia", "Gambia", "GM", "GMB", "270", "GMD", "Dalasi"),
                    new Country("Georgia", "Georgia", "GE", "GEO", "268", "GEL", "Lari"),
                    new Country("Germany", "Germany", "DE", "DEU", "276", "EUR", "Euro"),
                    new Country("Ghana", "Ghana", "GH", "GHA", "288", "GHS", "Ghana Cedi"),
                    new Country("Gibraltar", "Gibraltar", "GI", "GIB", "292", "GIP", "Gibraltar Pound"),
                    new Country("Greece", "Greece", "GR", "GRC", "300", "EUR", "Euro"),
                    new Country("Greenland", "Greenland", "GL", "GRL", "304", "DKK", "Danish Krone"),
                    new Country("Grenada", "Grenada", "GD", "GRD", "308", "XCD", "East Caribbean Dollar"),
                    new Country("Guadeloupe", "Guadeloupe", "GP", "GLP", "312", "EUR", "Euro"),
                    new Country("Guam", "Guam", "GU", "GUM", "316", "USD", "US Dollar"),
                    new Country("Guatemala", "Guatemala", "GT", "GTM", "320", "GTQ", "Quetzal"),
                    new Country("Guernsey", "Guernsey", "GG", "GGY", "831", "GBP", "Pound Sterling"),
                    new Country("Guinea", "Guinea", "GN", "GIN", "324", "GNF", "Guinea Franc"),
                    new Country("Guinea-Bissau", "Guinea-Bissau", "GW", "GNB", "624", "XOF", "CFA Franc BCEAO"),
                    new Country("Guyana", "Guyana", "GY", "GUY", "328", "GYD", "Guyana Dollar"),
                    new Country("Haiti", "Haiti", "HT", "HTI", "332", "USD", "US Dollar"),
                    new Country("Heard & McDonald Islands", "Heard Island and McDonald Islands", "HM", "HMD", "334", "AUD", "Australian Dollar"),
                    new Country("Vatican City", "Holy See (Vatican City State)", "VA", "VAT", "336", "EUR", "Euro"),
                    new Country("Honduras", "Honduras", "HN", "HND", "340", "HNL", "Lempira"),
                    new Country("Hong Kong", "Hong Kong", "HK", "HKG", "344", "HKD", "Hong Kong Dollar"),
                    new Country("Hungary", "Hungary", "HU", "HUN", "348", "HUF", "Forint"),
                    new Country("Iceland", "Iceland", "IS", "ISL", "352", "ISK", "Iceland Krona"),
                    new Country("India", "India", "IN", "IND", "356", "INR", "Indian Rupee"),
                    new Country("Indonesia", "Indonesia", "ID", "IDN", "360", "IDR", "Rupiah"),
                    new Country("Iran", "Iran - Islamic Republic of", "IR", "IRN", "364", "IRR", "Iranian Rial"),
                    new Country("Iraq", "Iraq", "IQ", "IRQ", "368", "IQD", "Iraqi Dinar"),
                    new Country("Ireland", "Ireland", "IE", "IRL", "372", "EUR", "Euro"),
                    new Country("Isle of Man", "Isle of Man", "IM", "IMN", "833", "GBP", "Pound Sterling"),
                    new Country("Israel", "Israel", "IL", "ISR", "376", "ILS", "New Israeli Sheqel"),
                    new Country("Italy", "Italy", "IT", "ITA", "380", "EUR", "Euro"),
                    new Country("Jamaica", "Jamaica", "JM", "JAM", "388", "JMD", "Jamaican Dollar"),
                    new Country("Japan", "Japan", "JP", "JPN", "392", "JPY", "Yen"),
                    new Country("Jersey", "Jersey", "JE", "JEY", "832", "GBP", "Pound Sterling"),
                    new Country("Jordan", "Jordan", "JO", "JOR", "400", "JOD", "Jordanian Dinar"),
                    new Country("Kazakhstan", "Kazakhstan", "KZ", "KAZ", "398", "KZT", "Tenge"),
                    new Country("Kenya", "Kenya", "KE", "KEN", "404", "KES", "Kenyan Shilling"),
                    new Country("Kiribati", "Kiribati", "KI", "KIR", "296", "AUD", "Australian Dollar"),
                    new Country("North Korea", "Korea - Democratic People's Republic of", "KP", "PRK", "408", "KPW", "North Korean Won"),
                    new Country("South Korea", "Korea - Republic of", "KR", "KOR", "410", "KRW", "Won"),
                    new Country("Kuwait", "Kuwait", "KW", "KWT", "414", "KWD", "Kuwaiti Dinar"),
                    new Country("Kyrgyzstan", "Kyrgyzstan", "KG", "KGZ", "417", "KGS", "Som"),
                    new Country("Laos", "Lao People's Democratic Republic", "LA", "LAO", "418", "LAK", "Kip"),
                    new Country("Latvia", "Latvia", "LV", "LVA", "428", "EUR", "Euro"),
                    new Country("Lebanon", "Lebanon", "LB", "LBN", "422", "LBP", "Lebanese Pound"),
                    new Country("Lesotho", "Lesotho", "LS", "LSO", "426", "ZAR", "Rand"),
                    new Country("Liberia", "Liberia", "LR", "LBR", "430", "LRD", "Liberian Dollar"),
                    new Country("Libya", "Libya", "LY", "LBY", "434", "LYD", "Libyan Dinar"),
                    new Country("Liechtenstein", "Liechtenstein", "LI", "LIE", "438", "CHF", "Swiss Franc"),
                    new Country("Lithuania", "Lithuania", "LT", "LTU", "440", "EUR", "Euro"),
                    new Country("Luxembourg", "Luxembourg", "LU", "LUX", "442", "EUR", "Euro"),
                    new Country("Macau", "Macao", "MO", "MAC", "446", "MOP", "Pataca"),
                    new Country("Macedonia", "Macedonia - the Former Yugoslav Republic of", "MK", "MKD", "807", "MKD", "Denar"),
                    new Country("Madagascar", "Madagascar", "MG", "MDG", "450", "MGA", "Malagasy Ariary"),
                    new Country("Malawi", "Malawi", "MW", "MWI", "454", "MWK", "Kwacha"),
                    new Country("Malaysia", "Malaysia", "MY", "MYS", "458", "MYR", "Malaysian Ringgit"),
                    new Country("Maldives", "Maldives", "MV", "MDV", "462", "MVR", "Rufiyaa"),
                    new Country("Mali", "Mali", "ML", "MLI", "466", "XOF", "CFA Franc BCEAO"),
                    new Country("Malta", "Malta", "MT", "MLT", "470", "EUR", "Euro"),
                    new Country("Marshall Islands", "Marshall Islands", "MH", "MHL", "584", "USD", "US Dollar"),
                    new Country("Martinique", "Martinique", "MQ", "MTQ", "474", "EUR", "Euro"),
                    new Country("Mauritania", "Mauritania", "MR", "MRT", "478", "MRO", "Ouguiya"),
                    new Country("Mauritius", "Mauritius", "MU", "MUS", "480", "MUR", "Mauritius Rupee"),
                    new Country("Mayotte", "Mayotte", "YT", "MYT", "175", "EUR", "Euro"),
                    new Country("Mexico", "Mexico", "MX", "MEX", "484", "MXN", "Mexican Peso"),
                    new Country("Micronesia", "Micronesia - Federated States of", "FM", "FSM", "583", "USD", "US Dollar"),
                    new Country("Moldova", "Moldova - Republic of", "MD", "MDA", "498", "MDL", "Moldovan Leu"),
                    new Country("Monaco", "Monaco", "MC", "MCO", "492", "EUR", "Euro"),
                    new Country("Mongolia", "Mongolia", "MN", "MNG", "496", "MNT", "Tugrik"),
                    new Country("Montenegro", "Montenegro", "ME", "MNE", "499", "EUR", "Euro"),
                    new Country("Montserrat", "Montserrat", "MS", "MSR", "500", "XCD", "East Caribbean Dollar"),
                    new Country("Morocco", "Morocco", "MA", "MAR", "504", "MAD", "Moroccan Dirham"),
                    new Country("Mozambique", "Mozambique", "MZ", "MOZ", "508", "MZN", "Mozambique Metical"),
                    new Country("Myanmar", "Myanmar", "MM", "MMR", "104", "MMK", "Kyat"),
                    new Country("Namibia", "Namibia", "NA", "NAM", "516", "ZAR", "Rand"),
                    new Country("Nauru", "Nauru", "NR", "NRU", "520", "AUD", "Australian Dollar"),
                    new Country("Nepal", "Nepal", "NP", "NPL", "524", "NPR", "Nepalese Rupee"),
                    new Country("Netherlands", "Netherlands", "NL", "NLD", "528", "EUR", "Euro"),
                    new Country("New Caledonia", "New Caledonia", "NC", "NCL", "540", "XPF", "CFP Franc"),
                    new Country("New Zealand", "New Zealand", "NZ", "NZL", "554", "NZD", "New Zealand Dollar"),
                    new Country("Nicaragua", "Nicaragua", "NI", "NIC", "558", "NIO", "Cordoba Oro"),
                    new Country("Niger", "Niger", "NE", "NER", "562", "XOF", "CFA Franc BCEAO"),
                    new Country("Nigeria", "Nigeria", "NG", "NGA", "566", "NGN", "Naira"),
                    new Country("Niue", "Niue", "NU", "NIU", "570", "NZD", "New Zealand Dollar"),
                    new Country("Norfolk Island", "Norfolk Island", "NF", "NFK", "574", "AUD", "Australian Dollar"),
                    new Country("Northern Mariana Islands", "Northern Mariana Islands", "MP", "MNP", "580", "USD", "US Dollar"),
                    new Country("Norway", "Norway", "NO", "NOR", "578", "NOK", "Norwegian Krone"),
                    new Country("Oman", "Oman", "OM", "OMN", "512", "OMR", "Rial Omani"),
                    new Country("Pakistan", "Pakistan", "PK", "PAK", "586", "PKR", "Pakistan Rupee"),
                    new Country("Palau", "Palau", "PW", "PLW", "585", "USD", "US Dollar"),
                    new Country("Palestine", "State of Palestine", " PS", "PSE", "275", "", "No universal currency"),
                    new Country("Panama", "Panama", "PA", "PAN", "591", "USD", "US Dollar"),
                    new Country("Papua New Guinea", "Papua New Guinea", "PG", "PNG", "598", "PGK", "Kina"),
                    new Country("Paraguay", "Paraguay", "PY", "PRY", "600", "PYG", "Guarani"),
                    new Country("Peru", "Peru", "PE", "PER", "604", "PEN", "Nuevo Sol"),
                    new Country("Philippines", "Philippines", "PH", "PHL", "608", "PHP", "Philippine Peso"),
                    new Country("Pitcairn Islands", "Pitcairn", "PN", "PCN", "612", "NZD", "New Zealand Dollar"),
                    new Country("Poland", "Poland", "PL", "POL", "616", "PLN", "Zloty"),
                    new Country("Portugal", "Portugal", "PT", "PRT", "620", "EUR", "Euro"),
                    new Country("Puerto Rico", "Puerto Rico", "PR", "PRI", "630", "USD", "US Dollar"),
                    new Country("Qatar", "Qatar", "QA", "QAT", "634", "QAR", "Qatari Rial"),
                    new Country("Romania", "Romania", "RO", "ROU", "642", "RON", "New Romanian Leu"),
                    new Country("Russia", "Russian Federation", "RU", "RUS", "643", "RUB", "Russian Ruble"),
                    new Country("Rwanda", "Rwanda", "RW", "RWA", "646", "RWF", "Rwanda Franc"),
                    new Country("Réunion", "Réunion", "RE", "REU", "638", "EUR", "Euro"),
                    new Country("St. Barthélemy", "Saint Barthélemy", "BL", "BLM", "652", "EUR", "Euro"),
                    new Country("St. Helena", "Ascension and Tristan da Cunha Saint Helena", "SH", "SHN", "654", "SHP", "Saint Helena Pound"),
                    new Country("St. Kitts & Nevis", "Saint Kitts and Nevis", "KN", "KNA", "659", "XCD", "East Caribbean Dollar"),
                    new Country("St. Lucia", "Saint Lucia", "LC", "LCA", "662", "XCD", "East Caribbean Dollar"),
                    new Country("St. Martin", "Saint Martin (French part)", "MF", "MAF", "663", "EUR", "Euro"),
                    new Country("St. Pierre & Miquelon", "Saint Pierre and Miquelon", "PM", "SPM", "666", "EUR", "Euro"),
                    new Country("St. Vincent & Grenadines", "Saint Vincent and the Grenadines", "VC", "VCT", "670", "XCD", "East Caribbean Dollar"),
                    new Country("Samoa", "Samoa", "WS", "WSM", "882", "WST", "Tala"),
                    new Country("San Marino", "San Marino", "SM", "SMR", "674", "EUR", "Euro"),
                    new Country("São Tomé & Príncipe", "Sao Tome and Principe", "ST", "STP", "678", "STD", "Dobra"),
                    new Country("Saudi Arabia", "Saudi Arabia", "SA", "SAU", "682", "SAR", "Saudi Riyal"),
                    new Country("Senegal", "Senegal", "SN", "SEN", "686", "XOF", "CFA Franc BCEAO"),
                    new Country("Serbia", "Serbia", "RS", "SRB", "688", "RSD", "Serbian Dinar"),
                    new Country("Seychelles", "Seychelles", "SC", "SYC", "690", "SCR", "Seychelles Rupee"),
                    new Country("Sierra Leone", "Sierra Leone", "SL", "SLE", "694", "SLL", "Leone"),
                    new Country("Singapore", "Singapore", "SG", "SGP", "702", "SGD", "Singapore Dollar"),
                    new Country("Sint Maarten", "Sint Maarten (Dutch part)", "SX", "SXM", "534", "ANG", "Netherlands Antillean Guilder"),
                    new Country("Slovakia", "Slovakia", "SK", "SVK", "703", "EUR", "Euro"),
                    new Country("Slovenia", "Slovenia", "SI", "SVN", "705", "EUR", "Euro"),
                    new Country("Solomon Islands", "Solomon Islands", "SB", "SLB", "90", "SBD", "Solomon Islands Dollar"),
                    new Country("Somalia", "Somalia", "SO", "SOM", "706", "SOS", "Somali Shilling"),
                    new Country("South Africa", "South Africa", "ZA", "ZAF", "710", "ZAR", "Rand"),
                    new Country("South Georgia & South Sandwich Islands", "South Georgia and the South Sandwich Islands", "GS", "SGS", "239", "", "No universal currency"),
                    new Country("South Sudan", "South Sudan", "SS", "SSD", "728", "SSP", "South Sudanese Pound"),
                    new Country("Spain", "Spain", "ES", "ESP", "724", "EUR", "Euro"),
                    new Country("Sri Lanka", "Sri Lanka", "LK", "LKA", "144", "LKR", "Sri Lanka Rupee"),
                    new Country("Sudan", "Sudan", "SD", "SDN", "729", "SDG", "Sudanese Pound"),
                    new Country("Suriname", "Suriname", "SR", "SUR", "740", "SRD", "Surinam Dollar"),
                    new Country("Svalbard & Jan Mayen", "Svalbard and Jan Mayen", "SJ", "SJM", "744", "NOK", "Norwegian Krone"),
                    new Country("Swaziland", "Swaziland", "SZ", "SWZ", "748", "SZL", "Lilangeni"),
                    new Country("Sweden", "Sweden", "SE", "SWE", "752", "SEK", "Swedish Krona"),
                    new Country("Switzerland", "Switzerland", "CH", "CHE", "756", "CHF", "Swiss Franc"),
                    new Country("Syria", "Syrian Arab Republic", "SY", "SYR", "760", "SYP", "Syrian Pound"),
                    new Country("Taiwan", "Taiwan", "TW", "TWN", "158", "TWD", "New Taiwan Dollar"),
                    new Country("Tajikistan", "Tajikistan", "TJ", "TJK", "762", "TJS", "Somoni"),
                    new Country("Tanzania", " United Republic of Tanzania", " TZ", "TZA", "834", "TZS", "Tanzanian Shilling"),
                    new Country("Thailand", "Thailand", "TH", "THA", "764", "THB", "Baht"),
                    new Country("Timor-Leste", "Timor-Leste", "TL", "TLS", "626", "USD", "US Dollar"),
                    new Country("Togo", "Togo", "TG", "TGO", "768", "XOF", "CFA Franc BCEAO"),
                    new Country("Tokelau", "Tokelau", "TK", "TKL", "772", "NZD", "New Zealand Dollar"),
                    new Country("Tonga", "Tonga", "TO", "TON", "776", "TOP", "Pa’anga"),
                    new Country("Trinidad & Tobago", "Trinidad and Tobago", "TT", "TTO", "780", "TTD", "Trinidad and Tobago Dollar"),
                    new Country("Tunisia", "Tunisia", "TN", "TUN", "788", "TND", "Tunisian Dinar"),
                    new Country("Turkey", "Turkey", "TR", "TUR", "792", "TRY", "Turkish Lira"),
                    new Country("Turkmenistan", "Turkmenistan", "TM", "TKM", "795", "TMT", "Turkmenistan New Manat"),
                    new Country("Turks & Caicos Islands", "Turks and Caicos Islands", "TC", "TCA", "796", "USD", "US Dollar"),
                    new Country("Tuvalu", "Tuvalu", "TV", "TUV", "798", "AUD", "Australian Dollar"),
                    new Country("Uganda", "Uganda", "UG", "UGA", "800", "UGX", "Uganda Shilling"),
                    new Country("Ukraine", "Ukraine", "UA", "UKR", "804", "UAH", "Hryvnia"),
                    new Country("United Arab Emirates", "United Arab Emirates", "AE", "ARE", "784", "AED", "UAE Dirham"),
                    new Country("United Kingdom", "United Kingdom", "GB", "GBR", "826", "GBP", "Pound Sterling"),
                    new Country("United States", "United States of America", "US", "USA", "840", "USD", "US Dollar"),
                    new Country("U.S. Outlying Islands", "United States Minor Outlying Islands", "UM", "UMI", "581", "USD", "US Dollar"),
                    new Country("Uruguay", "Uruguay", "UY", "URY", "858", "UYU", "Peso Uruguayo"),
                    new Country("Uzbekistan", "Uzbekistan", "UZ", "UZB", "860", "UZS", "Uzbekistan Sum"),
                    new Country("Vanuatu", "Vanuatu", "VU", "VUT", "548", "VUV", "Vatu"),
                    new Country("Venezuela", "Bolivarian Republic of Venezuela", "VE", "VEN", "862", "VEF", "Bolivar"),
                    new Country("Vietnam", "Viet Nam", "VN", "VNM", "704", "VND", "Dong"),
                    new Country("British Virgin Islands", "British Virgin Islands", "VG", "VGB", "92", "USD", "US Dollar"),
                    new Country("U.S. Virgin Islands", " U.S. Virgin Islands", " VI", "VIR", "850", "USD", "US Dollar"),
                    new Country("Wallis & Futuna", "Wallis and Futuna", "WF", "WLF", "876", "XPF", "CFP Franc"),
                    new Country("Western Sahara", "Western Sahara", "EH", "ESH", "732", "MAD", "Moroccan Dirham"),
                    new Country("Yemen", "Yemen", "YE", "YEM", "887", "YER", "Yemeni Rial"),
                    new Country("Zambia", "Zambia", "ZM", "ZMB", "894", "ZMW", "Zambian Kwacha"),
                    new Country("Zimbabwe", "Zimbabwe", "ZW", "ZWE", "716", "ZWL", "Zimbabwe Dollar"),
                    new Country("Åland Islands", "Åland Islands", "AX", "ALA", "248", "EUR", "Euro")
                };
                dictionary = dictionary.OrderBy(c => c.Name).ToList();
                return dictionary;
            }
        }

        public static List<SelectListItem> AsSelectListItems
        {
            get
            {
                var output = new List<SelectListItem>();
                output.Add(new SelectListItem() { Text = "--- Choose a country ---", Value = "" });
                output.AddRange(All.OrderBy(c => c.Name).Select(c => new SelectListItem() { Text = c.Name, Value = c.Name }));
                return output;
            }
        }

    }
}
