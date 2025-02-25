using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class OwnerPaymentSettings
    {
        public long PaymentSettingsId { get; set; }
        public long ParkingBusinessOwnerId { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string AccessToken { get; set; }
        public string ApplicationId { get; set; }
        public string LocationId { get; set; }

        public bool IsProduction { get; set; }
    }
}