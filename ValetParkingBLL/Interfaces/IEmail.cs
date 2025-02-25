using System.Threading.Tasks;

namespace ValetParkingBLL.Interfaces
{
    public interface IEmail
    {
        Task Send(string to, string subject, string html);

        Task WDLogError(string p_URL, string p_Error);
        Task SendPaymentErrorEmail();
    }
}