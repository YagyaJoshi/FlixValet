using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{


    public class UpdateOfficeEmployeePayment
    {
        public long BookingId { get; set; }
        public long UserId { get; set; }
        public decimal UnpaidAmount { get; set; }
        public int CurrencyId { get; set; }
        public string Notes { get; set; }
    }
    public class OfficeEmployeeListModel
    {
        public List<UpdateOfficeEmployeePayment> OfficeEmployeePaymentList { get; set; }
        public DateTime CurrentDate { get; set; }

    }
}