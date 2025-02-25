using System.Threading.Tasks;
using Square.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingBLL.Interfaces
{
    public interface ISquare
    {

        dynamic ChargePayment(SquareupModel SquareupInfo, decimal Amount, CustomerPaymentDetails model);

        dynamic RefundPayment(CancelBookingDetails model);

        Task<dynamic> GetSquareUpReaderToken(SquareUpRequest model);
    }
}