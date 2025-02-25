using System.Threading.Tasks;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class UpdateQRCodeModel
    {
        public long CustomerBookingId { get; set; }
        public string QRCodePath { get; set; }
    }
}