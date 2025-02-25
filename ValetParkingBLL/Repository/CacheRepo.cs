using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using TimeZone = ValetParkingDAL.Models.TimeZone;

namespace ValetParkingBLL.Repository
{
    public class CacheRepo : ICache
    {
        private readonly IRegion _regionRepo;
        private readonly IMaster _masterRepo;
        private readonly IMemoryCache _cache;

        public CacheRepo(IRegion regionRepo, IMaster masterRepo, IMemoryCache cache)
        {

            _regionRepo = regionRepo;
            _masterRepo = masterRepo;
            _cache = cache;
        }

        public List<TimeZone> CachedTimeZones()
        {
            List<TimeZone> timeZones = new List<TimeZone>();
            if (!_cache.TryGetValue(CacheKeys.TimeZone, out timeZones))
            {
                timeZones = _regionRepo.GetTimeZones();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(365));
                _cache.Set(CacheKeys.TimeZone, timeZones, cacheEntryOptions);
            }
            return timeZones;
        }

        public VehicleMasterResponse CachedVehicleMasterData()
        {
            VehicleMasterResponse vehicleMasterResponse = new VehicleMasterResponse();

            if (!_cache.TryGetValue(CacheKeys.VehicleMasterResponse, out vehicleMasterResponse))
            {
                vehicleMasterResponse = _masterRepo.GetVehicleMasterData();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(365));
                _cache.Set(CacheKeys.VehicleMasterResponse, vehicleMasterResponse, cacheEntryOptions);
            }

            return vehicleMasterResponse;
        }
    }
}