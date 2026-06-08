using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Application.Interfaces.Events;
using MooreHotelAndSuites.Application.DTOs.Order;
using MooreHotelAndSuites.Application.DTOs.Laundry;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Domain.Events;

namespace MooreHotelAndSuites.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly IGuestRepository _guestRepo;
        private readonly IDomainEventDispatcher _dispatcher;

        public OrderService(
            IOrderRepository orderRepo,
            IBookingRepository bookingRepo,
            IMenuRepository menuRepo,
            IGuestRepository guestRepo,
            IDomainEventDispatcher dispatcher)
        {
            _orderRepo = orderRepo;
            _bookingRepo = bookingRepo;
            _menuRepo = menuRepo;
            _guestRepo = guestRepo;
            _dispatcher = dispatcher;
        }
public async Task<(Guid id, decimal amount)> CreateOrderAsync(CreateOrderDto dto, string? userId = null)
{
    ServiceOrder order;
    string customerName;
    string phoneNumber;

   

    // Handle event hall walk-in (no booking required)
    if (dto.Source == OrderSource.EventHall)
    {
        if (string.IsNullOrEmpty(dto.CustomerName) || string.IsNullOrEmpty(dto.PhoneNumber))
            throw new Exception("Event hall orders require customer name and phone number");

        order = ServiceOrder.CreateForEventWalkIn(dto.CustomerName, dto.PhoneNumber);
        customerName = dto.CustomerName;
        phoneNumber = dto.PhoneNumber;
    }
    else
    {
        // For hotel services - find active booking
        if (dto.RoomItems.Count == 1 && string.IsNullOrEmpty(dto.RoomItems[0].RoomNumber))
        {
            // Single room - find by guest name/phone (we know which room)
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
    throw new Exception("Customer name is required");

if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
    throw new Exception("Phone number is required");

var booking = await _bookingRepo.GetActiveByGuestAsync(
    dto.CustomerName,
    dto.PhoneNumber);
            if (booking == null)
                throw new Exception("No active booking found for this guest");

            var guest = await _guestRepo.GetByIdAsync(booking.GuestId);
            customerName = guest?.FullName ?? dto.CustomerName;
            phoneNumber = guest?.PhoneNumber ?? dto.PhoneNumber;

            order = ServiceOrder.CreateForHotelGuest(booking, customerName, phoneNumber, dto.Source);
        }
        else
        {
            // Multiple rooms - validate each room belongs to the guest
           if (string.IsNullOrWhiteSpace(dto.CustomerName))
            throw new Exception("Customer name is required");

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            throw new Exception("Phone number is required");

        var guest = await _guestRepo.GetByFullNameAndPhoneAsync(
            dto.CustomerName,
            dto.PhoneNumber);

            if (guest == null)
                throw new Exception("Guest not found");

            customerName = guest.FullName;
            phoneNumber = guest.PhoneNumber;

            // Validate each room belongs to the guest
            foreach (var roomItem in dto.RoomItems)
            {
                if (string.IsNullOrEmpty(roomItem.RoomNumber))
                    throw new Exception("Room number is required for multi-room orders");

                var booking = await _bookingRepo.GetActiveByRoomNumberAsync(roomItem.RoomNumber);
                if (booking == null)
                    throw new Exception($"No active booking found for room {roomItem.RoomNumber}");

                if (booking.GuestId != guest.Id)
                    throw new Exception($"Room {roomItem.RoomNumber} does not belong to guest {customerName}");
            }

            // Create order for the first room (or main booking)
           var firstBooking =
    await _bookingRepo.GetActiveByRoomNumberAsync(
        dto.RoomItems[0].RoomNumber!);

        if (firstBooking == null)
        {
            throw new Exception(
                $"No active booking found for room {dto.RoomItems[0].RoomNumber}");
        }

        order = ServiceOrder.CreateForHotelGuest(
            firstBooking,
            customerName,
            phoneNumber,
            dto.Source);
                }
    }

    // Add items for each room - Pass RoomNumber to OrderItem
    foreach (var roomItem in dto.RoomItems)
    {
        foreach (var item in roomItem.Items)
        {
            var menu = await _menuRepo.GetByIdAsync(item.MenuItemId);
            if (menu == null)
                throw new Exception($"Menu item not found");

            // Pass the RoomNumber when creating the OrderItem
            order.AddItem(menu, item.Quantity, roomItem.RoomNumber);
        }
    }

    await _orderRepo.AddAsync(order);
    await _orderRepo.SaveChangesAsync();

    await _dispatcher.DispatchAsync(new[]
    {
        new OrderCreatedEvent(order.Id, dto.Source, customerName)
    });

    return (order.Id, order.TotalAmount);
}

        public async Task<(Guid id, decimal amount)> CreateLaundryOrderAsync(CreateLaundryOrderDto dto, string? userId = null)
        {
            // Find active booking by guest name/phone
            var booking = await _bookingRepo.GetActiveByGuestAsync(dto.CustomerName, dto.PhoneNumber);

            if (booking == null)
                throw new Exception("Only checked-in guests can use laundry");

            var order = ServiceOrder.CreateLaundry(booking, dto.CustomerName, dto.PhoneNumber);

            foreach (var item in dto.Items)
            {
                order.AddLaundryItem(item.Type, item.Quantity, item.Description);
            }

            await _orderRepo.AddAsync(order);

            await _dispatcher.DispatchAsync(new[]
            {
                new OrderCreatedEvent(order.Id, OrderSource.Laundry, dto.CustomerName)
            });

            return (order.Id, order.TotalAmount);
        }

   public async Task ConfirmPaymentAsync(ConfirmOrderPaymentDto dto)
{
    var order = await _orderRepo.GetPendingByCustomerAsync(
        dto.CustomerName,
        dto.PhoneNumber,
        dto.Amount);

    if (order == null)
        throw new Exception("Pending order not found. Check name, phone, and amount.");

   
    if (!string.IsNullOrEmpty(dto.RoomNumber) && order.RoomNumber != dto.RoomNumber)
    {
        throw new Exception("Room number does not match order");
    }

    
    if (order.Source != dto.Source)
    {
        throw new Exception("Order source does not match");
    }

    order.ConfirmPayment();
    await _orderRepo.SaveChangesAsync();  
    await _dispatcher.DispatchAsync(order.DomainEvents);
}

        public async Task MarkServedAsync(ServeOrderDto dto)
        {
            var order = await _orderRepo.GetActiveForServingAsync(
                dto.CustomerName,
                dto.PhoneNumber);

            if (order == null)
                throw new Exception("No confirmed order found");

            order.MarkServed();
        }
    }
}