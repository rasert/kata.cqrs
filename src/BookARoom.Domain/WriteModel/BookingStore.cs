using System;

namespace BookARoom.Domain.WriteModel
{
    public class BookingStore : IBookRooms
    {
        private readonly IHandleBookings handleBookings;
        private readonly IHandleClients handleClients;
        private readonly IPublishEvents publishEvents;

        public BookingStore(IHandleBookings handleBookings, IHandleClients handleClients,  IPublishEvents publishEvents)
        {
            this.handleBookings = handleBookings;
            this.handleClients = handleClients;
            this.publishEvents = publishEvents;
        }

        public void BookARoom(BookingCommand command)
        {
            if (!this.handleClients.IsClientAlready(command.ClientId))
            {
                this.handleClients.CreateClient(command.ClientId);    
            }

            Guid guid = Guid.NewGuid();
            var booking = new Booking(guid, command.ClientId, command.HotelId, command.RoomNumber, command.CheckInDate, command.CheckOutDate);
            this.handleBookings.Save(booking);

            // we could enrich the event from here (eg. finding the HotelName from the HotelId)
            var roomBooked = new RoomBooked(guid, command.HotelName, command.HotelId, command.ClientId, command.RoomNumber, command.CheckInDate, command.CheckOutDate);
            this.publishEvents.PublishTo(roomBooked);
        }

        public void CancelBooking(CancelBookingCommand command)
        {
            // TODO: fetch booking, check specified clientId, cancel and update persistence
            // TODO: instantiate and publish a new Booking Canceled event so that the Read Model has a chance to update itself
            // TODO: throw invalid operation exception if trying to cancel a booking for another client
            throw new NotImplementedException();
        }
    }
}
