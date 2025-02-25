using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class ProfilePicRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "Please enter valid User Id.")]
        public long UserId { get; set; }
        [Required]
        public string ProfilePic { get; set; }
    }
}