using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class VerifyOTPRequest
    {

        [Required]
        public string OTP { get; set; }
        public long CustomerInfoId { get; set; }

    }
}