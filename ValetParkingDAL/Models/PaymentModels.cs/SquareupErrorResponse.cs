using System.Collections.Generic;

namespace ValetParkingDAL.Models.PaymentModels.cs
{
    public class SquareupErrorResponse
    {
        public List<SquareupError> errors { get; set; }

    }
    public class SquareupError
    {
        public string code { get; set; }
        public string detail { get; set; }
        public string category { get; set; }
    }



}