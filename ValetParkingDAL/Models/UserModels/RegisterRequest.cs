using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class RegisterRequest
    {

        [Required(ErrorMessage = "Please enter your First Name.")]
        [RegularExpression(@"^[a-zA-Z ]*$", ErrorMessage = "Please Enter only Alphabets based Input. No Digits No Special Character.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your Last Name.")]
        [RegularExpression(@"^[a-zA-Z ]*$", ErrorMessage = "Please Enter only Alphabets based Input. No Digits No Special Character.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter your Mobile.")]
        [Phone]
        public string Mobile { get; set; }
        public string ProfilePic { get; set; }

        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your Password.")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Password must have minimum 8 characters, with at least 1 upper case letter, 1 numeric and 1 special character.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password fields must be identical. Please try again.")]
        public string ConfirmPassword { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
        public int AppVersion { get; set; }
        public long CreatedBy { get; set; }
        public char Gender { get; set; } ='U';
        public string AppVersionCode { get; set; }
    }
}