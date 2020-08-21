﻿namespace BookARoom.Domain.WriteModel
{
    public class WriteModelFacade : IHandleCommand<BookingCommand>, IHandleCommand<CancelBookingCommand>
    {
        public IBookRooms BookingStore { get; }

        public WriteModelFacade(IBookRooms bookingStore)
        {
            this.BookingStore = bookingStore;
        }

        public void Handle(BookingCommand command)
        {
            this.BookingStore.BookARoom(command);
        }

        public void Handle(CancelBookingCommand command)
        {
            // handle Cancel Booking commands
            this.BookingStore.CancelBooking(command);
        }
    }
}