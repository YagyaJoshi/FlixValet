using System.Threading.Tasks;

namespace ValetParkingBLL.Interfaces
{
    public interface IAWSQueueService
    {
        Task<string> CreateQueue(string queueName);

        Task DeleteQueue(string queueUrl);

        Task SendMessage(string queueURL, string messageBody);
    }
}