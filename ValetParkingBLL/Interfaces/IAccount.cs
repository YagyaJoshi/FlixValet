using System.Collections.Generic;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IAccount
    {
        LoginResponse Login(LoginRequest model);
        long Register(RegisterRequest model, string origin);
        bool VerifyEmail(string token, string origin);
        bool ForgotPassword(ForgotPasswordRequest model, string origin);
        bool ResetPassword(ResetPasswordRequest model, string origin);
        IEnumerable<AccountResponse> GetAll();
        bool SetPassword(ResetPasswordRequest model);
        AccountResponse GetUserByEmail(string Email);
        bool AddRole(Role model);
        List<Role> GetRoles();
        bool ChangePassword(ChangePasswordRequest model);
        CustomerLoginResponse CustomerLogin(LoginRequest model);

    }
}