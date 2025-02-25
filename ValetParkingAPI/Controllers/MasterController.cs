using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.UserModels;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using ValetParkingAPI.Resources;
using ValetParkingDAL.Models.PaymentModels.cs;
using ValetParkingDAL.Enums;
using System.Threading.Tasks;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models.NumberPlateRecogModels;
using VehicleDetails = ValetParkingDAL.Models.NumberPlateRecogModels.VehicleDetails;
using System.Text.Json;
using AutoMapper;
using System.Globalization;


namespace ValetParkingAPI.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class MasterController : ControllerBase
	{
		private readonly IParking _parkingRepo;
		private readonly IStaff _staffRepo;
		private readonly ICustomer _customerRepo;
		private readonly IMaster _masterRepo;
		private readonly IStripe _stripeRepo;
		private readonly ISquare _squareRepo;
		public readonly IPaypal _paypalRepo;
		private readonly INumberPlateRecognition _numberPlateRecognitionRepo;
		private readonly IFirebase _firebaseRepo;
		private readonly IQR _qRRepo;
		private readonly IAWSService aWSService;
		private readonly IConfiguration _configuration;
		private readonly IRegion _regionRepo;
		private readonly IEmail _mailService;
		private readonly ISMS _smsRepo;
		private readonly ParkingHelper _parkingHelper;
		private readonly ICache _cacheRepo;
		private readonly IMapper _mapper;
		private readonly ResourceMsgs _resourceMsgs;
		private readonly IStringLocalizer _localizer;

		public MasterController(IParking parkingRepo, IStaff staffRepo, ICustomer customerRepo, IConfiguration configuration, IRegion regionRepo, IEmail mailService, ISMS smsRepo, ParkingHelper parkingHelper, ICache cacheRepo, IMaster masterRepo, IStripe stripeRepo, ISquare squareRepo, IPaypal paypalRepo, INumberPlateRecognition numberPlateRecognitionRepo, IFirebase firebaseRepo, IQR qRRepo, IAWSService aWSService, IStringLocalizer<Resource> localizer, IMapper mapper)
		{
			_parkingRepo = parkingRepo;
			_mailService = mailService;
			_smsRepo = smsRepo;
			_parkingHelper = parkingHelper;
			_cacheRepo = cacheRepo;
			_staffRepo = staffRepo;
			_configuration = configuration;
			_regionRepo = regionRepo;
			_customerRepo = customerRepo;
			_masterRepo = masterRepo;
			_stripeRepo = stripeRepo;
			_squareRepo = squareRepo;
			_numberPlateRecognitionRepo = numberPlateRecognitionRepo;
			_firebaseRepo = firebaseRepo;
			_qRRepo = qRRepo;
			_paypalRepo = paypalRepo;
			this.aWSService = aWSService;
			_localizer = localizer;

			var config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<BookingRequest, CustomerPaymentDetails>();
			});
			_mapper = config.CreateMapper();

			_resourceMsgs = _configuration.GetSection("ResourceMsgs").Get<ResourceMsgs>();
		}

		[Authorize(Roles = "Valet,ParkingManager,SuperAdmin,ParkingAdmin,ParkingSupervisor")]
		[HttpGet("GetLocationsByUser")]
		public Response GetLocationsByUser(long ParkingBusinessOwnerId, long UserId)
		{
			Response response = new Response();
			try
			{
				if (ParkingBusinessOwnerId.Equals(0) || UserId.Equals(0))
					throw new AppException(_localizer["OwnerAndUserRequired"]);
				response.Data = _masterRepo.GetLocationsByUser(ParkingBusinessOwnerId, UserId);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetLocationByUser - " + ParkingBusinessOwnerId + UserId, ex.Message);
			}
			return response;
		}

		[HttpPost("EditUserProfile")]
		public Response EditUserProfile(ProfileEdit model)
		{
			Response response = new Response();
			try
			{
				response.Data = new CommonId { Id = _masterRepo.EditUserProfile(model) };
				response.Status = true;
				response.Message = _localizer["UpdateProfile"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("EditUserProfile - ", ex.Message);
			}
			return response;
		}


		[HttpGet("GetUserProfileDetails")]
		public Response GetUserProfileDetails(long UserId)
		{
			Response response = new Response();
			try
			{
				if (User.Equals(0))
					throw new AppException(_localizer["UserIdRequired"]);
				response.Data = _masterRepo.GetUserProfileDetails(UserId);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetUserProfileDetails - " + UserId, ex.Message);
			}
			return response;
		}


		[AllowAnonymous]
		// [Authorize(Roles = "Valet,ParkingManager,Customer")]
		[HttpPost("BookParkingLocationv1")]
		public Response BookParkingLocationv1(BookingRequest model)
		{

			decimal Amount;

			var timeZones = _cacheRepo.CachedTimeZones();
			model.TimeZone = timeZones.Where(e => e.TimeZoneId == model.TimeZoneId).Select(a => a.Name).FirstOrDefault();


			Response response = new Response();
			try
			{
				if (model.TotalAmount > 0 && model.BookingCategoryId != (int)EBookingCategories.ChargeBack && string.IsNullOrEmpty(model.PaymentMode))
					throw new AppException("PaymentMode is required.");

				object paymentResponse = null; CustomerPaymentDetails customerDetails = new CustomerPaymentDetails();

				if (!model.CustomerId.HasValue)
				{
					model.IsGuestAddRequired = true;
					model.Mobile ??= model.GuestInfo.Mobile;
					model.Email ??= model.GuestInfo.Email;
				}


				if (model.IsEarlyBirdOfferApplied)
					Amount = Convert.ToDecimal(model.EarlyBirdFinalAmount);
				else
					Amount = Convert.ToDecimal(model.FinalAmount);


				if (!string.IsNullOrEmpty(model.PaymentMode))
				{
					if (model.PaymentMode.ToLower() != EPaymentMode.Cash.ToString().ToLower()
					 && model.IsPaymentFromCustomerSite && model.PaymentMode.ToLower() != EPaymentMode.SquareCard.ToString().ToLower() && model.PaymentMode.ToLower() != EPaymentMode.PayLater.ToString().ToLower())
					{
						customerDetails = _mapper.Map<CustomerPaymentDetails>(model);
						customerDetails.CustomerName = model.FirstName + " " + model.LastName;
						customerDetails.PaypalCustomerId = model.PaypalCustomerId;

						string mode = EPaymentMode.Square.ToString().ToLower();

						if (model.PaymentMode.ToLower().Equals(EPaymentMode.Stripe.ToString().ToLower()))
						{
							paymentResponse = _stripeRepo.ChargePayment(model.StripeInfo, Amount, customerDetails);
							if (paymentResponse is StripeErrorResponse)
								throw new AppException(((StripeErrorResponse)paymentResponse).error.message);

						}
						if (model.PaymentMode.ToLower().Equals(EPaymentMode.Square.ToString().ToLower()))
						{
							paymentResponse = _squareRepo.ChargePayment(model.SquareupInfo, Amount, customerDetails);
							if (paymentResponse is SquareupErrorResponse)
								throw new AppException(((SquareupErrorResponse)paymentResponse).errors.FirstOrDefault().detail);
						}
						if (model.PaymentMode.ToLower().Equals(EPaymentMode.Paypal.ToString().ToLower()))
						{
							paymentResponse = _paypalRepo.ChargePayment(model.PaypalInfo, Amount, customerDetails);
							if (paymentResponse is PaypalErrorResponse)
								throw new AppException(((PaypalErrorResponse)paymentResponse).Message);
						}
						if (paymentResponse == null)
							throw new AppException(_localizer["PaymentUnsuccessful"]);
					}
				}

				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
				//model.CurrentDate = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

				//Added by narsing because of 2 minute difference
				TimeSpan StartTime = TimeSpan.Parse(model.StartTime);
				model.CurrentDate = Convert.ToDateTime(model.StartDate.Date + StartTime);

				PostBookingModel postBookingModel = _parkingRepo.BookParkingLocation_v1(model, paymentResponse);

				if (postBookingModel != null)
				{
					_customerRepo.UpdateQRCode(postBookingModel.BookingId, model.LogoUrl);
				}

				response.Message = _localizer["BookParkingLocation"];

				postBookingModel.CustomerMessage = _localizer["BookingConfirmedMsgToCustomer"];

				postBookingModel.CustomerTitle = _localizer["BookingConfirmedTitleToCustomer"];
				postBookingModel.StaffTitle = _localizer["BookingConfirmedTitleToValet"];
				postBookingModel.StaffMessage = _localizer["BookingConfirmedMsgToValet"];
				postBookingModel.ElectronicPaymentMessage = $"{_localizer["BaseUrl"]}/ElectronicPayment?BookingId={postBookingModel.BookingId}";

				if (postBookingModel != null)
				{
					//^ !model.PaymentMode.ToLower().Equals(EPaymentMode.PayLater.ToString().ToLower()


					Task.Run(
						() => _customerRepo.PostBookingActions(postBookingModel, model, Amount));


				}
				
				if (model.GateSettingId.HasValue)
				{
					long gateSettingId = model.GateSettingId.Value;
					_parkingRepo.OpenCloseGate(gateSettingId);
				}

				if (postBookingModel == null) throw new AppException(_localizer["UnableToBook"]);
				response.Data = new BookingIdResponse { BookingId = postBookingModel.BookingId };
				response.Status = true;

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}

			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("BookParkingLocation", ex.Message);
			}

			return response;
		}


		[AllowAnonymous]
		// [Authorize(Roles = "Valet,ParkingManager")]
		[HttpGet("GetBookingDetails")]
		public Response GetBookingDetails(long Id, DateTime CurrentDate)
		{

			Response response = new Response();
			try
			{

				BookingDetailResponse bookingDetails = _customerRepo.GetBookingDetails(Id, CurrentDate);
				response.Data = bookingDetails;
				if (bookingDetails == null)
					throw new AppException(_localizer["BookingNotFound"]);

				else
				{
					bookingDetails.BookingMessage = String.Format(bookingDetails.BookingType.ToLower().Equals("monthly") ? _localizer["MonthlyBookingMessage"] : _localizer["BookingMessage"], bookingDetails.StartDate.ToString("MMM dd"), bookingDetails.LastBookingDate?.ToString("MMM dd"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.StartTime)).ToString("hh:mm tt"), (bookingDetails.StartDate + TimeSpan.Parse(bookingDetails.EndTime)).ToString("hh:mm tt"), bookingDetails.MaxStay);

				}
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetBookingDetails - " + Id, ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		[HttpPost("GetAppVersion")]
		public Response GetAppVersion(AppVersionModel model)
		{
			Response response = new Response();
			try
			{

				AppVersionResponse AppVersion = _masterRepo.GetAppVersion(model);
				response.Data = AppVersion;

				response.Status = true;
				if (AppVersion != null)
				{
					if (model.AppVersionCode == AppVersion.AppVersionCode)
					{
						AppVersion.IsMandatoryUpdate = false;
						AppVersion.UpdateMessage = _localizer["UpToDate"];
					}
					else
					{
						AppVersion.UpdateMessage = String.Format(_localizer["AppVersionMessage"], AppVersion.AppVersionCode);
					}
					response.Message = _localizer["RequestSuccessful"];
				}
				else throw new AppException(_localizer["RecordNotFound"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}

			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetAppVersion - ", ex.Message);
			}
			return response;
		}


		[HttpGet("GetManufacturerMaster")]
		public Response GetManufacturerMaster()
		{
			Response response = new Response();
			try
			{
				response.Data = _masterRepo.GetManufacturerMaster();
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetManufacturerMaster - ", ex.Message);
			}
			return response;
		}

		[HttpGet("GetColorMaster")]
		public Response GetColorMaster()
		{
			Response response = new Response();
			try
			{
				response.Data = _masterRepo.GetColorMaster();
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetColorMaster - ", ex.Message);
			}
			return response;
		}

		[HttpGet("GetVehicleTypes")]
		public Response GetVehicleTypes()
		{
			Response response = new Response();
			try
			{
				response.Data = _masterRepo.GetVehicleTypes();
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetVehicleTypes - ", ex.Message);
			}
			return response;
		}

		[HttpGet("GetVehicleMasterData")]
		public Response GetVehicleMasterData()
		{
			Response response = new Response();
			try
			{
				response.Data = _cacheRepo.CachedVehicleMasterData();
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetVehicleTypes - ", ex.Message);
			}
			return response;
		}


		[HttpPost("UpdateProfilePic")]
		public Response UpdateProfilePic(ProfilePicRequest model)
		{
			Response response = new Response();
			try
			{
				_masterRepo.UpdateProfilePic(model);
				response.Status = true;
				response.Message = _localizer["ProfileUploadSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("UpdateProfilePic - ", ex.Message);
			}
			return response;
		}

		[AllowAnonymous]
		[HttpGet("GetOtp")]
		public Response GetOtp(string Mobile)
		{
			Response response = new Response();
			try
			{

				// DateTime date = DateTime.Now.AddDays(29);
				// ReadOnlyCollection<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones();
				// //TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Calcutta");
				// string format = "d";
				// string dateString = "06/15/2008 06:00 PM";
				// dateString = DateTime.Now.ToString("MM/dd/yyyy HH:mm");

				// DateTimeOffset clientTime = DateTimeOffset.ParseExact(dateString, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
				// DateTimeOffset serverTime = clientTime.ToOffset(TimeSpan.Parse("-07:00"));
				// TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
				// TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
				// DateTime dt = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, timeZoneInfo);
				// DateTime d = new DateTime(2020, 02, 01);
				// int days = System.DateTime.DaysInMonth(d.Year, d.Month);

				// TimeSpan ts7 = new TimeSpan(14, 0, 0);

				// TimeSpan ts5 = TimeSpan.Parse("14");
				// string tsstr = ts7.ToString();

				// TimeSpan ts = TimeSpan.Parse("12:00");

				// TimeSpan ts1 = TimeSpan.Parse("12:30");
				// string ss = ts1.ToString();
				// TimeSpan ts2 = new TimeSpan(23, 00, 00);
				// ts2 += TimeSpan.FromHours(1);

				// if (TimeSpan.Parse("12:20") < ts1 && TimeSpan.Parse("12:20") > ts)
				// {
				//     //true
				// }
				// else
				// {
				//     //false
				// }

				// #region ff
				// //   aWSService.UploadFile("");

				// // int x = (int)Math.Floor(Math.Log10(n) + 1);

				// // int g = n % 10;
				// // int y = n;
				// // if (g != 0)
				// // {
				// //     n = n / 10;
				// //     y = (n + 1) * 10;
				// // }


				// //    string firstDate="",lastdate ="";

				// //    int weekOfYear =43;

				// //     DateTime jan1 = new DateTime(DateTime.Now.Year, 1, 1);
				// //     int daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
				// //     DateTime firstWeekDay = jan1.AddDays(daysOffset);
				// //     int firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
				// //     if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
				// //     {
				// //         weekOfYear -= 1;
				// //     }
				// //     return firstWeekDay.AddDays(weekOfYear * 7);

				// // for (int i = 0; i <= 3; i++)
				// // {
				// //     DateTime first = FirstDayOfWeek(DateTime.Now.AddDays(-7 * i));
				// //     DateTime last = LastDayOfWeek(DateTime.Now.AddDays(-7 * i));
				// // }




				// // DateTime date = DateTime.Now;
				// // DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
				// // int offset = fdow - date.DayOfWeek;
				// // DateTime fdowDate = date.AddDays(offset);



				// // DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);

				// // d = d.AddDays(43 * 7);
				// // _firebaseRepo.SendNotificationtoStaff(281, "Hello", "Clerk", DateTime.Now);
				// // dynamic data = new
				// // {
				// //     to = "dmF5gTV3ae3x-l39YEASvT:APA91bEzgBvYeEuTF9gwSu3EoR8LYy8RSL8jtok66eSQ2Qz3UEOrBREkEy4uLwtwpG8l-dF1O_yaQsmc07tVgAwQ50rgTVbGOJIcUg3FQNivOAcPIC1FK8wWVHeoLj6b3Y__-LcKpYXe",
				// //     notification = new
				// //     {
				// //         title = "Test",
				// //         body = "hello"
				// //     },
				// //     data = new
				// //     {
				// //         badge = 1//With new request, notification addition takes place later but badgecount should be incremented

				// //     },
				// //     priority = "high"
				// // };
				// // var json = JsonSerializer.Serialize(data);
				// // Byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);

				// // string SERVER_API_KEY = "AAAAWxdPntc:APA91bF-exKYzFF0-LicOg6utmBSGO4-MMgvON7Qh1EWiAzHvejCquyuOTWW8oQXNeyk8zhFn567LZdtZaTScGdeiLYpuky-Mg6T9tqIk0JeNFFzeuf4ESM4Q-5X02nf7wPIdfivJsoY";
				// // string SENDER_ID = "391233117911";

				// // WebRequest tRequest;
				// // tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
				// // tRequest.Method = "post";
				// // tRequest.ContentType = "application/json";
				// // tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));

				// // tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

				// // tRequest.ContentLength = byteArray.Length;
				// // Stream dataStream = tRequest.GetRequestStream();
				// // dataStream.Write(byteArray, 0, byteArray.Length);
				// // dataStream.Close();

				// // WebResponse tResponse = tRequest.GetResponse();

				// // dataStream = tResponse.GetResponseStream();

				// // StreamReader tReader = new StreamReader(dataStream);

				// // String sResponseFromServer = tReader.ReadToEnd();

				// // tReader.Close();
				// // dataStream.Close();
				// // tResponse.Close();

				// //     decimal rr1 = Math.Round(6.6666666667m, 2,MidpointRounding.AwayFromZero);
				// //    decimal rr= RoundOff(9.99);
				// //     string RandomGuid = Guid.NewGuid().ToString();
				// //     string test = RandomGuid.Substring(0, RandomGuid.LastIndexOf('-')) + DateTime.Now.ToString("yyyyMMddHHmmss");

				// //  _stripeRepo.ChargePayment(Token);

				// // List<string> tz = new List<string>();
				// // foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
				// // {
				// //     tz.Add(timeZone.Id);
				// // }
				// //  new { TZ = tz, Count = tz.Count };
				// //  response.Data = _numberPlateRecognitionRepo.GetNumberPlateFromImg(@"C:\Users\DEV-15\Desktop\check2.jpg");

				// //_squareRepo.ChargePayment(new SquareupModel { AccessToken = "EAAAEP3-IgG3EgUE5XhuUZtXfpSe9CmPtDtUOFNUXpr_abMByrRmlDC2EgRIjQG", SourceId = "cnon:card-nonce-ok" }, 10, new CustomerPaymentDetails());
				// //   string gg = _paymentRepo.ChargePaymentViaStripe(Token).Replace("\n", "");

				// // response.Data = _qRRepo.GetStaticTigerQRImage("Test");
				// //_masterRepo.GetOtp(Mobile);
				// #endregion

				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}

			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			return response;

		}

		public static DateTime FirstDayOfWeek(DateTime date)
		{
			DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
			int offset = fdow - date.DayOfWeek;
			DateTime fdowDate = date.AddDays(offset);
			return fdowDate;
		}

		public static DateTime LastDayOfWeek(DateTime date)
		{
			DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);
			return ldowDate;
		}



		[HttpGet("GetDamageVehicleDetails")]
		public Response GetDamageVehicleDetails(long DamageVehicleId)
		{
			Response response = new Response();
			try
			{
				if (DamageVehicleId.Equals(0))
					throw new AppException(_localizer["DamageIdRequired"]);
				response.Data = _staffRepo.GetDamageVehicleDetails(DamageVehicleId);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError(" GetDamageVehicleDetails- " + DamageVehicleId, ex.Message);
			}
			return response;
		}


		[HttpGet("GetDepositReport")]
		public Response GetDepositReport(string ParkingLocationId, string CurrentDate, string sortColumn, string sortOrder, int? pageNo, int? pageSize, string SearchValue)
		{
			Response response = new Response();
			try
			{

				if (string.IsNullOrEmpty(ParkingLocationId))
					throw new AppException("ParkingLocationIdRequired");
				response.Data = _parkingRepo.GetDepositReport(ParkingLocationId, CurrentDate, sortColumn, sortOrder, pageNo, pageSize, SearchValue);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}

			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

				_mailService.WDLogError("GetDepositReport", ex.Message);
			}
			return response;
		}

		[HttpPost("GetNumberPlateFromImg")]
		public Response GetNumberPlateFromImg()
		{
			Response response = new Response();
			try
			{
				string ImagePath = "";

				if (Request.Form.Files.Count > 0)
				{
					var file = Request.Form.Files[0];

					ImagePath = _masterRepo.ImageUpload(file);

					if (!string.IsNullOrEmpty(ImagePath))
					{
						var npresponse = _numberPlateRecognitionRepo.GetNumberPlateFromImg(ImagePath);

						if (npresponse is NumberPlateApiResponse && ((NumberPlateApiResponse)npresponse).results.Count > 0)
						{
							VehicleDetails vehicleResp = new VehicleDetails();
							NumberPlateApiResponse numberPlateApiResponse = (NumberPlateApiResponse)npresponse;

							var vehicleMaster = _cacheRepo.CachedVehicleMasterData();
							var RSvehicleDetails = numberPlateApiResponse.vehicles.FirstOrDefault();
							var Result = numberPlateApiResponse.results.FirstOrDefault();

							vehicleResp.NumberPlate = Result.plate;

							var Country = vehicleMaster.ListCountries.FirstOrDefault(a => a.CountryCode.ToLower().Equals(Result.region.Split('-')[0]));

							if (Country != null)
							{
								vehicleResp.CountryCode = Country.CountryCode;
								vehicleResp.VehicleCountry = Country.Name;
							}

							vehicleResp.StateCode = Result.region.Split('-')[1].ToString().ToUpper();

							if (RSvehicleDetails != null)
							{

								var Manufacturer = vehicleMaster.ListManufacturer.FirstOrDefault(a => a.Name.ToLower().Equals(RSvehicleDetails.details.make.FirstOrDefault().name));

								if (Manufacturer != null)
								{
									vehicleResp.VehicleManufacturer = Manufacturer.Name;
									vehicleResp.VehicleManufacturerId = Manufacturer.Id;
								}

								var Color = vehicleMaster.ListColor.FirstOrDefault(a => a.Name.ToLower().Equals(RSvehicleDetails.details.color.FirstOrDefault().name));

								if (Color != null)
								{
									vehicleResp.VehicleColor = Color.Name;
									vehicleResp.VehicleColorId = Color.Id;
								}

								vehicleResp.VehicleModal = RSvehicleDetails.details.make_model.FirstOrDefault().name;
							}


							response.Data = vehicleResp;
							response.Status = true;
							response.Message = _localizer["RequestSuccessful"];
						}
						else
						{
							if (npresponse is NumberPlateErrorResponse)
							{
								NumberPlateErrorResponse numberPlateerrResponse = (NumberPlateErrorResponse)npresponse;
								response.Message = numberPlateerrResponse.error;
							}
							else throw new AppException(_localizer["NPRecognitionError"]);

						}

					}
					else throw new AppException(_localizer["CouldntloadImg"]);
				}
				else throw new AppException(_localizer["CouldntloadImg"]);
			}

			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			return response;
		}

		[AllowAnonymous]
		[HttpPost("PostFrameResponse")]
		public async Task<Response> PostFrameResponse(CameraFrameResponse model)
		{
			_customerRepo.InsertIntoLogFile(JsonSerializer.Serialize(model));
			Response response = new Response();
			try
			{
				if (model.results != null && model.results.Count > 0)
				{
					var decInfo = _masterRepo.CheckBookingForDetectedVehicles((from item in model.results select new VehicleNumber { NumberPlate = item.plate }).ToList(), Convert.ToString(model.camera_id));

					if (decInfo.BookingStatus != null && decInfo.LocCameraInfo != null && decInfo.BookingStatus.Count > 0)
					{
						await Task.Run(() =>
								{
									string Title = string.Empty, Message = string.Empty;
									if (decInfo.LocCameraInfo.IsForEntry)
									{
										Title = "Vehicle Arrival";
										Message = string.Format(_localizer["VehicleArrivalMessage"], decInfo.LocCameraInfo.LocationName);
									}
									else
									{
										Title = "Vehicle Departure";
										Message = string.Format(_localizer["VehicleDepartureMessage"], decInfo.LocCameraInfo.LocationName);
									}

									_firebaseRepo.SendNotificationtoStaff(decInfo.LocCameraInfo.ParkingLocationId, Title, Message, decInfo.CurrentDate);
								});
					}
				}

				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;

			}
			return response;
		}

		[AllowAnonymous]
		[HttpGet("GetRecognizedVehicleList")]
		public Response GetRecognizedVehicleList(long ParkingLocationId, DateTime CurrentDate)
		{
			Response response = new Response();
			try
			{
				response.Data = _masterRepo.GetRecognizedVehicleList(ParkingLocationId, CurrentDate);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}

			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			return response;
		}


		[Authorize(Roles = "ParkingManager,Valet,Customer")]
		[HttpPost("StaffCustomerConversation")]
		public Response StaffCustomerConversation(StaffNotificationModel model)
		{
			Response response = new Response();
			try
			{

				NotificationDetails notificationDetails = _customerRepo.GetNotificationDetails(model);

				if (notificationDetails != null)
				{
					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(notificationDetails.TimeZone);
					model.NotificationDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

					string fcmresponse = "";
					if (!model.IsFromCustomer)
						fcmresponse = _firebaseRepo.SendNotificationtoCustomer(notificationDetails.DeviceTokens, notificationDetails.BadgeCount, _localizer["RequestVehicleTitle"] + " - " + notificationDetails.NumberPlate, model.NotificationMessage);
					else
						fcmresponse = _firebaseRepo.SendNotificationtoStaff(notificationDetails.ParkingLocationId, _localizer["RequestVehicleTitle"] + " - " + notificationDetails.NumberPlate, model.NotificationMessage, model.NotificationDateTime);

					// if (fcmresponse == null) throw new AppException(!model.IsFromCustomer ? _localizer["NotificationCustFailed"] : _localizer["NotificationFailed"]);
					// var FbResp = JsonSerializer.Deserialize<FirebaseResponse>(fcmresponse);

					// if (FbResp.success == 0) throw new AppException(_localizer["NotificationFailed"]);

					response.Data = new CommonId { Id = _customerRepo.StaffCustConversation(model, notificationDetails) };
					response.Message = _localizer["AcknowledgementSent"];
					response.Status = true;
				}
				else throw new AppException(_localizer["NoRequestInitiated"]);

			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("StaffCustomerConversation", ex.Message);
			}
			return response;
		}

		[Authorize(Roles = "ParkingManager,Valet,Customer")]
		[HttpGet("GetConversationList")]
		public Response GetConversationList(long NotificationId, bool IsFromCustomer)
		{
			Response response = new Response();
			try
			{
				response.Data = _masterRepo.GetConversationList(NotificationId, IsFromCustomer);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}

			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			return response;
		}

		[Authorize]
		[HttpGet("GetUnreadCount")]
		public Response GetUnreadCount(long UserId, long? ParkingLocationId, DateTime CurrentDate, bool IsFromValetApp)
		{
			Response response = new Response();
			try
			{
				response.Data = _masterRepo.GetUnreadCount(UserId, ParkingLocationId, CurrentDate, IsFromValetApp);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}

			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetUnreadCount - ", ex.Message);
			}
			return response;
		}

		[HttpGet("GetPOBusinessOfficeEmployeeById")]
		public Response GetPOBusinessOfficeEmployeeById(long BusinessOfficeEmployeeId)
		{
			Response response = new Response();
			try
			{
				if (BusinessOfficeEmployeeId.Equals(0))
					throw new AppException(_localizer["CustomerIdRequired"]);
				response.Data = _customerRepo.GetPOBusinessOfficeEmployeeById(BusinessOfficeEmployeeId);
				if (response.Data == null)
					throw new AppException(_localizer["RecordNotFound"]);
				response.Status = true;
				response.Message = _localizer["RequestSuccessful"];
			}
			catch (AppException ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
			}
			catch (Exception ex)
			{
				response.Data = null;
				response.Status = false;
				response.Message = ex.Message;
				_mailService.WDLogError("GetPOBusinessOfficeEmployeeById - " + BusinessOfficeEmployeeId, ex.Message);
			}
			return response;
		}
	}

}