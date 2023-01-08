using System;
using Citadel.Model.Customer;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Citadel.Services.Data;

namespace Citadel.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ILogger<ICustomerService> _log;
        private readonly IDatabaseService _dbservice;

        public CustomerService(ILogger<ICustomerService> log, IDatabaseService dbservice)
        {
            _log = log;
            _dbservice = dbservice;
        }

        public List<CustomerData> GetCustomer(string companyShortName)
        {
            string sqlQuery = $"SELECT * FROM Customer WHERE CompanyShortName = '{companyShortName}'";
            return GetCustomerFromQuery(sqlQuery);
        }

        public List<CustomerData> GetAllCustomers()
        {
            string sqlQuery = "SELECT * FROM Customer";
            return GetCustomerFromQuery(sqlQuery);
        }

        private List<CustomerData> GetCustomerFromQuery(string sqlQuery)
        {
            List<CustomerData> customerDataList = new List<CustomerData>();
            try
            {
                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();
                SqlCommand command = new SqlCommand(sqlQuery, conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new NoRecordsFoundException("No customer information found");
                    while (reader.Read())
                    {
                        CustomerData customerData = new CustomerData();
                        customerData.type = Constant.TYPE_CUSTOMERS;
                        customerData.id = reader.GetString(0).ToString();
                        Customer customer = new Customer()
                        {
                            CompanyShort = reader.GetString(0).ToString(),
                            CustomerName = reader.IsDBNull(1) ? null : reader.GetString(1)
                        };
                        customerData.attributes = customer;
                        customerDataList.Add(customerData);
                    }
                }
                conn.Close();

            }
            catch (NoRecordsFoundException e)
            {
                _log.LogInformation(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
            return customerDataList;
        }

        public void AddCustomer(string companyShortName, string companyName)
        {
            try
            {
                companyShortName = companyShortName.Trim();
                if (String.IsNullOrEmpty(companyShortName))
                    throw new BadRequestException("Bad format - missing company short");

                string sqlCustomerCount = $"SELECT COUNT(*) FROM Customer WHERE CompanyShortName = '{companyShortName}'";
                int numCustomers = _dbservice.GetScalarValueFromQuery(sqlCustomerCount);
                if (numCustomers > 0)
                    throw new ExistingCustomerException($"Customer '{companyShortName}' exists already");

                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();

                string sqlQuery = $"INSERT INTO Customer (CompanyShortName, CustomerName) VALUES ('{companyShortName}', '{companyName}')";
                SqlCommand comm = new SqlCommand(sqlQuery, conn);
                comm.ExecuteNonQuery();
                comm.Dispose();
                _log.LogInformation($"New customer '{companyShortName}' added.");
            }
            catch (ExistingCustomerException e)
            {
                _log.LogWarning(e.Message);
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
        }

        public void AddCustomer(string companyShortName)
        {
            this.AddCustomer(companyShortName, "");
        }

    }
}