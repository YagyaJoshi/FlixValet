namespace ValetParkingDAL.Models
{
    public class AppSettings
    {
        public string ApiDomain { get; set; }
        public string AppName { get; set; }
        public string Secret { get; set; }

        // refresh token time to live (in days), inactive tokens are
        // automatically deleted from the database after this time
        public int RefreshTokenTTL { get; set; }

        public string EmailFrom { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public string StripeBaseUrl { get; set; }
        public string SquareupBaseUrl { get; set; }
        public string SquareupProductionUrl { get; set; }
        public bool IsSandbox { get; set; }

        public string LogoUrl { get; set; }

        public string LoginUrl { get; set; }

    }
}