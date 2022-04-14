using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared.Helpers.Models
{
    public class CurrencyDto
    {
        static CurrencyDto()
        {
            var currencies = new List<CurrencyDto>
            {
                new CurrencyDto("AED", "United Arab Emirates dirham", 784, 2),
                new CurrencyDto("AFN", "Afghan afghani", 971, 2),
                new CurrencyDto("ALL", "Albanian lek", 8, 2),
                new CurrencyDto("AMD", "Armenian dram", 51, 2),
                new CurrencyDto("ANG", "Netherlands Antillean guilder", 532, 2),
                new CurrencyDto("AOA", "Angolan kwanza", 973, 2),
                new CurrencyDto("ARS", "Argentine peso", 32, 2),
                new CurrencyDto("AUD", "Australian dollar", 36, 2),
                new CurrencyDto("AWG", "Aruban florin", 533, 2),
                new CurrencyDto("AZN", "Azerbaijani manat", 944, 2),
                new CurrencyDto("BAM", "Bosnia and Herzegovina convertible mark", 977, 2),
                new CurrencyDto("BBD", "Barbados dollar", 52, 2),
                new CurrencyDto("BDT", "Bangladeshi taka", 50, 2),
                new CurrencyDto("BGN", "Bulgarian lev", 975, 2),
                new CurrencyDto("BHD", "Bahraini dinar", 48, 3),
                new CurrencyDto("BIF", "Burundian franc", 108, 0),
                new CurrencyDto("BMD", "Bermudian dollar", 60, 2),
                new CurrencyDto("BND", "Brunei dollar", 96, 2),
                new CurrencyDto("BOB", "Boliviano", 68, 2),
                new CurrencyDto("BOV", "Bolivian Mvdol (funds code)", 984, 2),
                new CurrencyDto("BRL", "Brazilian real", 986, 2),
                new CurrencyDto("BSD", "Bahamian dollar", 44, 2),
                new CurrencyDto("BTN", "Bhutanese ngultrum", 64, 2),
                new CurrencyDto("BWP", "Botswana pula", 72, 2),
                new CurrencyDto("BYN", "Belarusian ruble", 933, 2),
                new CurrencyDto("BZD", "Belize dollar", 84, 2),
                new CurrencyDto("CAD", "Canadian dollar", 124, 2),
                new CurrencyDto("CDF", "Congolese franc", 976, 2),
                new CurrencyDto("CHE", "WIR Euro (complementary currency)", 947, 2),
                new CurrencyDto("CHF", "Swiss franc", 756, 2),
                new CurrencyDto("CHW", "WIR Franc (complementary currency)", 948, 2),
                new CurrencyDto("CLF", "Unidad de Fomento (funds code)", 990, 4),
                new CurrencyDto("CLP", "Chilean peso", 152, 0),
                new CurrencyDto("CNY", "Chinese yuan", 156, 2),
                new CurrencyDto("COP", "Colombian peso", 170, 2),
                new CurrencyDto("COU", "Unidad de Valor Real (UVR) (funds code)[7]", 970, 2),
                new CurrencyDto("CRC", "Costa Rican colon", 188, 2),
                new CurrencyDto("CUC", "Cuban convertible peso", 931, 2),
                new CurrencyDto("CUP", "Cuban peso", 192, 2),
                new CurrencyDto("CVE", "Cape Verde escudo", 132, 0),
                new CurrencyDto("CZK", "Czech koruna", 203, 2),
                new CurrencyDto("DJF", "Djiboutian franc", 262, 0),
                new CurrencyDto("DKK", "Danish krone", 208, 2),
                new CurrencyDto("DOP", "Dominican peso", 214, 2),
                new CurrencyDto("DZD", "Algerian dinar", 12, 2),
                new CurrencyDto("EGP", "Egyptian pound", 818, 2),
                new CurrencyDto("ERN", "Eritrean nakfa", 232, 2),
                new CurrencyDto("ETB", "Ethiopian birr", 230, 2),
                new CurrencyDto("EUR", "Euro", 978, 2),
                new CurrencyDto("FJD", "Fiji dollar", 242, 2),
                new CurrencyDto("FKP", "Falkland Islands pound", 238, 2),
                new CurrencyDto("GBP", "Pound sterling", 826, 2),
                new CurrencyDto("GEL", "Georgian lari", 981, 2),
                new CurrencyDto("GHS", "Ghanaian cedi", 936, 2),
                new CurrencyDto("GIP", "Gibraltar pound", 292, 2),
                new CurrencyDto("GMD", "Gambian dalasi", 270, 2),
                new CurrencyDto("GNF", "Guinean franc", 324, 0),
                new CurrencyDto("GTQ", "Guatemalan quetzal", 320, 2),
                new CurrencyDto("GYD", "Guyanese dollar", 328, 2),
                new CurrencyDto("HKD", "Hong Kong dollar", 344, 2),
                new CurrencyDto("HNL", "Honduran lempira", 340, 2),
                new CurrencyDto("HRK", "Croatian kuna", 191, 2),
                new CurrencyDto("HTG", "Haitian gourde", 332, 2),
                new CurrencyDto("HUF", "Hungarian forint", 348, 2),
                new CurrencyDto("IDR", "Indonesian rupiah", 360, 2),
                new CurrencyDto("ILS", "Israeli new shekel", 376, 2),
                new CurrencyDto("INR", "Indian rupee", 356, 2),
                new CurrencyDto("IQD", "Iraqi dinar", 368, 3),
                new CurrencyDto("IRR", "Iranian rial", 364, 2),
                new CurrencyDto("ISK", "Icelandic króna", 352, 0),
                new CurrencyDto("JMD", "Jamaican dollar", 388, 2),
                new CurrencyDto("JOD", "Jordanian dinar", 400, 3),
                new CurrencyDto("JPY", "Japanese yen", 392, 0),
                new CurrencyDto("KES", "Kenyan shilling", 404, 2),
                new CurrencyDto("KGS", "Kyrgyzstani som", 417, 2),
                new CurrencyDto("KHR", "Cambodian riel", 116, 2),
                new CurrencyDto("KMF", "Comoro franc", 174, 0),
                new CurrencyDto("KPW", "North Korean won", 408, 2),
                new CurrencyDto("KRW", "South Korean won", 410, 0),
                new CurrencyDto("KWD", "Kuwaiti dinar", 414, 3),
                new CurrencyDto("KYD", "Cayman Islands dollar", 136, 2),
                new CurrencyDto("KZT", "Kazakhstani tenge", 398, 2),
                new CurrencyDto("LAK", "Lao kip", 418, 2),
                new CurrencyDto("LBP", "Lebanese pound", 422, 2),
                new CurrencyDto("LKR", "Sri Lankan rupee", 144, 2),
                new CurrencyDto("LRD", "Liberian dollar", 430, 2),
                new CurrencyDto("LSL", "Lesotho loti", 426, 2),
                new CurrencyDto("LYD", "Libyan dinar", 434, 3),
                new CurrencyDto("MAD", "Moroccan dirham", 504, 2),
                new CurrencyDto("MDL", "Moldovan leu", 498, 2),
                new CurrencyDto("MGA", "Malagasy ariary", 969, 1),
                new CurrencyDto("MKD", "Macedonian denar", 807, 2),
                new CurrencyDto("MMK", "Myanmar kyat", 104, 2),
                new CurrencyDto("MNT", "Mongolian tögrög", 496, 2),
                new CurrencyDto("MOP", "Macanese pataca", 446, 2),
                new CurrencyDto("MRO", "Mauritanian ouguiya", 478, 1),
                new CurrencyDto("MUR", "Mauritian rupee", 480, 2),
                new CurrencyDto("MVR", "Maldivian rufiyaa", 462, 2),
                new CurrencyDto("MWK", "Malawian kwacha", 454, 2),
                new CurrencyDto("MXN", "Mexican peso", 484, 2),
                new CurrencyDto("MXV", "Mexican Unidad de Inversion (UDI) (funds code)", 979, 2),
                new CurrencyDto("MYR", "Malaysian ringgit", 458, 2),
                new CurrencyDto("MZN", "Mozambican metical", 943, 2),
                new CurrencyDto("NAD", "Namibian dollar", 516, 2),
                new CurrencyDto("NGN", "Nigerian naira", 566, 2),
                new CurrencyDto("NIO", "Nicaraguan córdoba", 558, 2),
                new CurrencyDto("NOK", "Norwegian krone", 578, 2),
                new CurrencyDto("NPR", "Nepalese rupee", 524, 2),
                new CurrencyDto("NZD", "New Zealand dollar", 554, 2),
                new CurrencyDto("OMR", "Omani rial", 512, 3),
                new CurrencyDto("PAB", "Panamanian balboa", 590, 2),
                new CurrencyDto("PEN", "Peruvian Sol", 604, 2),
                new CurrencyDto("PGK", "Papua New Guinean kina", 598, 2),
                new CurrencyDto("PHP", "Philippine peso", 608, 2),
                new CurrencyDto("PKR", "Pakistani rupee", 586, 2),
                new CurrencyDto("PLN", "Polish złoty", 985, 2),
                new CurrencyDto("PYG", "Paraguayan guaraní", 600, 0),
                new CurrencyDto("QAR", "Qatari riyal", 634, 2),
                new CurrencyDto("RON", "Romanian leu", 946, 2),
                new CurrencyDto("RSD", "Serbian dinar", 941, 2),
                new CurrencyDto("RUB", "Russian ruble", 643, 2),
                new CurrencyDto("RWF", "Rwandan franc", 646, 0),
                new CurrencyDto("SAR", "Saudi riyal", 682, 2),
                new CurrencyDto("SBD", "Solomon Islands dollar", 90, 2),
                new CurrencyDto("SCR", "Seychelles rupee", 690, 2),
                new CurrencyDto("SDG", "Sudanese pound", 938, 2),
                new CurrencyDto("SEK", "Swedish krona/kronor", 752, 2),
                new CurrencyDto("SGD", "Singapore dollar", 702, 2),
                new CurrencyDto("SHP", "Saint Helena pound", 654, 2),
                new CurrencyDto("SLL", "Sierra Leonean leone", 694, 2),
                new CurrencyDto("SOS", "Somali shilling", 706, 2),
                new CurrencyDto("SRD", "Surinamese dollar", 968, 2),
                new CurrencyDto("SSP", "South Sudanese pound", 728, 2),
                new CurrencyDto("STD", "São Tomé and Príncipe dobra", 678, 2),
                new CurrencyDto("SVC", "Salvadoran colón", 222, 2),
                new CurrencyDto("SYP", "Syrian pound", 760, 2),
                new CurrencyDto("SZL", "Swazi lilangeni", 748, 2),
                new CurrencyDto("THB", "Thai baht", 764, 2),
                new CurrencyDto("TJS", "Tajikistani somoni", 972, 2),
                new CurrencyDto("TMT", "Turkmenistani manat", 934, 2),
                new CurrencyDto("TND", "Tunisian dinar", 788, 3),
                new CurrencyDto("TOP", "Tongan paʻanga", 776, 2),
                new CurrencyDto("TRY", "Turkish lira", 949, 2),
                new CurrencyDto("TTD", "Trinidad and Tobago dollar", 780, 2),
                new CurrencyDto("TWD", "New Taiwan dollar", 901, 2),
                new CurrencyDto("TZS", "Tanzanian shilling", 834, 2),
                new CurrencyDto("UAH", "Ukrainian hryvnia", 980, 2),
                new CurrencyDto("UGX", "Ugandan shilling", 800, 0),
                new CurrencyDto("USD", "United States dollar", 840, 2),
                new CurrencyDto("USN", "United States dollar (next day) (funds code)", 997, 2),
                new CurrencyDto("UYI", "Uruguay Peso en Unidades Indexadas (URUIURUI) (funds code)", 940, 0),
                new CurrencyDto("UYU", "Uruguayan peso", 858, 2),
                new CurrencyDto("UZS", "Uzbekistan som", 860, 2),
                new CurrencyDto("VEF", "Venezuelan bolívar", 937, 2),
                new CurrencyDto("VND", "Vietnamese đồng", 704, 0),
                new CurrencyDto("VUV", "Vanuatu vatu", 548, 0),
                new CurrencyDto("WST", "Samoan tala", 882, 2),
                new CurrencyDto("XAF", "CFA franc BEAC", 950, 0),
                new CurrencyDto("XAG", "Silver (one troy ounce)", 961, 0),
                new CurrencyDto("XAU", "Gold (one troy ounce)", 959, 0),
                new CurrencyDto("XBA", "European Composite Unit (EURCO) (bond market unit)", 955, 0),
                new CurrencyDto("XBB", "European Monetary Unit (E.M.U.-6) (bond market unit)", 956, 0),
                new CurrencyDto("XBC", "European Unit of Account 9 (E.U.A.-9) (bond market unit)", 957, 0),
                new CurrencyDto("XBD", "European Unit of Account 17 (E.U.A.-17) (bond market unit)", 958, 0),
                new CurrencyDto("XCD", "East Caribbean dollar", 951, 2),
                new CurrencyDto("XDR", "Special drawing rights", 960, 0),
                new CurrencyDto("XOF", "CFA franc BCEAO", 952, 0),
                new CurrencyDto("XPD", "Palladium (one troy ounce)", 964, 0),
                new CurrencyDto("XPF", "CFP franc (franc Pacifique)", 953, 0),
                new CurrencyDto("XPT", "Platinum (one troy ounce)", 962, 0),
                new CurrencyDto("XSU", "SUCRE", 994, 0),
                new CurrencyDto("XTS", "Code reserved for testing purposes", 963, 0),
                new CurrencyDto("XUA", "ADB Unit of Account", 965, 0),
                new CurrencyDto("XXX", "No currency ", 999, 0),
                new CurrencyDto("YER", "Yemeni rial", 886, 2),
                new CurrencyDto("ZAR", "South African rand", 710, 2),
                new CurrencyDto("ZMW", "Zambian kwacha", 967, 2),
                new CurrencyDto("ZWL", "Zimbabwean dollar A/10", 932, 2),
            };

            IsoCurrencyCodes = currencies.ToDictionary(d => d.Code);
        }

        public CurrencyDto()
        {
        }

        public CurrencyDto(string isoCode, string name, int number, int exponent)
        {
            this.Code = isoCode;
            this.IsoCode = isoCode;
            this.Name = name;
            this.Number = number;
            this.Exponent = exponent;
        }

        public string IsoCode { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }

        public int Exponent { get; set; }

        public static CurrencyDto GetCurrencyDefinition(string isoCurrencyCode)
        {
            if (IsoCurrencyCodes.TryGetValue(isoCurrencyCode, out var currencyDto))
            {
                return currencyDto;
            }

            return null;
        }

        public static int? GetCurrencyISONumber(CurrencyEnum currency)
        {
            return GetCurrencyDefinition(currency.ToString())?.Number;
        }

        private static readonly IReadOnlyDictionary<string, CurrencyDto> IsoCurrencyCodes;
    }
}
