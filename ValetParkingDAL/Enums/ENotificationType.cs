namespace ValetParkingDAL.Enums
{
    public enum ENotificationType
    {
        RequestVehicle = 1,
        BookingReminder = 2,
        MonthlyRenewal = 3,
        RequestAcknowledgement = 4,
        QRBookingConfirmation = 5,

        /*
        different from QRBookingConfirmation, in this case notification is sent to customer only while in QRBookingConfirmation notification is sent to both valet and customer.
        */
        CustomerBookingConfirmation = 6,
        CustomerQRBookingConfirmation = 7,
        CustomerBookingReminder = 8

    }
}