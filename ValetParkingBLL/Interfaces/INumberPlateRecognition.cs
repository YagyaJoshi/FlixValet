namespace ValetParkingBLL.Interfaces
{
    public interface INumberPlateRecognition
    {
        public dynamic GetNumberPlateFromImg(string ImagePath);
    }
}