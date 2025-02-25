namespace ValetParkingDAL.Models
{
    public class FirebaseResponse
    {
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public dynamic results { get; set; }

    }
}