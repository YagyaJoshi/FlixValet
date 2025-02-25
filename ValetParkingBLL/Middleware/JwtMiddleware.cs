
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValetParkingDAL;
using ValetParkingDAL.Models;

namespace ValetParkingBLL.Middleware
{
    public class JwtMiddleware
    {
        private readonly IConfiguration _configuration;
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private SQLManager objSQL;
        private SqlCommand objCmd;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _next = next;
            _appSettings = appSettings.Value;
            _configuration = configuration;
        }

        // public async Task Invoke(HttpContext context, DataContext dataContext)
        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                attachAccountToContext(context, token);
            // await attachAccountToContext(context, dataContext, token);

            await _next(context);
        }

        // private async Task attachAccountToContext(HttpContext context, DataContext dataContext, string token)
        private void attachAccountToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                objSQL = new SQLManager(_configuration);
                objCmd = new SqlCommand("sp_GetUserbyId");
                objCmd.Parameters.AddWithValue("@Id", accountId);

               // DataTable dtUser = objSQL.FetchDT(objCmd);
                // var account = (from DataRow dr in dtUser.Rows
                //                select new User
                //                {
                //                    Id = Convert.ToInt32(dr["Id"]),
                //                    FirstName = dr["FirstName"].ToString(),
                //                    LastName = dr["LastName"].ToString(),
                //                    PasswordHash = Convert.ToString(dr["PasswordHash"]),
                //                    RoleId =Convert.ToInt32(dr["RoleId"]),
                //                    RoleName = Convert.ToString(dr["RoleName"]),
                //                    VerificationToken = dr["VerificationToken"].ToString(),
                //                    Email = dr["Email"].ToString(),
                //                    Verified = dr["Verified"] != System.DBNull.Value ? Convert.ToDateTime(dr["Verified"]) : (DateTime?)null,
                //                    ResetToken = dr["ResetToken"].ToString(),
                //                    ResetTokenExpires = dr["ResetTokenExpires"] != System.DBNull.Value ? Convert.ToDateTime(dr["ResetTokenExpires"]) : (DateTime?)null,
                //                    PasswordReset = dr["PasswordReset"] != System.DBNull.Value ? Convert.ToDateTime(dr["PasswordReset"]) : (DateTime?)null,
                //                    CreatedDate = Convert.ToDateTime(dr["Created"]),
                //                    UpdatedDate = dr["Updated"] != System.DBNull.Value ? Convert.ToDateTime(dr["Updated"]) : (DateTime?)null
                //                }).FirstOrDefault();
                // attach account to context on successful jwt validation
                context.Items["Account"] = new User();
            }
            catch (Exception)
            {
                // do nothing if jwt validation fails
                // account is not attached to context so request won't have access to secure routes
            }
        }
    }
}