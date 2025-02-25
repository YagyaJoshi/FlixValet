using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
    public class CustomerAddress
    {
        public long CustomerId { get; set; }
        [Required]
        [StringLength(1000)]
        public string Address { get; set; }
        [Required]
        [StringLength(100)]
        public string City { get; set; }

        public string State { get; set; }
        [Required]

        public string StateCode { get; set; }

        public string Country { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Zip code length must be between 5 to 10")]
        public string ZipCode { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}