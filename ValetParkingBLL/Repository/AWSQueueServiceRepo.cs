using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;

namespace ValetParkingBLL.Repository
{
    public class AWSQueueServiceRepo : IAWSQueueService
    {
         private readonly IConfiguration Configuration;

        private readonly AWSSQSDetails _aWSSQSDetails;

        private readonly AmazonSQSConfig _amazonSQSConfig;

        private readonly BasicAWSCredentials _basicAWSCredentials;
        public AWSQueueServiceRepo(IConfiguration configuration)
        {
            Configuration = configuration;
            _aWSSQSDetails = Configuration.GetSection("AWSSQSDetails").Get<AWSSQSDetails>();
            _basicAWSCredentials= new BasicAWSCredentials(_aWSSQSDetails.AccessKey, _aWSSQSDetails.SecretKey);
            _amazonSQSConfig = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.USEast2
            };
        }
        public async Task<string> CreateQueue(string queueName)
        {
            try
            {
                var sqsClient = new AmazonSQSClient(_basicAWSCredentials, _amazonSQSConfig);

                // Create the queue 
                var responseCreate = await sqsClient.CreateQueueAsync(
                    new CreateQueueRequest { QueueName = queueName });

                return responseCreate.QueueUrl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteQueue(string queueUrl)
        {
            try
            {
                var sqsClient = new AmazonSQSClient(_basicAWSCredentials, _amazonSQSConfig);

                await sqsClient.DeleteQueueAsync(queueUrl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SendMessage(string queueURL, string messageBody)
        {
            try
            {
                var sqsClient = new AmazonSQSClient(_basicAWSCredentials, _amazonSQSConfig);

                SendMessageResponse responseSendMsg = await sqsClient.SendMessageAsync(queueURL, messageBody);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}