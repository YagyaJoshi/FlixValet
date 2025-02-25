namespace ValetParkingDAL.Models.StateModels
{
    public class StatesMst
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string StateCode { get; set; }
        public long CountryId { get; set; }

        public string CountryCode { get; set; }

    }
}