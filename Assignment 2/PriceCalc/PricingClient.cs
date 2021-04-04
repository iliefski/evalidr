using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;


namespace PriceCalc
{
    public class PricingClient
    {
        private readonly ServiceSettings settings;
        private readonly PricingSettings serviceSettings;

        private readonly MongoClient client;
        private readonly IMongoCollection<Customer> Customers;
        public PricingClient( IOptions<PricingSettings> servicesPricingSettings, IOptions<ServiceSettings> settings)
        {
            this.serviceSettings = servicesPricingSettings.Value;
            this.settings = settings.Value;
            client = new MongoClient(this.settings.ConnectionString);
            var database = client.GetDatabase(this.settings.DatabaseName);
            Customers = database.GetCollection<Customer>(this.settings.CollectionName);
        }

		internal PricingSettings GetBasePricingSettings(){
            return serviceSettings;
        }
        public record Response(string status, string message);
        internal Customer GetCustomer(string customerID)
        {
           var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();
            if(customer is not null){
                return customer;
            }
            return customer ?? throw new ArgumentException("No customer with found with that ID.");
            
        }

        // gets customer from database
        internal async Task<string> createCustomer(string customerID,DateTime startDate){
            var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();
            if(customer is not null){
                throw new ArgumentException("Customer with that ID already exists");
            }
                await Customers.InsertOneAsync(new Customer(customerID,startDate,0, new List<Service>()));
            return "success";
        }

        // sets current service price
        internal async Task<string> setCustomerServicePrice(string customerID, string serviceName, decimal price){
            var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();

            if(price<0){
                throw new ArgumentException("Non-positive pricing unallowed");
            }
            var serviceIndex = customer.services.FindIndex(s=>s.serviceName=="serviceName"); 
            if (serviceIndex<0){
                throw new ArgumentException("Customer is not a user of service");
            }   
            if(customer is not null){     
                UpdateDefinition<Customer> update = Builders<Customer>.Update
                    .Set(c => c.services[serviceIndex].currentPrice, price);
                var res = await Customers
                    .FindOneAndUpdateAsync(Builders<Customer>.Filter
                    .Where(c => c.customerID == customerID),update); 
            }else{
                throw new ArgumentException("Invalid customerID");
            } 
            return "success";
        }

        // sets current service discount for customer
        internal  async Task<string> setCustomerServiceDiscount(string customerID, string serviceName, decimal discount){
            var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();
            var serviceIndex = customer.services.FindIndex(s=>s.serviceName=="serviceName"); 
            if (serviceIndex<0){
                throw new ArgumentException("customer is not a user of service");
            } 
            if(customer is not null){
                UpdateDefinition<Customer> update = Builders<Customer>.Update.Set(c => c.services[serviceIndex].currentDiscountRate, discount);

                var res = await Customers.FindOneAndUpdateAsync(
                    Builders<Customer>.Filter.Where(c => c.customerID == customerID),
                    update);
                    
            }else{
                throw new ArgumentException("Invalid customerID");
            } 
            return "success";

        }

        // Sets a start date for a for customer, implying service use from startdate to today -> adds days for customer over that time
        internal async Task<string> setCustomerServiceStartDate(string customerID, string serviceName, DateTime startDate)
        {
            var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();

            var serviceIndex = customer.services.FindIndex(s=>s.serviceName=="serviceName");
            if(customer is not null && serviceSettings.ServiceNames.Contains(serviceName)){
				var filter = Builders<Customer>.Filter.Eq(c => c.customerID , customerID);
				UpdateDefinition<Customer> update;	
                if (serviceIndex>=0){
                    update = Builders<Customer>.Update
                            .Set(c => c.services[serviceIndex].startDate, startDate);    
                }else{
					update = Builders<Customer>.Update
							.Push<Service>(c => c.services, 
								new Service(serviceName,startDate,serviceSettings.ServiceBasePrices[serviceName],0,new List<TimePeriod>()));
                }
				 var res = await Customers
                                .FindOneAndUpdateAsync(filter,update); 
            }else{
                throw new ArgumentException("Invalid customerID");
            } 
            return "success";
        }   

        // if no custom customer price set (default == -1), use basePrice of service
        private decimal baseOrUserPrice(decimal basePrice, decimal userPrice){
            if(userPrice>0){
                return userPrice;
            }
            return basePrice;
        }
        

        // Defines a period of active service use for customer
        internal async Task<string> setCustomerServicePeriod(string customerID, string serviceName, DateTime startDate, DateTime endDate, decimal userPrice,decimal discount)
        {
             var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();

            if(!serviceSettings.ServiceNames.Contains(serviceName)){
                throw new ArgumentException("Non-existant service");
            }
            var serviceIndex = customer.services.FindIndex(s=>s.serviceName=="serviceName");
            var price = baseOrUserPrice(serviceSettings.ServiceBasePrices[serviceName],userPrice);

            if(customer is not null){
               	var filter = Builders<Customer>.Filter.Eq(c => c.customerID , customerID);
				UpdateDefinition<Customer> update;	

				// customer has used service before => add new usage period, adjust Service "startDate" (customers first usage of service) if necessary
                if (serviceIndex>=0){
                    update = Builders<Customer>.Update
                            .Push(c => c.services[serviceIndex].acitvePeriods, new TimePeriod(startDate,endDate,price,discount));
					if(customer.services[serviceIndex].startDate<startDate){
						update.Set(c => c.services[serviceIndex].startDate, startDate);
					}
				
				// customer has not used service before, add service to customers services with usage period and set startDate
                }else{
					update = Builders<Customer>.Update
							.Push<Service>(c => c.services, 
								new Service(serviceName,startDate,price,0,new List<TimePeriod>(){new TimePeriod(startDate,endDate,price,discount)}));
                }
				 var res = await Customers
                                .FindOneAndUpdateAsync(filter,update); 
            }else{
                throw new ArgumentException("Invalid customerID");
            } 
            return "success";
        }

        // Positively or negatively adjust freedays available to customer
        internal async Task<string> adjustCustomerFreeDays(string customerID, int freeDays)
        {
			var customer = Customers
                            .Find(any => any.customerID == customerID)
                            .FirstOrDefault();
            if(customer is not null){     
                UpdateDefinition<Customer> update = Builders<Customer>.Update
                    .Set(c => c.availableFreeDays, customer.availableFreeDays+freeDays);
                var res = await Customers
                    .FindOneAndUpdateAsync(Builders<Customer>.Filter
                    .Where(c => c.customerID == customerID),update); 
            }else{
                throw new ArgumentException("Invalid customerID or customer is not a user of service");
            } 
            return "success";
        }
    }
}