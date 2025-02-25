using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class DepositReportResponse
    {
        public List<DepositReport> DepositReport { get; set; }
        public decimal TotalAmount { get; set; }
        public int Total { get; set; }
    }

    public class DepositReport
    {
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public DateTime DepositedDate { get; set; }
        public string DepositedVia { get; set; }
        public decimal Amount { get; set; }
        public string LocationName { get; set; }

    }
}