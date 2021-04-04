using System.Collections.Generic;

namespace PriceCalc
{
    public class PricingSettings
    {  
        public Dictionary<string,decimal> ServiceBasePrices {get;set;}
        public Dictionary<string,string[]> ServiceFreeDays {get;set;}

        public string[] ServiceNames{get;set;}
        public string Currency{get;set;}

    }
}