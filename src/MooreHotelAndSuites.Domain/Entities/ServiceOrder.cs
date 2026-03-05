using MooreHotelAndSuites.Domain.Abstractions;
using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Domain.Events;

namespace MooreHotelAndSuites.Domain.Entities
{
    public class ServiceOrder
    {
        public Guid Id { get; private set; }
        public Guid? BookingId { get; private set; }
        public int? GuestId { get; private set; }
        public Guid? RoomId { get; private set; }
        public string? RoomNumber { get; private set; }
        public string CustomerName { get; private set; } = null!;
        public string PhoneNumber { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public OrderSource Source { get; private set; }
        public OrderStatus Status { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();
        public Guest? Guest { get; private set; }
        public decimal TotalAmount { get; private set; }
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        private ServiceOrder() { }

        // ===== FACTORIES =====

        public static ServiceOrder CreateForHotelGuest(
            Booking booking, string name, string phone, OrderSource source)
        {
            if (source is OrderSource.Kitchen or OrderSource.Bar)
            {
                if (booking.Status != BookingStatus.CheckedIn &&
                    booking.Status != BookingStatus.Reserved)
                    throw new InvalidOperationException(
                        "Only checked-in or reserved guests can order");
            }

            var order = new ServiceOrder
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                GuestId = booking.GuestId,
                CustomerName = name,
                PhoneNumber = phone,
                Source = source,
                Status = OrderStatus.PendingPayment,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber,
                TotalAmount = 0  
            };

            order.AddDomainEvent(new OrderCreatedEvent(order.Id, source, name));
            return order;
        }

        public static ServiceOrder CreateForEventWalkIn(string name, string phone)
        {
            var order = new ServiceOrder
            {
                Id = Guid.NewGuid(),
                CustomerName = name,
                PhoneNumber = phone,
                Source = OrderSource.EventHall,
                Status = OrderStatus.PendingPayment,
                TotalAmount = 0  
            };

            order.AddDomainEvent(new OrderCreatedEvent(order.Id, OrderSource.EventHall, name));
            return order;
        }

        public static ServiceOrder CreateRoomService(Booking booking, string name, string phone)
        {
            if (booking.Status != BookingStatus.CheckedIn)
                throw new InvalidOperationException(
                    "Only checked-in guests can use room service");

            var order = new ServiceOrder
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                GuestId = booking.GuestId,
                CustomerName = name,
                PhoneNumber = phone,
                Source = OrderSource.RoomService,
                Status = OrderStatus.PendingPayment,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber,
                TotalAmount = 0  
            };

            order.AddDomainEvent(new OrderCreatedEvent(order.Id, OrderSource.RoomService, name));
            return order;
        }

        public static ServiceOrder CreateLaundry(Booking booking, string name, string phone)
        {
            if (booking.Status != BookingStatus.CheckedIn &&
                booking.Status != BookingStatus.Reserved)
                throw new InvalidOperationException(
                    "Only checked-in or reserved guests can use laundry");

            var order = new ServiceOrder
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                GuestId = booking.GuestId,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber,
                CustomerName = name,
                PhoneNumber = phone,
                Source = OrderSource.Laundry,
                Status = OrderStatus.PendingPayment,
                TotalAmount = 0 
            };

            order.AddDomainEvent(new OrderCreatedEvent(order.Id, OrderSource.Laundry, name));
            return order;
        }

        // ===== BEHAVIOR =====

        public void AddLaundryItem(LaundryServiceType type, int quantity, string description)
        {
            var item = OrderItem.FromLaundry(type, quantity, description);
            Items.Add(item);
            UpdateTotalAmount(); 
             Console.WriteLine($"Laundry item added. TotalAmount: {TotalAmount}"); 
        }

        public void AddItem(MenuItem menu, int quantity, string? roomNumber = null)
    {
        if (quantity <= 0) 
            throw new InvalidOperationException("Quantity must be greater than zero");
        
        var item = new OrderItem(menu, quantity, roomNumber);
        Items.Add(item);
        UpdateTotalAmount();
    }

      
        private void UpdateTotalAmount()
        {
            TotalAmount = Items.Sum(x => x.Total);
        }

       public void ConfirmPayment(int? guestId = null)
{
    if (Status != OrderStatus.PendingPayment)
        throw new InvalidOperationException("Order is not awaiting payment");

    if (guestId.HasValue)
        GuestId = guestId.Value;

    Status = OrderStatus.Confirmed;

    AddDomainEvent(new OrderPaymentConfirmedEvent(Id, Source));
}

        public void MarkServed()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Payment not confirmed");
            
            Status = OrderStatus.Served;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkCreated()
        {
            AddDomainEvent(new OrderCreatedEvent(Id, Source, CustomerName));
        }

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}