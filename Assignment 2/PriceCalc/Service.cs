using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace PriceCalc
{
    [BsonIgnoreExtraElements]
    public class Service
    {
        public Service(string serviceName, DateTime startDate, decimal currentPrice, decimal currentDiscountRate, List<TimePeriod> acitvePeriods)
        {
            this.serviceName = serviceName;
            this.startDate = startDate;
            this.currentPrice = currentPrice;
            this.currentDiscountRate = currentDiscountRate;
            this.acitvePeriods = acitvePeriods;
        }
            public string serviceName{get;set;}
            public DateTime startDate{get;set;}
            public decimal currentPrice{get;set;}
            public decimal currentDiscountRate{get;set;}
            public List<TimePeriod> acitvePeriods{get;set;}
    }

    [BsonIgnoreExtraElements]
    public class TimePeriod
    {
        public TimePeriod(DateTime startDate, DateTime endDate, decimal price, decimal discount)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.price = price;
            this.discount = discount;
        }

        public DateTime startDate{get;set;}
            public DateTime endDate{get;set;}
            public decimal price{get;set;}
            public decimal discount{get;set;}
    }
}