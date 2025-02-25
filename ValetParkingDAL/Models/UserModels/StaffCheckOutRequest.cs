using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class StaffCheckOutRequest
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public DateTime? CheckOutTime { get; set; }
    }
}