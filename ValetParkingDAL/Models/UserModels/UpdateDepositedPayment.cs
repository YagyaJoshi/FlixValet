using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class UpdateDepositedPayment
    {
        public long UserId { get; set; }
        public string DepositedVia { get; set; }
        public DateTime DepositedDate { get; set; }
    }
}