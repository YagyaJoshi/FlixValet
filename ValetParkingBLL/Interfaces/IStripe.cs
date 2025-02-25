using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingBLL.Interfaces
{
    public interface IStripe
    {
        dynamic ChargePayment(StripeModel StripeInfo, decimal Amount, CustomerPaymentDetails model);

        StripeModel GetCredentials(long ParkingLocationId);

        dynamic RefundPayment(CancelBookingDetails model);
    }
}