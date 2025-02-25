using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.UserModels
{
    public class ProfileEdit
    {
        public long UserId { get; set; }

        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        public string Email { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        public string MobileCode { get; set; }
        public char Gender { get; set; } = 'U';
        public string ProfilePic { get; set; }

        public bool IsFromCustomerApp { get; set; }

    }
}