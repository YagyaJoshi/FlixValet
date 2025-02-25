using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        public string Email { get; set; }

        public string BaseUrl { get; set; }
    }
}