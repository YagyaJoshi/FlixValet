using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestUserRequest
    {
        [Required]
      
        public string Mobile { get; set; }
        public string OTP { get; set; }
    }
}