using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingBLL.Interfaces
{
    public interface IPaypal
    {
        (string, string) GetClientToken(PaypalModel model);

        dynamic ChargePayment(PaypalPaymentModel PaypalInfo, decimal Amount, CustomerPaymentDetails model);

        dynamic RefundPayment(CancelBookingDetails model);
        dynamic CancelPayment(CancelBookingDetails model);
    }
}