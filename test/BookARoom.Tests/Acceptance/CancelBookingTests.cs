using System;
using System.Linq;
using BookARoom.Domain;
using BookARoom.Domain.ReadModel;
using BookARoom.Domain.WriteModel;
using BookARoom.Infra;
using BookARoom.Infra.MessageBus;
using BookARoom.Infra.ReadModel.Adapters;
using BookARoom.Infra.WriteModel;
using NFluent;
using NUnit.Framework;

namespace BookARoom.Tests.Acceptance
{
    [TestFixture]
    public class CancelBookingTests
    {
        [Test]
        public void Should_Update_booking_engine_when_CancelBookingCommand_is_sent()
        {
            var bookingEngine = new BookingAndClientsRepository();
            var bus = new FakeBus();
            CompositionRootHelper.BuildTheWriteModelHexagon(bookingEngine, bookingEngine, bus, bus);

            var hotelId = 2;
            var roomNumber = "101";
            var clientId = "thomas@pierrain.net";
            var bookingCommand = new BookingCommand(clientId: clientId, hotelName: "New York Sofitel", hotelId: hotelId, roomNumber: roomNumber, checkInDate: Constants.MyFavoriteSaturdayIn2017, checkOutDate: Constants.MyFavoriteSaturdayIn2017.AddDays(1));

            bus.Send(bookingCommand);

            Check.That(bookingEngine.GetBookingsFrom(clientId)).HasSize(1);
            var bookingGuid = bookingEngine.GetBookingsFrom(clientId).First().BookingId;

            var cancelBookingCommand = new CancelBookingCommand(bookingGuid, clientId);
            bus.Send(cancelBookingCommand);

            // Booking is still there, but canceled
            Check.That(bookingEngine.GetBookingsFrom(clientId)).HasSize(1);
            Check.That(bookingEngine.GetBookingsFrom(clientId).First().IsCanceled).IsTrue();
        }
        
        [Test]
        public void Should_Update_readmodel_user_reservations_when_CancelBookingCommand_is_sent()
        {
            var bookingEngine = new BookingAndClientsRepository();
            var bus = new FakeBus(synchronousPublication:true);
            CompositionRootHelper.BuildTheWriteModelHexagon(bookingEngine, bookingEngine, bus, bus);

            var hotelsAndRoomsAdapter = new HotelsAndRoomsAdapter(Constants.RelativePathForHotelIntegrationFiles, bus);
            hotelsAndRoomsAdapter.LoadAllHotelsFiles();
            var reservationAdapter = new ReservationAdapter(bus);
            CompositionRootHelper.BuildTheReadModelHexagon(hotelsAndRoomsAdapter, hotelsAndRoomsAdapter, reservationAdapter, bus);

            var clientId = "thomas@pierrain.net";
            Check.That(reservationAdapter.GetReservationsFor(clientId)).IsEmpty();

            var hotelId = 2;
            var roomNumber = "101";
            var bookingCommand = new BookingCommand(clientId: clientId, hotelName: "New York Sofitel", hotelId: hotelId, roomNumber: roomNumber, checkInDate: Constants.MyFavoriteSaturdayIn2017, checkOutDate: Constants.MyFavoriteSaturdayIn2017.AddDays(1));

            bus.Send(bookingCommand);

            var bookingGuid = bookingEngine.GetBookingsFrom(clientId).First().BookingId;

            Check.That(reservationAdapter.GetReservationsFor(clientId)).HasSize(1);

            var reservation = reservationAdapter.GetReservationsFor(clientId).First();
            Check.That(reservation.RoomNumber).IsEqualTo(roomNumber);
            Check.That(reservation.HotelId).IsEqualTo(hotelId.ToString());

            var cancelCommand = new CancelBookingCommand(bookingGuid, clientId);
            bus.Send(cancelCommand);

            Check.That(reservationAdapter.GetReservationsFor(clientId)).HasSize(0);
        }
        
        [Test]
        public void Should_impact_room_search_results_when_CancelBookingCommand_is_sent()
        {
            // Initialize Read-model side
            var bus = new FakeBus(synchronousPublication: true);
            var hotelsAdapter = new HotelsAndRoomsAdapter(Constants.RelativePathForHotelIntegrationFiles, bus);
            var reservationsAdapter = new ReservationAdapter(bus);
            hotelsAdapter.LoadHotelFile("New York Sofitel-availabilities.json");

            // Initialize Write-model side
            var bookingRepository = new BookingAndClientsRepository();
            CompositionRootHelper.BuildTheWriteModelHexagon(bookingRepository, bookingRepository, bus, bus);

            var readFacade = CompositionRootHelper.BuildTheReadModelHexagon(hotelsAdapter, hotelsAdapter, reservationsAdapter, bus);

            // Search Rooms availabilities
            var checkInDate = Constants.MyFavoriteSaturdayIn2017;
            var checkOutDate = checkInDate.AddDays(1);

            var searchQuery = new SearchBookingOptions(checkInDate, checkOutDate, location: "New York", numberOfAdults: 2);
            var bookingOptions = readFacade.SearchBookingOptions(searchQuery);

            // We should get 1 booking option with 13 available rooms in it.
            Check.That(bookingOptions).HasSize(1);

            var bookingOption = bookingOptions.First();
            var initialRoomsNumbers = 13;
            Check.That(bookingOption.AvailableRoomsWithPrices).HasSize(initialRoomsNumbers);

            // Now, let's book that room!
            var firstRoomOfThisBookingOption = bookingOption.AvailableRoomsWithPrices.First();
            var clientId = "thomas@pierrain.net";
            var bookingCommand = new BookingCommand(clientId: clientId, hotelName: "New York Sofitel", hotelId: bookingOption.Hotel.Identifier, roomNumber: firstRoomOfThisBookingOption.RoomIdentifier, checkInDate: checkInDate, checkOutDate: checkOutDate);

            // We send the BookARoom command
            bus.Send(bookingCommand);

            // We check that both the BookingRepository (Write model) and the available rooms (Read model) have been updated.
            Check.That(bookingRepository.GetBookingsFrom(clientId).Count()).IsEqualTo(1);
            var bookingId = bookingRepository.GetBookingsFrom(clientId).First().BookingId;

            // Fetch rooms availabilities now. One room should have disappeared from the search result
            bookingOptions = readFacade.SearchBookingOptions(searchQuery);
            Check.That(bookingOptions).HasSize(1);
            Check.That(bookingOption.AvailableRoomsWithPrices).As("available matching rooms").HasSize(initialRoomsNumbers - 1);

            // We cancel our booking
            var cancelBookingCommand = new CancelBookingCommand(bookingId, clientId);
            bus.Send(cancelBookingCommand);

            // Search again and the missing room should be back on the search result again
            bookingOptions = readFacade.SearchBookingOptions(searchQuery);
            Check.That(bookingOptions).HasSize(1);
            Check.That(bookingOption.AvailableRoomsWithPrices).As("available matching rooms").HasSize(initialRoomsNumbers - 1 + 1);
        }
    }
}