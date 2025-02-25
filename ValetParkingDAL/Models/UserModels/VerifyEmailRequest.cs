using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
}