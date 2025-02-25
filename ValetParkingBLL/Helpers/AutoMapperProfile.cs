using AutoMapper;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Helpers
{
    public class AutoMapperProfile : Profile
    {
        // mappings between model and entity objects
        public AutoMapperProfile()
        {
            CreateMap<User, AccountResponse>();

            CreateMap<User, LoginResponse>();

            CreateMap<RegisterRequest, User>();

            CreateMap<AdditionalPaymentRequest, BookingDetailResponse>();
        }
    }
}