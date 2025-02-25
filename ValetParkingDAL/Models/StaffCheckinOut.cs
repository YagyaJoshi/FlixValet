using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
    public class StaffCheckinOut
    {

        public long Id { get; set; }
        [Required]
        public long UserId { get; set; }
        [Required]
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }

    }
}
