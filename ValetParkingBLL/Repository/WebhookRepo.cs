using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using AutoMapper;
using ValetParkingBLL.Helpers;
using ValetParkingDAL.Models;
using Microsoft.Extensions.Configuration;
using Stripe;
using Newtonsoft.Json.Linq;
using ThirdParty.Json.LitJson;

namespace ValetParkingBLL.Repository
{
    public class WebhookRepo : IWebhook
    {
        private readonly IConfiguration _configuration;

        public WebhookRepo(IConfiguration configuration)
        {
            _configuration = configuration;
           
        }
        public bool UpdateCustomerAmount(string customerId, decimal amount)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateCustomerNetAmount");


            if (amount > 0)
            {
                // Convert from cents to main currency unit (e.g., dollars)
                amount = amount / 100;
            }

            try
            {
                objCmd.Parameters.AddWithValue("@CustomerId", customerId);
                objCmd.Parameters.AddWithValue("@Amount", amount);
                DataTable dtUser = objSQL.FetchDT(objCmd);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
    }
}
