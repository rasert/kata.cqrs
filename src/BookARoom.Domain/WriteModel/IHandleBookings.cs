using System;

namespace BookARoom.Domain.WriteModel
{
    public interface IHandleBookings
    {
        void Save(Booking booking);
        Booking GetBooking(string clientId, Guid bookingId);
        void Update(Booking booking);
    }
}