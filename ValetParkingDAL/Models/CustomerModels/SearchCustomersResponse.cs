using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class SearchCustomersFromFilterResponse
    {
        public List<CustomerDetailList> CustomerDetails { get; set; }
        public bool IsCustomerExists { get; set; }
    }

    public class CustomerDetailList
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}