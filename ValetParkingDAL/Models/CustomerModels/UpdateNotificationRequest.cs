using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class UpdateNotificationRequest
    {
        [Required]
        public long CustomerInfoId { get; set; }
        [Required]
        public string ReservationNotificationMode { get; set; }
        [Required]
        public string PaymentNotificationMode { get; set; }
    }
}