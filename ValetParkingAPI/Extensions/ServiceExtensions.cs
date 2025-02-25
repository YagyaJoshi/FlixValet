using Microsoft.Extensions.DependencyInjection;
using ValetParkingBLL.Helpers;
using ValetParkingBLL.Interfaces;
using ValetParkingBLL.Repository;

namespace ValetParkingAPI.Extensions
{
	public static class ServiceExtensions
	{
		public static void RegisterDI(this IServiceCollection services)
		{
			services.AddScoped<IAccount, AccountRepo>();
			services.AddScoped<IRegion, RegionRepo>();
			services.AddScoped<IParking, ParkingRepo>();
			services.AddScoped<IStaff, StaffRepo>();
			services.AddScoped<ICustomer, CustomerRepo>();
			services.AddScoped<IEmail, EmailRepo>();
			services.AddScoped<ICache, CacheRepo>();
			services.AddScoped<IMaster, MasterRepo>();
			services.AddScoped<ISMS, SMSRepo>();
			services.AddScoped<IFirebase, FirebaseRepo>();
			services.AddScoped<IStripe, StripeRepo>();
			services.AddScoped<ISquare, SquareRepo>();
			services.AddScoped<INumberPlateRecognition, NumberPlateRecognitionRepo>();
			services.AddSingleton<ParkingHelper>();
			services.AddSingleton<DateTimeHelper>();
			services.AddScoped<IJob, JobRepo>();
			services.AddScoped<IPaypal, PaypalRepo>();
			services.AddScoped<IQR, QRRepo>();
			services.AddScoped<IAWSService, AWSServiceRepo>();
			services.AddScoped<IStatistics, StatisticsRepo>();
			services.AddSingleton<StatisticsHelper>();
			services.AddScoped<IAWSQueueService, AWSQueueServiceRepo>();
			services.AddScoped<IWebhook, WebhookRepo>();
		}
	}
}
