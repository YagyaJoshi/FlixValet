namespace ValetParkingBLL.Interfaces
{
    public interface ISMS
    {
        public void SendSMS(string Msg, string To, bool IsFromEnter = false);
    }
}