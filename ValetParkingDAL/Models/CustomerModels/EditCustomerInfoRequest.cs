using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class EditCustomerInfoRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "Please provide a valid UserId")]
        public long UserId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please provide a valid CustomerId")]
        public long CustomerId { get; set; }

        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        public string Email { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }



        public string Address { get; set; }


        public string City { get; set; }

        public string State { get; set; }


        public string StateCode { get; set; }

        public string Country { get; set; }


        public string CountryCode { get; set; }


        public string ZipCode { get; set; }

        public string ProfilePic { get; set; }
        public char Gender { get; set; } = 'U';
        public string MobileCode { get; set; }
        public string PaypalCustomerId { get; set; }

    }
}