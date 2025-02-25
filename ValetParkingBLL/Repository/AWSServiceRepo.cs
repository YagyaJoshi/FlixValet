using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;

namespace ValetParkingBLL.Repository
{
    public class AWSServiceRepo : IAWSService
    {
        private readonly IConfiguration Configuration;

        private readonly AWSS3Details _awsS3Details;

        public AWSServiceRepo(IConfiguration configuration)
        {
            Configuration = configuration;
            _awsS3Details = Configuration.GetSection("AWSS3Details").Get<AWSS3Details>();
        }

        public async Task<string> UploadFile(string base64Img)
        {
            byte[] bytes = Convert.FromBase64String(base64Img);
            MemoryStream stream = new MemoryStream(bytes);

            var credentials = new BasicAWSCredentials(_awsS3Details.AccessKey, _awsS3Details.SecretKey);
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast2
            };

            using var client = new AmazonS3Client(credentials, config);
            var documentName = "Img_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";

            string key = string.Format("{0}/{1}", "CustomerProfiles", documentName);

            // URL for Accessing Document for Demo
            var result = $"https://{_awsS3Details.QRBucketName}.s3.amazonaws.com/{documentName}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = documentName,
                BucketName = _awsS3Details.QRBucketName,
                CannedACL = S3CannedACL.PublicRead
            };

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return result;
        }
    }
}