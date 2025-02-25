using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your Password.")]
        public string Password { get; set; }


        public string AppVersionCode { get; set; }


        public string DeviceType { get; set; }

        public string DeviceName { get; set; }
        public string DeviceToken { get; set; }

        public string OSVersion { get; set; }

        public string TimeZoneId { get; set; }

        public bool IsLoginFromValetApp { get; set; }
        public string BrowserDeviceToken { get; set; }
    }
}