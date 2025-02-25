using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class AccountReconcilationModel
    {
        public decimal CashCollected { get; set; }

        public decimal CardCollected { get; set; }

        public int TicketsIssued { get; set; }

        public int OpenTickets { get; set; }

        public List<DepositReport> DepositReport { get; set; }

        public int DepositTotal { get; set; }

        public List<BookingDataModel> BookingDataList { get; set; }

        public int BookingDataTotal { get; set; }

        public Summary Summary { get; set; }

    }

    public class DepositReport
    {
        public string Depositor { get; set; }

        public string Source { get; set; }

        public decimal DepositedAmount { get; set; }
    }

    public class BookingDataModel
    {
        public string NumberPlate { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Source { get; set; }
    }

    public class Summary
    {
        public decimal CashRevenue { get; set; }
        public decimal CardRevenue { get; set; }
        public decimal Deficit { get; set; }
        public decimal Total { get; set; }
    }
}