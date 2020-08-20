using System;

namespace BookARoom.Domain.WriteModel
{
    public class BookingCanceled : IEvent
    {
        public string ClientId { get; } // immutable
        public Guid BookingId { get; }

        public BookingCanceled(string clientId, Guid bookingId)
        {
            this.ClientId = clientId;
            this.BookingId = bookingId;
        }
    }
}