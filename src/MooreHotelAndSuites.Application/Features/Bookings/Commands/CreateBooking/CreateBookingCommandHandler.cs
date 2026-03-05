using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Application.Interfaces.Identity;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, List<BookingDto>>
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IGuestService _guestService;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepo,
        IRoomRepository roomRepo,
        IGuestService guestService)
    {
        _bookingRepo = bookingRepo;
        _roomRepo = roomRepo;
        _guestService = guestService;
    }

    public async Task<List<BookingDto>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.RoomIds == null || !dto.RoomIds.Any())
            throw new InvalidOperationException("At least one room must be selected.");

        if (dto.CheckOutDate <= dto.CheckInDate)
            throw new InvalidOperationException("Invalid stay duration.");

        if (string.IsNullOrWhiteSpace(dto.GuestFullName) ||
            string.IsNullOrWhiteSpace(dto.GuestPhoneNumber))
            throw new InvalidOperationException("Guest details are required.");

        
        var groupReference = GenerateGroupReference();

       
        int guestId = await _guestService.EnsureGuestAsync(
            dto.GuestFullName!,
            dto.GuestEmail ?? string.Empty,
            dto.GuestPhoneNumber!
        );

        var bookings = new List<Booking>();

        for (int i = 0; i < dto.RoomIds.Count; i++)
        {
            var roomId = dto.RoomIds[i];
            var room = await _roomRepo.GetByIdAsync(roomId);

            int occupants = dto.OccupantsPerRoom.Count > i
                ? dto.OccupantsPerRoom[i]
                : dto.Occupants;

            var booking = Booking.Create(
                roomId,
                guestId,
                dto.CheckInDate,
                dto.CheckOutDate,
                occupants,
                groupReference
            );

            await _bookingRepo.AddAsync(booking);
            bookings.Add(booking);
        }

        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            CheckIn = b.CheckIn,
            CheckOut = b.CheckOut,
            RoomId = b.RoomId,
            GuestId = b.GuestId,
            Occupants = b.Occupants,
            Status = b.Status,
            GroupReference = b.GroupReference,
            Amount = b.CalculateAmount()  
        }).ToList();
    }

    private static string GenerateGroupReference()
    {
        return $"GRP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";
    }
}