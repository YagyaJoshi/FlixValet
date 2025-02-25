using System.Collections.Generic;

namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class PendingStatusRequest
    {
        public List<RefundPendingStatus> RefundStatusList { get; set; }

    }
}