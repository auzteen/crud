using Citadel.Model.Customer;
using System.Collections.Generic;

namespace Citadel.Services
{
    public interface ICustomerService
    {
        public List<CustomerData> GetCustomer(string companyShortName);
        public List<CustomerData> GetAllCustomers();

        public void AddCustomer(string companyShortName, string companyName);

        public void AddCustomer(string companyShortName);
    }
}