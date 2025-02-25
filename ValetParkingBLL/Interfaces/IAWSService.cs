using System.Threading.Tasks;

namespace ValetParkingBLL.Interfaces
{
    public interface IAWSService
    {
        public Task<string> UploadFile(string base64Img);
    }
}