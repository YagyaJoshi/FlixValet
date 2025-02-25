namespace ValetParkingDAL.Models
{
    public class AWSS3Details
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string QRBucketName { get; set; }
        public string ImageBucketName { get; set; }
    }
}