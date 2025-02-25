using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class ParkingLocationRequest
    {
        public long Id { get; set; }
        [Required]
        public long UserId { get; set; }
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
        [Range(0, Int64.MaxValue, ErrorMessage = "Please enter a valid Zipcode")]
        public string ZipCode { get; set; }

        public string LocationPic { get; set; }

        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        [Required]

        [Range(1, 9999, ErrorMessage = "Maximum number of spaces should be 9999")]
        public int No_of_Spaces { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public string TimeZoneId { get; set; }

        public string TimeZone { get; set; }
        public string Instructions { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [Required]
        [StringLength(255)]
        public string LocationName { get; set; }
        [Required]
        public decimal OverSizedChargesRegular { get; set; }
        [Required]
        public decimal OverSizedChargesMonthly { get; set; }
        [Required]
        public long ParkingBusinessOwnerId { get; set; }

        [Range(0, 100, ErrorMessage = "Please enter a valid Tax")]
        public decimal Tax { get; set; }
        public decimal ConvenienceFee { get; set; }
        public string MobileCode { get; set; }
        public DateTime StartDate { get; set; }
        public List<List<ParkingLocationTimingRequest>> ParkingTimings { get; set; }
        public List<List<ParkingLocationRateRequest>> ParkingRates { get; set; }
        public List<ParkingLocationEarlyBirdOffer> EarlyBirdOffer { get; set; }


        public bool IsMonthlySubscription { get; set; }
        public long PricingPlanId { get; set; }


        public List<ParkingLocationNightFareOffer> NightFareOffer { get; set; }

    }
}