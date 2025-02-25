using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class FetchCustomerFromEmailAndMobileRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]

        public string Mobile { get; set; }
    }

    public class FetchCustomerFromEmailAndMobileResponse
    {
        public long CustomerId { get; set; }
        public List<VehicleDetails> CustomerVehicles { get; set; }
    }
}
