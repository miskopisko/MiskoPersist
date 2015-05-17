using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace MiskoPersist.MoneyType
{
    [Serializable]
    public struct Currency : IEquatable<Currency>, IFormatProvider
    {
        #region Static Currency Fields

        public static readonly Currency None = new Currency(0);

        public static readonly Currency All;

        public static readonly Currency Dzd;

        public static readonly Currency Ars;

        public static readonly Currency Aud;

        public static readonly Currency Bsd;

        public static readonly Currency Bhd;

        public static readonly Currency Bdt;

        public static readonly Currency Amd;

        public static readonly Currency Bbd;

        public static readonly Currency Bmd;

        public static readonly Currency Bob;

        public static readonly Currency Bwp;

        public static readonly Currency Bzd;

        public static readonly Currency Sbd;

        public static readonly Currency Bnd;

        public static readonly Currency Mmk;

        public static readonly Currency Bif;

        public static readonly Currency Khr;

        public static readonly Currency Cad;

        public static readonly Currency Cve;

        public static readonly Currency Kyd;

        public static readonly Currency Lkr;

        public static readonly Currency Clp;

        public static readonly Currency Cny;

        public static readonly Currency Cop;

        public static readonly Currency Kmf;

        public static readonly Currency Crc;

        public static readonly Currency Hrk;

        public static readonly Currency Cup;

        public static readonly Currency Czk;

        public static readonly Currency Dkk;

        public static readonly Currency Dop;

        public static readonly Currency Svc;

        public static readonly Currency Etb;

        public static readonly Currency Ern;

        public static readonly Currency Eek;

        public static readonly Currency Fkp;

        public static readonly Currency Fjd;

        public static readonly Currency Djf;

        public static readonly Currency Gmd;

        public static readonly Currency Gip;

        public static readonly Currency Gtq;

        public static readonly Currency Gnf;

        public static readonly Currency Gyd;

        public static readonly Currency Htg;

        public static readonly Currency Hnl;

        public static readonly Currency Hkd;

        public static readonly Currency Huf;

        public static readonly Currency Isk;

        public static readonly Currency Inr;

        public static readonly Currency Idr;

        public static readonly Currency Irr;

        public static readonly Currency Iqd;

        public static readonly Currency Ils;

        public static readonly Currency Jmd;

        public static readonly Currency Jpy;

        public static readonly Currency Kzt;

        public static readonly Currency Jod;

        public static readonly Currency Kes;

        public static readonly Currency Kpw;

        public static readonly Currency Krw;

        public static readonly Currency Kwd;

        public static readonly Currency Kgs;

        public static readonly Currency Lak;

        public static readonly Currency Lbp;

        public static readonly Currency Lvl;

        public static readonly Currency Lrd;

        public static readonly Currency Lyd;

        public static readonly Currency Ltl;

        public static readonly Currency Mop;

        public static readonly Currency Mwk;

        public static readonly Currency Myr;

        public static readonly Currency Mvr;

        public static readonly Currency Mro;

        public static readonly Currency Mur;

        public static readonly Currency Mxn;

        public static readonly Currency Mnt;

        public static readonly Currency Mdl;

        public static readonly Currency Mad;

        public static readonly Currency Omr;

        public static readonly Currency Npr;

        public static readonly Currency Ang;

        public static readonly Currency Awg;

        public static readonly Currency Vuv;

        public static readonly Currency Nzd;

        public static readonly Currency Nio;

        public static readonly Currency Ngn;

        public static readonly Currency Nok;

        public static readonly Currency Pkr;

        public static readonly Currency Pab;

        public static readonly Currency Pgk;

        public static readonly Currency Pyg;

        public static readonly Currency Pen;

        public static readonly Currency Php;

        public static readonly Currency Gwp;

        public static readonly Currency Qar;

        public static readonly Currency Rub;

        public static readonly Currency Rwf;

        public static readonly Currency Shp;

        public static readonly Currency Std;

        public static readonly Currency Sar;

        public static readonly Currency Scr;

        public static readonly Currency Sll;

        public static readonly Currency Sgd;

        public static readonly Currency Skk;

        public static readonly Currency Vnd;

        public static readonly Currency Sos;

        public static readonly Currency Zar;

        public static readonly Currency Zwd;

        public static readonly Currency Szl;

        public static readonly Currency Sek;

        public static readonly Currency Chf;

        public static readonly Currency Syp;

        public static readonly Currency Thb;

        public static readonly Currency Top;

        public static readonly Currency Ttd;

        public static readonly Currency Aed;

        public static readonly Currency Tnd;

        public static readonly Currency Tmm;

        public static readonly Currency Ugx;

        public static readonly Currency Mkd;

        public static readonly Currency Egp;

        public static readonly Currency Gbp;

        public static readonly Currency Tzs;

        public static readonly Currency Usd;

        public static readonly Currency Uyu;

        public static readonly Currency Uzs;

        public static readonly Currency Wst;

        public static readonly Currency Yer;

        public static readonly Currency Zmk;

        public static readonly Currency Twd;

        public static readonly Currency Ghs;

        public static readonly Currency Vef;

        public static readonly Currency Sdg;

        public static readonly Currency Rsd;

        public static readonly Currency Mzn;

        public static readonly Currency Azn;

        public static readonly Currency Ron;

        public static readonly Currency Try;

        public static readonly Currency Xaf;

        public static readonly Currency Xcd;

        public static readonly Currency Xof;

        public static readonly Currency Xpf;

        public static readonly Currency Xba;

        public static readonly Currency Xbb;

        public static readonly Currency Xbc;

        public static readonly Currency Xbd;

        public static readonly Currency Xau;

        public static readonly Currency Xdr;

        public static readonly Currency Xag;

        public static readonly Currency Xpt;

        public static readonly Currency Xts;

        public static readonly Currency Xpd;

        public static readonly Currency Srd;

        public static readonly Currency Mga;

        public static readonly Currency Afn;

        public static readonly Currency Tjs;

        public static readonly Currency Aoa;

        public static readonly Currency Byr;

        public static readonly Currency Bgn;

        public static readonly Currency Cdf;

        public static readonly Currency Bam;

        public static readonly Currency Eur;

        public static readonly Currency Uah;

        public static readonly Currency Gel;

        public static readonly Currency Pln;

        public static readonly Currency Brl;

        public static readonly Currency Xxx;

        #endregion

        private static readonly Dictionary<Int32, CurrencyTableEntry> _currencies
            = new Dictionary<Int32, CurrencyTableEntry>();

        private static readonly Dictionary<String, Int32> _codeIndex
            = new Dictionary<String, Int32>(StringComparer.InvariantCultureIgnoreCase);

        private static readonly Dictionary<String, List<Int32>> _symbolIndex
            = new Dictionary<String, List<Int32>>(StringComparer.InvariantCultureIgnoreCase);

        private static readonly Dictionary<Int32, Int32> _lcidToIsoCodeLookup
            = new Dictionary<Int32, Int32>();

        private static readonly Dictionary<Int32, List<Int32>> _isoCodeToLcidLookup
            = new Dictionary<Int32, List<Int32>>();

        private readonly Int32 _id;

        static Currency()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            var cultureIdLookup = new Dictionary<String, List<Int32>>();
            var symbolLookup = new Dictionary<String, String>();

            foreach (var culture in cultures)
            {
                var lcid = culture.LCID;
                var regionInfo = new RegionInfo(lcid);
                var isoSymbol = regionInfo.ISOCurrencySymbol;

                if (!cultureIdLookup.ContainsKey(isoSymbol))
                {
                    cultureIdLookup[isoSymbol] = new List<Int32>();
                }

                cultureIdLookup[isoSymbol].Add(lcid);
                symbolLookup[isoSymbol] = regionInfo.CurrencySymbol;
            }



            _currencies[008] = new CurrencyTableEntry("Lek", "ALL", 008, symbolLookup.GetValueOrDefault("ALL"));
            _currencies[012] = new CurrencyTableEntry("Algerian Dinar",
                                                      "DZD",
                                                      012,
                                                      symbolLookup.GetValueOrDefault("DZD"));
            _currencies[032] = new CurrencyTableEntry("Argentine Peso",
                                                      "ARS",
                                                      032,
                                                      symbolLookup.GetValueOrDefault("ARS"));
            _currencies[036] = new CurrencyTableEntry("Australian Dollar",
                                                      "AUD",
                                                      036,
                                                      symbolLookup.GetValueOrDefault("AUD"));
            _currencies[044] = new CurrencyTableEntry("Bahamian Dollar",
                                                      "BSD",
                                                      044,
                                                      symbolLookup.GetValueOrDefault("BSD"));
            _currencies[048] = new CurrencyTableEntry("Bahraini Dinar",
                                                      "BHD",
                                                      048,
                                                      symbolLookup.GetValueOrDefault("BHD"));
            _currencies[050] = new CurrencyTableEntry("Taka", "BDT", 050, symbolLookup.GetValueOrDefault("BDT"));
            _currencies[051] = new CurrencyTableEntry("Armenian Dram", "AMD", 051, symbolLookup.GetValueOrDefault("AMD"));
            _currencies[052] = new CurrencyTableEntry("Barbados Dollar",
                                                      "BBD",
                                                      052,
                                                      symbolLookup.GetValueOrDefault("BBD"));
            _currencies[060] = new CurrencyTableEntry("Bermudian Dollar (customarily known as Bermuda Dollar)",
                                                      "BMD",
                                                      060,
                                                      symbolLookup.GetValueOrDefault("BMD"));
            _currencies[068] = new CurrencyTableEntry("Boliviano", "BOB", 068, symbolLookup.GetValueOrDefault("BOB"));
            _currencies[072] = new CurrencyTableEntry("Pula", "BWP", 072, symbolLookup.GetValueOrDefault("BWP"));
            _currencies[084] = new CurrencyTableEntry("Belize Dollar", "BZD", 084, symbolLookup.GetValueOrDefault("BZD"));
            _currencies[090] = new CurrencyTableEntry("Solomon Islands Dollar",
                                                      "SBD",
                                                      090,
                                                      symbolLookup.GetValueOrDefault("SBD"));
            _currencies[096] = new CurrencyTableEntry("Brunei Dollar", "BND", 096, symbolLookup.GetValueOrDefault("BND"));
            _currencies[104] = new CurrencyTableEntry("Kyat", "MMK", 104, symbolLookup.GetValueOrDefault("MMK"));
            _currencies[108] = new CurrencyTableEntry("Burundi Franc", "BIF", 108, symbolLookup.GetValueOrDefault("BIF"));
            _currencies[116] = new CurrencyTableEntry("Riel", "KHR", 116, symbolLookup.GetValueOrDefault("KHR"));
            _currencies[124] = new CurrencyTableEntry("Canadian Dollar",
                                                      "CAD",
                                                      124,
                                                      symbolLookup.GetValueOrDefault("CAD"));
            _currencies[132] = new CurrencyTableEntry("Cape Verde Escudo",
                                                      "CVE",
                                                      132,
                                                      symbolLookup.GetValueOrDefault("CVE"));
            _currencies[136] = new CurrencyTableEntry("Cayman Islands Dollar",
                                                      "KYD",
                                                      136,
                                                      symbolLookup.GetValueOrDefault("KYD"));
            _currencies[144] = new CurrencyTableEntry("Sri Lanka Rupee",
                                                      "LKR",
                                                      144,
                                                      symbolLookup.GetValueOrDefault("LKR"));
            _currencies[152] = new CurrencyTableEntry("Chilean Peso", "CLP", 152, symbolLookup.GetValueOrDefault("CLP"));
            _currencies[156] = new CurrencyTableEntry("Yuan Renminbi", "CNY", 156, symbolLookup.GetValueOrDefault("CNY"));
            _currencies[170] = new CurrencyTableEntry("Colombian Peso",
                                                      "COP",
                                                      170,
                                                      symbolLookup.GetValueOrDefault("COP"));
            _currencies[174] = new CurrencyTableEntry("Comoro Franc", "KMF", 174, symbolLookup.GetValueOrDefault("KMF"));
            _currencies[188] = new CurrencyTableEntry("Costa Rican Colon",
                                                      "CRC",
                                                      188,
                                                      symbolLookup.GetValueOrDefault("CRC"));
            _currencies[191] = new CurrencyTableEntry("Croatian Kuna", "HRK", 191, symbolLookup.GetValueOrDefault("HRK"));
            _currencies[192] = new CurrencyTableEntry("Cuban Peso", "CUP", 192, symbolLookup.GetValueOrDefault("CUP"));
            _currencies[203] = new CurrencyTableEntry("Czech Koruna", "CZK", 203, symbolLookup.GetValueOrDefault("CZK"));
            _currencies[208] = new CurrencyTableEntry("Danish Krone", "DKK", 208, symbolLookup.GetValueOrDefault("DKK"));
            _currencies[214] = new CurrencyTableEntry("Dominican Peso",
                                                      "DOP",
                                                      214,
                                                      symbolLookup.GetValueOrDefault("DOP"));
            _currencies[222] = new CurrencyTableEntry("El Salvador Colon",
                                                      "SVC",
                                                      222,
                                                      symbolLookup.GetValueOrDefault("SVC"));
            _currencies[230] = new CurrencyTableEntry("Ethiopian Birr",
                                                      "ETB",
                                                      230,
                                                      symbolLookup.GetValueOrDefault("ETB"));
            _currencies[232] = new CurrencyTableEntry("Nakfa", "ERN", 232, symbolLookup.GetValueOrDefault("ERN"));
            _currencies[233] = new CurrencyTableEntry("Kroon", "EEK", 233, symbolLookup.GetValueOrDefault("EEK"));
            _currencies[238] = new CurrencyTableEntry("Falkland Islands Pound",
                                                      "FKP",
                                                      238,
                                                      symbolLookup.GetValueOrDefault("FKP"));
            _currencies[242] = new CurrencyTableEntry("Fiji Dollar", "FJD", 242, symbolLookup.GetValueOrDefault("FJD"));
            _currencies[262] = new CurrencyTableEntry("Djibouti Franc",
                                                      "DJF",
                                                      262,
                                                      symbolLookup.GetValueOrDefault("DJF"));
            _currencies[270] = new CurrencyTableEntry("Dalasi", "GMD", 270, symbolLookup.GetValueOrDefault("GMD"));
            _currencies[292] = new CurrencyTableEntry("Gibraltar Pound",
                                                      "GIP",
                                                      292,
                                                      symbolLookup.GetValueOrDefault("GIP"));
            _currencies[320] = new CurrencyTableEntry("Quetzal", "GTQ", 320, symbolLookup.GetValueOrDefault("GTQ"));
            _currencies[324] = new CurrencyTableEntry("Guinea Franc", "GNF", 324, symbolLookup.GetValueOrDefault("GNF"));
            _currencies[328] = new CurrencyTableEntry("Guyana Dollar", "GYD", 328, symbolLookup.GetValueOrDefault("GYD"));
            _currencies[332] = new CurrencyTableEntry("Gourde", "HTG", 332, symbolLookup.GetValueOrDefault("HTG"));
            _currencies[340] = new CurrencyTableEntry("Lempira", "HNL", 340, symbolLookup.GetValueOrDefault("HNL"));
            _currencies[344] = new CurrencyTableEntry("Hong Kong Dollar",
                                                      "HKD",
                                                      344,
                                                      symbolLookup.GetValueOrDefault("HKD"));
            _currencies[348] = new CurrencyTableEntry("Forint", "HUF", 348, symbolLookup.GetValueOrDefault("HUF"));
            _currencies[352] = new CurrencyTableEntry("Iceland Krona", "ISK", 352, symbolLookup.GetValueOrDefault("ISK"));
            _currencies[356] = new CurrencyTableEntry("Indian Rupee", "INR", 356, symbolLookup.GetValueOrDefault("INR"));
            _currencies[360] = new CurrencyTableEntry("Rupiah", "IDR", 360, symbolLookup.GetValueOrDefault("IDR"));
            _currencies[364] = new CurrencyTableEntry("Iranian Rial", "IRR", 364, symbolLookup.GetValueOrDefault("IRR"));
            _currencies[368] = new CurrencyTableEntry("Iraqi Dinar", "IQD", 368, symbolLookup.GetValueOrDefault("IQD"));
            _currencies[376] = new CurrencyTableEntry("New Israeli Sheqel",
                                                      "ILS",
                                                      376,
                                                      symbolLookup.GetValueOrDefault("ILS"));
            _currencies[388] = new CurrencyTableEntry("Jamaican Dollar",
                                                      "JMD",
                                                      388,
                                                      symbolLookup.GetValueOrDefault("JMD"));
            _currencies[392] = new CurrencyTableEntry("Yen", "JPY", 392, symbolLookup.GetValueOrDefault("JPY"));
            _currencies[398] = new CurrencyTableEntry("Tenge", "KZT", 398, symbolLookup.GetValueOrDefault("KZT"));
            _currencies[400] = new CurrencyTableEntry("Jordanian Dinar",
                                                      "JOD",
                                                      400,
                                                      symbolLookup.GetValueOrDefault("JOD"));
            _currencies[404] = new CurrencyTableEntry("Kenyan Shilling",
                                                      "KES",
                                                      404,
                                                      symbolLookup.GetValueOrDefault("KES"));
            _currencies[408] = new CurrencyTableEntry("North Korean Won",
                                                      "KPW",
                                                      408,
                                                      symbolLookup.GetValueOrDefault("KPW"));
            _currencies[410] = new CurrencyTableEntry("Won", "KRW", 410, symbolLookup.GetValueOrDefault("KRW"));
            _currencies[414] = new CurrencyTableEntry("Kuwaiti Dinar", "KWD", 414, symbolLookup.GetValueOrDefault("KWD"));
            _currencies[417] = new CurrencyTableEntry("Som", "KGS", 417, symbolLookup.GetValueOrDefault("KGS"));
            _currencies[418] = new CurrencyTableEntry("Kip", "LAK", 418, symbolLookup.GetValueOrDefault("LAK"));
            _currencies[422] = new CurrencyTableEntry("Lebanese Pound",
                                                      "LBP",
                                                      422,
                                                      symbolLookup.GetValueOrDefault("LBP"));
            _currencies[428] = new CurrencyTableEntry("Latvian Lats", "LVL", 428, symbolLookup.GetValueOrDefault("LVL"));
            _currencies[430] = new CurrencyTableEntry("Liberian Dollar",
                                                      "LRD",
                                                      430,
                                                      symbolLookup.GetValueOrDefault("LRD"));
            _currencies[434] = new CurrencyTableEntry("Libyan Dinar", "LYD", 434, symbolLookup.GetValueOrDefault("LYD"));
            _currencies[440] = new CurrencyTableEntry("Lithuanian Litas",
                                                      "LTL",
                                                      440,
                                                      symbolLookup.GetValueOrDefault("LTL"));
            _currencies[446] = new CurrencyTableEntry("Pataca", "MOP", 446, symbolLookup.GetValueOrDefault("MOP"));
            _currencies[454] = new CurrencyTableEntry("Kwacha", "MWK", 454, symbolLookup.GetValueOrDefault("MWK"));
            _currencies[458] = new CurrencyTableEntry("Malaysian Ringgit",
                                                      "MYR",
                                                      458,
                                                      symbolLookup.GetValueOrDefault("MYR"));
            _currencies[462] = new CurrencyTableEntry("Rufiyaa", "MVR", 462, symbolLookup.GetValueOrDefault("MVR"));
            _currencies[478] = new CurrencyTableEntry("Ouguiya", "MRO", 478, symbolLookup.GetValueOrDefault("MRO"));
            _currencies[480] = new CurrencyTableEntry("Mauritius Rupee",
                                                      "MUR",
                                                      480,
                                                      symbolLookup.GetValueOrDefault("MUR"));
            _currencies[484] = new CurrencyTableEntry("Mexican Peso", "MXN", 484, symbolLookup.GetValueOrDefault("MXN"));
            _currencies[496] = new CurrencyTableEntry("Tugrik", "MNT", 496, symbolLookup.GetValueOrDefault("MNT"));
            _currencies[498] = new CurrencyTableEntry("Moldovan Leu", "MDL", 498, symbolLookup.GetValueOrDefault("MDL"));
            _currencies[504] = new CurrencyTableEntry("Moroccan Dirham",
                                                      "MAD",
                                                      504,
                                                      symbolLookup.GetValueOrDefault("MAD"));
            _currencies[512] = new CurrencyTableEntry("Rial Omani", "OMR", 512, symbolLookup.GetValueOrDefault("OMR"));
            _currencies[524] = new CurrencyTableEntry("Nepalese Rupee",
                                                      "NPR",
                                                      524,
                                                      symbolLookup.GetValueOrDefault("NPR"));
            _currencies[532] = new CurrencyTableEntry("Netherlands Antillian Guilder",
                                                      "ANG",
                                                      532,
                                                      symbolLookup.GetValueOrDefault("ANG"));
            _currencies[533] = new CurrencyTableEntry("Aruban Guilder",
                                                      "AWG",
                                                      533,
                                                      symbolLookup.GetValueOrDefault("AWG"));
            _currencies[548] = new CurrencyTableEntry("Vatu", "VUV", 548, symbolLookup.GetValueOrDefault("VUV"));
            _currencies[554] = new CurrencyTableEntry("New Zealand Dollar",
                                                      "NZD",
                                                      554,
                                                      symbolLookup.GetValueOrDefault("NZD"));
            _currencies[558] = new CurrencyTableEntry("Cordoba Oro", "NIO", 558, symbolLookup.GetValueOrDefault("NIO"));
            _currencies[566] = new CurrencyTableEntry("Naira", "NGN", 566, symbolLookup.GetValueOrDefault("NGN"));
            _currencies[578] = new CurrencyTableEntry("Norwegian Krone",
                                                      "NOK",
                                                      578,
                                                      symbolLookup.GetValueOrDefault("NOK"));
            _currencies[586] = new CurrencyTableEntry("Pakistan Rupee",
                                                      "PKR",
                                                      586,
                                                      symbolLookup.GetValueOrDefault("PKR"));
            _currencies[590] = new CurrencyTableEntry("Balboa", "PAB", 590, symbolLookup.GetValueOrDefault("PAB"));
            _currencies[598] = new CurrencyTableEntry("Kina", "PGK", 598, symbolLookup.GetValueOrDefault("PGK"));
            _currencies[600] = new CurrencyTableEntry("Guarani", "PYG", 600, symbolLookup.GetValueOrDefault("PYG"));
            _currencies[604] = new CurrencyTableEntry("Nuevo Sol", "PEN", 604, symbolLookup.GetValueOrDefault("PEN"));
            _currencies[608] = new CurrencyTableEntry("Philippine Peso",
                                                      "PHP",
                                                      608,
                                                      symbolLookup.GetValueOrDefault("PHP"));
            _currencies[624] = new CurrencyTableEntry("Guinea-Bissau Peso",
                                                      "GWP",
                                                      624,
                                                      symbolLookup.GetValueOrDefault("GWP"));
            _currencies[634] = new CurrencyTableEntry("Qatari Rial", "QAR", 634, symbolLookup.GetValueOrDefault("QAR"));
            _currencies[643] = new CurrencyTableEntry("Russian Ruble", "RUB", 643, symbolLookup.GetValueOrDefault("RUB"));
            _currencies[646] = new CurrencyTableEntry("Rwanda Franc", "RWF", 646, symbolLookup.GetValueOrDefault("RWF"));
            _currencies[654] = new CurrencyTableEntry("Saint Helena Pound",
                                                      "SHP",
                                                      654,
                                                      symbolLookup.GetValueOrDefault("SHP"));
            _currencies[678] = new CurrencyTableEntry("Dobra", "STD", 678, symbolLookup.GetValueOrDefault("STD"));
            _currencies[682] = new CurrencyTableEntry("Saudi Riyal", "SAR", 682, symbolLookup.GetValueOrDefault("SAR"));
            _currencies[690] = new CurrencyTableEntry("Seychelles Rupee",
                                                      "SCR",
                                                      690,
                                                      symbolLookup.GetValueOrDefault("SCR"));
            _currencies[694] = new CurrencyTableEntry("Leone", "SLL", 694, symbolLookup.GetValueOrDefault("SLL"));
            _currencies[702] = new CurrencyTableEntry("Singapore Dollar",
                                                      "SGD",
                                                      702,
                                                      symbolLookup.GetValueOrDefault("SGD"));
            _currencies[703] = new CurrencyTableEntry("Slovak Koruna", "SKK", 703, symbolLookup.GetValueOrDefault("SKK"));
            _currencies[704] = new CurrencyTableEntry("Dong", "VND", 704, symbolLookup.GetValueOrDefault("VND"));
            _currencies[706] = new CurrencyTableEntry("Somali Shilling",
                                                      "SOS",
                                                      706,
                                                      symbolLookup.GetValueOrDefault("SOS"));
            _currencies[710] = new CurrencyTableEntry("Rand", "ZAR", 710, symbolLookup.GetValueOrDefault("ZAR"));
            _currencies[716] = new CurrencyTableEntry("Zimbabwe Dollar",
                                                      "ZWD",
                                                      716,
                                                      symbolLookup.GetValueOrDefault("ZWD"));
            _currencies[748] = new CurrencyTableEntry("Lilangeni", "SZL", 748, symbolLookup.GetValueOrDefault("SZL"));
            _currencies[752] = new CurrencyTableEntry("Swedish Krona", "SEK", 752, symbolLookup.GetValueOrDefault("SEK"));
            _currencies[756] = new CurrencyTableEntry("Swiss Franc", "CHF", 756, symbolLookup.GetValueOrDefault("CHF"));
            _currencies[760] = new CurrencyTableEntry("Syrian Pound", "SYP", 760, symbolLookup.GetValueOrDefault("SYP"));
            _currencies[764] = new CurrencyTableEntry("Baht", "THB", 764, symbolLookup.GetValueOrDefault("THB"));
            _currencies[776] = new CurrencyTableEntry("Pa'anga", "TOP", 776, symbolLookup.GetValueOrDefault("TOP"));
            _currencies[780] = new CurrencyTableEntry("Trinidad and Tobago Dollar",
                                                      "TTD",
                                                      780,
                                                      symbolLookup.GetValueOrDefault("TTD"));
            _currencies[784] = new CurrencyTableEntry("UAE Dirham", "AED", 784, symbolLookup.GetValueOrDefault("AED"));
            _currencies[788] = new CurrencyTableEntry("Tunisian Dinar",
                                                      "TND",
                                                      788,
                                                      symbolLookup.GetValueOrDefault("TND"));
            _currencies[795] = new CurrencyTableEntry("Manat", "TMM", 795, symbolLookup.GetValueOrDefault("TMM"));
            _currencies[800] = new CurrencyTableEntry("Uganda Shilling",
                                                      "UGX",
                                                      800,
                                                      symbolLookup.GetValueOrDefault("UGX"));
            _currencies[807] = new CurrencyTableEntry("Denar", "MKD", 807, symbolLookup.GetValueOrDefault("MKD"));
            _currencies[818] = new CurrencyTableEntry("Egyptian Pound",
                                                      "EGP",
                                                      818,
                                                      symbolLookup.GetValueOrDefault("EGP"));
            _currencies[826] = new CurrencyTableEntry("Pound Sterling",
                                                      "GBP",
                                                      826,
                                                      symbolLookup.GetValueOrDefault("GBP"));
            _currencies[834] = new CurrencyTableEntry("Tanzanian Shilling",
                                                      "TZS",
                                                      834,
                                                      symbolLookup.GetValueOrDefault("TZS"));
            _currencies[840] = new CurrencyTableEntry("US Dollar", "USD", 840, symbolLookup.GetValueOrDefault("USD"));
            _currencies[858] = new CurrencyTableEntry("Peso Uruguayo", "UYU", 858, symbolLookup.GetValueOrDefault("UYU"));
            _currencies[860] = new CurrencyTableEntry("Uzbekistan Sum",
                                                      "UZS",
                                                      860,
                                                      symbolLookup.GetValueOrDefault("UZS"));
            _currencies[882] = new CurrencyTableEntry("Tala", "WST", 882, symbolLookup.GetValueOrDefault("WST"));
            _currencies[886] = new CurrencyTableEntry("Yemeni Rial", "YER", 886, symbolLookup.GetValueOrDefault("YER"));
            _currencies[894] = new CurrencyTableEntry("Kwacha", "ZMK", 894, symbolLookup.GetValueOrDefault("ZMK"));
            _currencies[901] = new CurrencyTableEntry("New Taiwan Dollar",
                                                      "TWD",
                                                      901,
                                                      symbolLookup.GetValueOrDefault("TWD"));
            _currencies[936] = new CurrencyTableEntry("Ghana Cedi", "GHS", 936, symbolLookup.GetValueOrDefault("GHS"));
            _currencies[937] = new CurrencyTableEntry("Bolivar Fuerte",
                                                      "VEF",
                                                      937,
                                                      symbolLookup.GetValueOrDefault("VEF"));
            _currencies[938] = new CurrencyTableEntry("Sudanese Pound",
                                                      "SDG",
                                                      938,
                                                      symbolLookup.GetValueOrDefault("SDG"));
            _currencies[941] = new CurrencyTableEntry("Serbian Dinar", "RSD", 941, symbolLookup.GetValueOrDefault("RSD"));
            _currencies[943] = new CurrencyTableEntry("Metical", "MZN", 943, symbolLookup.GetValueOrDefault("MZN"));
            _currencies[944] = new CurrencyTableEntry("Azerbaijanian Manat",
                                                      "AZN",
                                                      944,
                                                      symbolLookup.GetValueOrDefault("AZN"));
            _currencies[946] = new CurrencyTableEntry("New Leu", "RON", 946, symbolLookup.GetValueOrDefault("RON"));
            _currencies[949] = new CurrencyTableEntry("New Turkish Lira",
                                                      "TRY",
                                                      949,
                                                      symbolLookup.GetValueOrDefault("TRY"));
            _currencies[950] = new CurrencyTableEntry("CFA Franc BEAC",
                                                      "XAF",
                                                      950,
                                                      symbolLookup.GetValueOrDefault("XAF"));
            _currencies[951] = new CurrencyTableEntry("East Caribbean Dollar",
                                                      "XCD",
                                                      951,
                                                      symbolLookup.GetValueOrDefault("XCD"));
            _currencies[952] = new CurrencyTableEntry("CFA Franc BCEAO",
                                                      "XOF",
                                                      952,
                                                      symbolLookup.GetValueOrDefault("XOF"));
            _currencies[953] = new CurrencyTableEntry("CFP Franc", "XPF", 953, symbolLookup.GetValueOrDefault("XPF"));
            _currencies[955] = new CurrencyTableEntry("Bond Markets Units European Composite Unit (EURCO)",
                                                      "XBA",
                                                      955,
                                                      symbolLookup.GetValueOrDefault("XBA"));
            _currencies[956] = new CurrencyTableEntry("European Monetary Unit (E.M.U.-6)",
                                                      "XBB",
                                                      956,
                                                      symbolLookup.GetValueOrDefault("XBB"));
            _currencies[957] = new CurrencyTableEntry("European Unit of Account 9(E.U.A.-9)",
                                                      "XBC",
                                                      957,
                                                      symbolLookup.GetValueOrDefault("XBC"));
            _currencies[958] = new CurrencyTableEntry("European Unit of Account 17(E.U.A.-17)",
                                                      "XBD",
                                                      958,
                                                      symbolLookup.GetValueOrDefault("XBD"));
            _currencies[959] = new CurrencyTableEntry("Gold", "XAU", 959, symbolLookup.GetValueOrDefault("XAU"));
            _currencies[960] = new CurrencyTableEntry("SDR", "XDR", 960, symbolLookup.GetValueOrDefault("XDR"));
            _currencies[961] = new CurrencyTableEntry("Silver", "XAG", 961, symbolLookup.GetValueOrDefault("XAG"));
            _currencies[962] = new CurrencyTableEntry("Platinum", "XPT", 962, symbolLookup.GetValueOrDefault("XPT"));
            _currencies[963] = new CurrencyTableEntry("Codes specifically reserved for testing purposes",
                                                      "XTS",
                                                      963,
                                                      symbolLookup.GetValueOrDefault("XTS"));
            _currencies[964] = new CurrencyTableEntry("Palladium", "XPD", 964, symbolLookup.GetValueOrDefault("XPD"));
            _currencies[968] = new CurrencyTableEntry("Surinam Dollar",
                                                      "SRD",
                                                      968,
                                                      symbolLookup.GetValueOrDefault("SRD"));
            _currencies[969] = new CurrencyTableEntry("Malagasy Ariary",
                                                      "MGA",
                                                      969,
                                                      symbolLookup.GetValueOrDefault("MGA"));
            _currencies[971] = new CurrencyTableEntry("Afghani", "AFN", 971, symbolLookup.GetValueOrDefault("AFN"));
            _currencies[972] = new CurrencyTableEntry("Somoni", "TJS", 972, symbolLookup.GetValueOrDefault("TJS"));
            _currencies[973] = new CurrencyTableEntry("Kwanza", "AOA", 973, symbolLookup.GetValueOrDefault("AOA"));
            _currencies[974] = new CurrencyTableEntry("Belarussian Ruble",
                                                      "BYR",
                                                      974,
                                                      symbolLookup.GetValueOrDefault("BYR"));
            _currencies[975] = new CurrencyTableEntry("Bulgarian Lev", "BGN", 975, symbolLookup.GetValueOrDefault("BGN"));
            _currencies[976] = new CurrencyTableEntry("Franc Congolais",
                                                      "CDF",
                                                      976,
                                                      symbolLookup.GetValueOrDefault("CDF"));
            _currencies[977] = new CurrencyTableEntry("Convertible Marks",
                                                      "BAM",
                                                      977,
                                                      symbolLookup.GetValueOrDefault("BAM"));
            _currencies[978] = new CurrencyTableEntry("Euro", "EUR", 978, symbolLookup.GetValueOrDefault("EUR"));
            _currencies[980] = new CurrencyTableEntry("Hryvnia", "UAH", 980, symbolLookup.GetValueOrDefault("UAH"));
            _currencies[981] = new CurrencyTableEntry("Lari", "GEL", 981, symbolLookup.GetValueOrDefault("GEL"));
            _currencies[985] = new CurrencyTableEntry("Zloty", "PLN", 985, symbolLookup.GetValueOrDefault("PLN"));
            _currencies[986] = new CurrencyTableEntry("Brazilian Real",
                                                      "BRL",
                                                      986,
                                                      symbolLookup.GetValueOrDefault("BRL"));
            _currencies[999] =
                new CurrencyTableEntry("The codes assigned for transactions where no currency is involved are:",
                                       "XXX",
                                       999,
                                       symbolLookup.GetValueOrDefault("XXX"));



            foreach (var currency in _currencies.Values)
            {
                var iso3LetterCode = currency.Iso3LetterCode;
                var symbol = currency.Symbol;
                List<Int32> lcids;

                if (cultureIdLookup.TryGetValue(iso3LetterCode, out lcids))
                {
                    foreach (var lcid in lcids)
                    {
                        _lcidToIsoCodeLookup[lcid] = currency.IsoNumberCode;
                    }

                    _isoCodeToLcidLookup[currency.IsoNumberCode] = lcids;
                }

                _codeIndex[iso3LetterCode] = currency.IsoNumberCode;

                if (symbol == null)
                {
                    continue;
                }

                List<Int32> idList;

                if (!_symbolIndex.TryGetValue(symbol, out idList))
                {
                    idList = new List<Int32>();
                    _symbolIndex[symbol] = idList;
                }

                idList.Add(currency.IsoNumberCode);
            }

            All = new Currency(008);
            Dzd = new Currency(012);
            Ars = new Currency(032);
            Aud = new Currency(036);
            Bsd = new Currency(044);
            Bhd = new Currency(048);
            Bdt = new Currency(050);
            Amd = new Currency(051);
            Bbd = new Currency(052);
            Bmd = new Currency(060);
            Bob = new Currency(068);
            Bwp = new Currency(072);
            Bzd = new Currency(084);
            Sbd = new Currency(090);
            Bnd = new Currency(096);
            Mmk = new Currency(104);
            Bif = new Currency(108);
            Khr = new Currency(116);
            Cad = new Currency(124);
            Cve = new Currency(132);
            Kyd = new Currency(136);
            Lkr = new Currency(144);
            Clp = new Currency(152);
            Cny = new Currency(156);
            Cop = new Currency(170);
            Kmf = new Currency(174);
            Crc = new Currency(188);
            Hrk = new Currency(191);
            Cup = new Currency(192);
            Czk = new Currency(203);
            Dkk = new Currency(208);
            Dop = new Currency(214);
            Svc = new Currency(222);
            Etb = new Currency(230);
            Ern = new Currency(232);
            Eek = new Currency(233);
            Fkp = new Currency(238);
            Fjd = new Currency(242);
            Djf = new Currency(262);
            Gmd = new Currency(270);
            Gip = new Currency(292);
            Gtq = new Currency(320);
            Gnf = new Currency(324);
            Gyd = new Currency(328);
            Htg = new Currency(332);
            Hnl = new Currency(340);
            Hkd = new Currency(344);
            Huf = new Currency(348);
            Isk = new Currency(352);
            Inr = new Currency(356);
            Idr = new Currency(360);
            Irr = new Currency(364);
            Iqd = new Currency(368);
            Ils = new Currency(376);
            Jmd = new Currency(388);
            Jpy = new Currency(392);
            Kzt = new Currency(398);
            Jod = new Currency(400);
            Kes = new Currency(404);
            Kpw = new Currency(408);
            Krw = new Currency(410);
            Kwd = new Currency(414);
            Kgs = new Currency(417);
            Lak = new Currency(418);
            Lbp = new Currency(422);
            Lvl = new Currency(428);
            Lrd = new Currency(430);
            Lyd = new Currency(434);
            Ltl = new Currency(440);
            Mop = new Currency(446);
            Mwk = new Currency(454);
            Myr = new Currency(458);
            Mvr = new Currency(462);
            Mro = new Currency(478);
            Mur = new Currency(480);
            Mxn = new Currency(484);
            Mnt = new Currency(496);
            Mdl = new Currency(498);
            Mad = new Currency(504);
            Omr = new Currency(512);
            Npr = new Currency(524);
            Ang = new Currency(532);
            Awg = new Currency(533);
            Vuv = new Currency(548);
            Nzd = new Currency(554);
            Nio = new Currency(558);
            Ngn = new Currency(566);
            Nok = new Currency(578);
            Pkr = new Currency(586);
            Pab = new Currency(590);
            Pgk = new Currency(598);
            Pyg = new Currency(600);
            Pen = new Currency(604);
            Php = new Currency(608);
            Gwp = new Currency(624);
            Qar = new Currency(634);
            Rub = new Currency(643);
            Rwf = new Currency(646);
            Shp = new Currency(654);
            Std = new Currency(678);
            Sar = new Currency(682);
            Scr = new Currency(690);
            Sll = new Currency(694);
            Sgd = new Currency(702);
            Skk = new Currency(703);
            Vnd = new Currency(704);
            Sos = new Currency(706);
            Zar = new Currency(710);
            Zwd = new Currency(716);
            Szl = new Currency(748);
            Sek = new Currency(752);
            Chf = new Currency(756);
            Syp = new Currency(760);
            Thb = new Currency(764);
            Top = new Currency(776);
            Ttd = new Currency(780);
            Aed = new Currency(784);
            Tnd = new Currency(788);
            Tmm = new Currency(795);
            Ugx = new Currency(800);
            Mkd = new Currency(807);
            Egp = new Currency(818);
            Gbp = new Currency(826);
            Tzs = new Currency(834);
            Usd = new Currency(840);
            Uyu = new Currency(858);
            Uzs = new Currency(860);
            Wst = new Currency(882);
            Yer = new Currency(886);
            Zmk = new Currency(894);
            Twd = new Currency(901);
            Ghs = new Currency(936);
            Vef = new Currency(937);
            Sdg = new Currency(938);
            Rsd = new Currency(941);
            Mzn = new Currency(943);
            Azn = new Currency(944);
            Ron = new Currency(946);
            Try = new Currency(949);
            Xaf = new Currency(950);
            Xcd = new Currency(951);
            Xof = new Currency(952);
            Xpf = new Currency(953);
            Xba = new Currency(955);
            Xbb = new Currency(956);
            Xbc = new Currency(957);
            Xbd = new Currency(958);
            Xau = new Currency(959);
            Xdr = new Currency(960);
            Xag = new Currency(961);
            Xpt = new Currency(962);
            Xts = new Currency(963);
            Xpd = new Currency(964);
            Srd = new Currency(968);
            Mga = new Currency(969);
            Afn = new Currency(971);
            Tjs = new Currency(972);
            Aoa = new Currency(973);
            Byr = new Currency(974);
            Bgn = new Currency(975);
            Cdf = new Currency(976);
            Bam = new Currency(977);
            Eur = new Currency(978);
            Uah = new Currency(980);
            Gel = new Currency(981);
            Pln = new Currency(985);
            Brl = new Currency(986);
            Xxx = new Currency(999);
        }

        public Currency(Int32 isoCurrencyCode)
        {
            if (isoCurrencyCode != 0 && !_currencies.ContainsKey(isoCurrencyCode))
            {
                throw new ArgumentOutOfRangeException("isoCurrencyCode",
                                                      isoCurrencyCode,
                                                      "The value isn't a valid " +
                                                      "ISO 4217 numeric currency code.");
            }

            _id = isoCurrencyCode;
        }

        public Currency(String iso3LetterCodeOrSymbol)
        {
            Currency parsed;

            if (!TryParse(iso3LetterCodeOrSymbol, out parsed))
            {
                throw new ArgumentException("Unknown currency code or symbol: " +
                                            iso3LetterCodeOrSymbol);
            }

            _id = parsed._id;
        }

        public String Name
        {
            get
            {
                var entry = getEntry(_id);
                return entry.Name;
            }
        }

        public String Symbol
        {
            get
            {
                var entry = getEntry(_id);
                return entry.Symbol;
            }
        }

        public String Iso3LetterCode
        {
            get
            {
                var entry = getEntry(_id);
                return entry.Iso3LetterCode;
            }
        }

        public Int32 IsoNumericCode
        {
            get
            {
                var entry = getEntry(_id);
                return entry.IsoNumberCode;
            }
        }

        public static Currency FromCurrentCulture()
        {
            return FromCultureInfo(CultureInfo.CurrentCulture);
        }

        public static Currency FromCultureInfo(CultureInfo cultureInfo)
        {
            Int32 currencyId;

            if (_lcidToIsoCodeLookup.TryGetValue(cultureInfo.LCID, out currencyId))
            {
                return new Currency(currencyId);
            }

            throw new InvalidOperationException("Unknown culture: " + cultureInfo);
        }

        public static Currency FromIso3LetterCode(String code)
        {
            return new Currency(code);
        }

        public static Boolean operator ==(Currency left, Currency right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(Currency left, Currency right)
        {
            return !left.Equals(right);
        }

        public static Boolean TryParse(String s, out Currency currency)
        {
            currency = None;

            Int32 currencyId;

            s = s.Trim();

            if (_codeIndex.TryGetValue(s, out currencyId))
            {
                currency = new Currency(currencyId);
                return true;
            }

            List<Int32> currencyIdList;

            if (_symbolIndex.TryGetValue(s, out currencyIdList))
            {
                if (_lcidToIsoCodeLookup.TryGetValue(Thread.CurrentThread.CurrentCulture.LCID, out currencyId) &&
                    !currencyIdList.Contains(currencyId))
                {
                    currencyId = currencyIdList[0];
                }

                currency = new Currency(currencyId);
                return true;
            }

            return false;
        }

        public override Int32 GetHashCode()
        {
            return unchecked(609502847 ^ _id.GetHashCode());
        }

        public override Boolean Equals(Object obj)
        {
            if (!(obj is Currency))
            {
                return false;
            }

            var other = (Currency)obj;
            return Equals(other);
        }

        public override String ToString()
        {
            return String.Format("{0} ({1})", Name, Iso3LetterCode);
        }

        private static CurrencyTableEntry getEntry(Int32 id)
        {
            CurrencyTableEntry entry;

            if (!_currencies.TryGetValue(id, out entry))
            {
                throw new InvalidOperationException("Unknown currency: " + id);
            }

            return entry;
        }

        #region IEquatable<Currency> Members

        public Boolean Equals(Currency other)
        {
            return _id == other._id;
        }

        #endregion

        #region IFormatProvider Members

        public Object GetFormat(Type formatType)
        {
            if (formatType != typeof(NumberFormatInfo))
            {
                return null;
            }

            var lcids = _isoCodeToLcidLookup[_id];

            var lcid = lcids.Contains(CultureInfo.CurrentCulture.LCID)
                           ? CultureInfo.CurrentCulture.LCID
                           : lcids[0];

            return new CultureInfo(lcid).NumberFormat;
        }

        #endregion

        private struct CurrencyTableEntry : IEquatable<CurrencyTableEntry>
        {
            internal readonly String Iso3LetterCode;
            internal readonly UInt16 IsoNumberCode;
            internal readonly String Name;
            internal readonly String Symbol;

            internal CurrencyTableEntry(String name,
                                        String iso3LetterCode,
                                        UInt16 isoNumberCode,
                                        String symbol)
            {
                Name = name;
                Iso3LetterCode = iso3LetterCode;
                IsoNumberCode = isoNumberCode;
                Symbol = symbol;
            }

            #region IEquatable<CurrencyTableEntry> Members

            public Boolean Equals(CurrencyTableEntry other)
            {
                return IsoNumberCode == other.IsoNumberCode;
            }

            #endregion
        }
    }

    internal static class DictionaryExtensions
    {
        public static String GetValueOrDefault(this IDictionary<String, String> table, String key)
        {
            String value;

            return !table.TryGetValue(key, out value) ? null : value;
        }
    }
}
