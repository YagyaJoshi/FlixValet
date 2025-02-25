using System;

namespace ValetParkingAPI.Models
{
    public class AppException : Exception
    {
        public AppException()
        { }

        public AppException(string message)
            : base(message)
        { }
    }
}