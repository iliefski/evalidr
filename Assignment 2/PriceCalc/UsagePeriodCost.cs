using System;

namespace PriceCalc
{
    public class UsagePeriodCost
    {
        public string CustomerID{get;set;}
        public decimal TotalCost{get; set;}
        public string Currency{get; set;}
        public int UsedFreeDays{get;set;}
        public DateTime StartTime {get; set;}
        public DateTime EndTime {get; set;}
    }
}