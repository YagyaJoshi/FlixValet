using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class SetGuestOtpRequest
    {
        public long CustomerId { get; set; }
        [Required]
        public string Mobile { get; set; }

    }
}