using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models;
using System.Linq;
using System;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingBLL.Repository
{
    public class RegionRepo : IRegion
    {
        private readonly IConfiguration _configuration;
        public RegionRepo(
            IConfiguration configuration
            )
        {
            _configuration = configuration;
        }
        public void AddStateCodesForCountry(List<States> lstStates, string CountryCode)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddStateCodeForSpecificCountry");
            try
            {
                objCmd.Parameters.AddWithValue("@StateRef", MapDataTable.ToDataTable(lstStates));
                objCmd.Parameters.AddWithValue("@CountryCode", CountryCode);
                objSQL.UpdateDB(objCmd, true);
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

        public List<Countries> GetAllCountries()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllCountries");
            try
            {
                DataTable dtCountries = objSQL.FetchDT(objCmd);

                var Countries = (from DataRow dr in dtCountries.Rows
                                 select new Countries
                                 {
                                     Name = Convert.ToString(dr["Name"]),
                                     CountryCode = Convert.ToString(dr["CountryCode"])
                                 }).ToList();

                return Countries;
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

        public List<States> GetStatesForCountry(string CountryCode)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStatesByCountry");
            try
            {
                objCmd.Parameters.AddWithValue("@CountryCode", CountryCode);
                DataTable dtStates = objSQL.FetchDT(objCmd);

                var states = (from DataRow dr in dtStates.Rows
                              select new States
                              {
                                  StateName = Convert.ToString(dr["Name"]),
                                  StateCode = Convert.ToString(dr["StateCode"])
                              }).ToList();

                return states;
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

        public List<ValetParkingDAL.Models.TimeZone> GetTimeZones()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetTimeZone");
            try
            {
                DataTable dtZones = objSQL.FetchDT(objCmd);

                var timeZones = (from DataRow dr in dtZones.Rows
                                 select new ValetParkingDAL.Models.TimeZone
                                 {
                                     Name = Convert.ToString(dr["Name"]),
                                     TimeZoneId = Convert.ToString(dr["TimeZoneId"])
                                 }).ToList();

                return timeZones;
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