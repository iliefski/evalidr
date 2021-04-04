using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PriceCalc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly ILogger<PricingController> _logger;
        private readonly PricingClient client;
        private readonly PricingSettings servicePricingSettings;

        public PricingController(ILogger<PricingController> logger, PricingClient client)
        {
            _logger = logger;
            this.client = client;
            this.servicePricingSettings = client.GetBasePricingSettings();
        }

        [HttpGet]
        [Route("{customerID}/getCost/{startTime}/{endTime}")]

        //startTime and endtime given in ISO8601 format. Indiscriminately uses any freeDays left. 
        public async Task<UsagePeriodCost> GetCustomerCost(string customerID, string startTime, string endTime)
        {   
            DateTime start = DateTime.Parse(startTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime end = DateTime.Parse(endTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var customer = client.GetCustomer(customerID);
            decimal totalCost = 0m;
            var freeDaysLeft = customer.availableFreeDays;
            foreach(Service service in customer.services){
                var serviceName = service.serviceName;

                // Sum up all chargeable days in customers registered active periods within the given timespan
                foreach(TimePeriod timePeriod in service.acitvePeriods){
                    DateTime chargedTimeStart = timePeriod.startDate;
                    DateTime chargedTimeEnd = timePeriod.endDate;
                    if(chargedTimeStart<start){
                        chargedTimeStart=start;
                    }
                    if(chargedTimeEnd>end){
                        chargedTimeEnd = end;
                    }
                    var chargeableDays = getChargeableDaysInPeriod(serviceName, chargedTimeStart, chargedTimeEnd);
                    foreach(DateTime _ in chargeableDays){
                        if(freeDaysLeft>0){
                            freeDaysLeft--;
                        }else{
                            totalCost += timePeriod.price- timePeriod.price*timePeriod.discount;
                        }
                    }
                }
            }
            return new UsagePeriodCost{
                CustomerID = customerID,
                TotalCost = totalCost,
                Currency = servicePricingSettings.Currency,
                UsedFreeDays = customer.availableFreeDays-freeDaysLeft,
                StartTime = start,
                EndTime = end 
            };
        }

        private List<DateTime> getChargeableDaysInPeriod(string serviceName, DateTime startDate, DateTime endDate){
             var allDays = Enumerable
                        .Range(0, int.MaxValue)
                        .Select(index => new DateTime?(startDate.AddDays(index)))
                        .TakeWhile(date => date <= endDate)
                        .ToList();
            List<DateTime> chargeableDays = new();

            // Removing days in time span for which services are free/inactive
            foreach(DateTime date in allDays){
                if(!servicePricingSettings.ServiceFreeDays[serviceName].Contains(date.DayOfWeek.ToString())){
                    chargeableDays.Add(date);
                }
            }
            return chargeableDays;
        }

        [HttpPost]
        [Route("{customerID}/createCustomer/{startTime}")]
        public async Task<string> createCustomer(string customerID, string startTime){
            DateTime start = DateTime.Parse(startTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var res = await client.createCustomer(customerID,start);
            return res;
        }
        [HttpPost]
        [Route("{customerID}/setPrice/{serviceName}/{price}")]
        public async Task<string> SetCustomerServicePrice(string customerID,string serviceName, string price){
            var res = await client.setCustomerServicePrice(customerID,serviceName, decimal.Parse(price));
            return res;
        }

        [HttpPost]
        [Route("{customerID}/setDiscount/{serviceName}/{discountRate}")]
        public async Task<string> SetCustomerServiceDiscount(string customerID,string serviceName, string discountRate){
            var res = await client.setCustomerServiceDiscount(customerID,serviceName, decimal.Parse(discountRate));
            return res;
        }
        [HttpPost]
        [Route("{customerID}/setServiceStartDate/{serviceName}/{customerStartDate}")]
        public async Task<string> SetCustomerServiceStartDate(string customerID,string serviceName, string startDate, decimal price = -1, decimal discount = 0){
            DateTime start = DateTime.Parse(startDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var res = await client.setCustomerServiceStartDate(customerID,serviceName, start);
            return res;
        }
        [HttpPost]
        [Route("{customerID}/setServicePeriod/{serviceName}/{startDate}/{endDate}")]
        public async Task<string> SetCustomerServicePeriod(string customerID,string serviceName, string startDate, string endDate, decimal price =-1, decimal discount = 0){
            DateTime start = DateTime.Parse(startDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime end = DateTime.Parse(endDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var res = await client.setCustomerServicePeriod(customerID,serviceName,start, end, price, discount);
            return res;
        }
        [HttpPost]
        [Route("{customerID}/adjustFreeDays/{freeDays}")]
        public async Task<string> addFreeDays(string customerID,string freeDays){
            var res = await client.adjustCustomerFreeDays(customerID, int.Parse(freeDays));
            return res;
        }
    }
}