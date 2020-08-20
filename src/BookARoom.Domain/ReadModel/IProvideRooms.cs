using System;
using System.Collections.Generic;

namespace BookARoom.Domain.ReadModel
{
    /// <summary>
    /// Find rooms.
    /// <remarks>Repository of AvailableRoomsWithPrices.</remarks>
    /// </summary>
    public interface IProvideRooms
    {
        IEnumerable<BookingOption> SearchAvailableHotelsInACaseInsensitiveWay(string location, DateTime checkInDate, DateTime checkOutDate);
    }
}