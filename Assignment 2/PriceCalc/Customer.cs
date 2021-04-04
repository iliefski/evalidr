using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace PriceCalc
{       
    
    [BsonIgnoreExtraElements]
    public class Customer
    {
        public Customer(string customerID, DateTime customerStartDate, int availableFreeDays, List<Service> services)
        {
            this.customerID = customerID;
            this.customerStartDate = customerStartDate;
            this.availableFreeDays = availableFreeDays;
            this.services = services;
        }
            public string customerID{get; set;}
            public DateTime customerStartDate{get;set;}
            public int availableFreeDays{get;set;}
            public List<Service> services{get;set;}
    }
}