namespace ValetParkingDAL.Models.CustomerModels
{


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class FNotification
    {
        public string title { get; set; }
        public string body { get; set; }
        public string sound { get; set; }
    }

    public class FData
    {
        public string badge { get; set; }
        public string vehicle_number { get; set; }
    }

    public class FirebaseCustomerRequest
    {
        public string to { get; set; }
        public FNotification notification { get; set; }
        public FData data { get; set; }
        public string priority { get; set; }
        // public bool contentAvailable {get;set;}
    }




}