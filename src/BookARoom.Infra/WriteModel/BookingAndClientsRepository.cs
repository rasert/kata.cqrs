using System;
using System.Collections.Generic;
using System.Linq;
using BookARoom.Domain.WriteModel;

namespace BookARoom.Infra.WriteModel
{
    public class BookingAndClientsRepository : IHandleBookings, IHandleClients
    {
        private readonly Dictionary<string, List<Booking>> perClientBookings;

        public BookingAndClientsRepository()
        {
            perClientBookings = new Dictionary<string, List<Booking>>();
        }

        public void Save(Booking booking)
        {
            perClientBookings[booking.ClientId].Add(booking);
        }

        public Booking GetBooking(string clientId, Guid bookingId)
        {
            return perClientBookings[clientId].SingleOrDefault(b =>
                b.BookingId.Equals(bookingId)) ?? Booking.Null;
        }

        public void Update(Booking booking)
        {
            if (!this.perClientBookings.ContainsKey(booking.ClientId))
            {
                this.perClientBookings[booking.ClientId] = new List<Booking>();
            }

            var bookingsForThisClient = this.perClientBookings[booking.ClientId];

            int? index = null;
            for (int i = 0; i < bookingsForThisClient.Count; i++)
            {
                if (bookingsForThisClient[i].BookingId == booking.BookingId)
                {
                    index = i;
                    break;
                }
            }

            if (index.HasValue)
            {
                bookingsForThisClient[index.Value] = booking;
            }
        }

        public bool IsClientAlready(string clientIdentifier)
        {
            return perClientBookings.ContainsKey(clientIdentifier);
        }

        public void CreateClient(string clientIdentifier)
        {
            if (!perClientBookings.ContainsKey(clientIdentifier))
            { 
                perClientBookings[clientIdentifier] = new List<Booking>();
            }
        }

        public IEnumerable<Booking> GetBookingsFrom(string clientIdentifier)
        {
            if (perClientBookings.ContainsKey(clientIdentifier))
            { 
                return perClientBookings[clientIdentifier];
            }

            return new List<Booking>();
        }
    }
}