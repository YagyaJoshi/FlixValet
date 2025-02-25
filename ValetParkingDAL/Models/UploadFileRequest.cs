using ValetParkingDAL.Enums;

namespace ValetParkingDAL.Models
{
    public class UploadFileRequest
    {
        public EFileType? FileType { get; set; } = EFileType.BusinessLogo;
    }
}
