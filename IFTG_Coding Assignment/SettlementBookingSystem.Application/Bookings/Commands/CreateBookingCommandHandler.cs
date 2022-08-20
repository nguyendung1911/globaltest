using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SettlementBookingSystem.Application.Bookings.Dtos;
using SettlementBookingSystem.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SettlementBookingSystem.Application.Bookings.Commands
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
    {
        private readonly IMemoryCache _memoryCache;
        public CreateBookingCommandHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            if (!DateTime.TryParse(request.BookingTime, out DateTime bookingTime))
            {
                throw new Exception(); 
            }

            var hour = bookingTime.Hour;
            if (hour < 9 || hour > 16)
                throw new ValidationException("invalid time");

            var currentBookings = _memoryCache.Get<List<string>>("booking") ?? new List<string>();

            if (currentBookings != null && currentBookings.Any())
            {
                foreach (var booking in currentBookings)
                {
                    var startBooking = DateTime.Parse(booking);
                    var endBooking = startBooking.AddMinutes(59);

                    if (bookingTime >= startBooking && bookingTime <= endBooking)
                        throw new ConflictException("Conflict Booking");
                }
            }

            currentBookings.Add(bookingTime.TimeOfDay.ToString());
            _memoryCache.Set("booking", currentBookings);

            return Task.FromResult(new BookingDto());
        }
    }
}
