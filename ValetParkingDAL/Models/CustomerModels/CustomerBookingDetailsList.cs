using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models.CustomerModels
{

    public class MonthlyBookingDetails
    {
      public  List<CustomerBookingDetailsList> BookingDetails{get;set;}
    }

    public class CustomerBookingDetailsList
    {
        public long BookingId { get; set; }

        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
    
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }

        public long LocationId { get; set; }

        public int RoleId { get; set; }

        public string Role { get; set; }

        public string NumberPlate { get; set; }
    }
}