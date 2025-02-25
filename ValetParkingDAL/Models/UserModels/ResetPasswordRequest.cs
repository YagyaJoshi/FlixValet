using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token is missing.")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Please enter your Password.")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Password must have minimum 8 characters, with at least 1 upper case letter, 1 numeric and 1 special character.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password fields must be identical. Please try again.")]
        public string ConfirmPassword { get; set; }
    }
}