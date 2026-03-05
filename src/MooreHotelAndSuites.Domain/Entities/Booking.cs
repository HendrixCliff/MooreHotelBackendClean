using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Domain.Events;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Abstractions;


namespace MooreHotelAndSuites.Domain.Entities
{
    public class Booking
    {
        private Booking() { }

        public Guid Id { get; private set; }
        public string Reference { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime CheckIn { get; private set; }
        public DateTime CheckOut { get; private set; }

        public Guid RoomId { get; private set; }
        public Room Room { get; private set; } = null!;

        public int GuestId { get; private set; }
        public Guest Guest { get; private set; } = null!;

        public string? UserAccountId { get; private set; } 

        public int Occupants { get; private set; }

        public BookingStatus Status { get; private set; } = BookingStatus.Pending;
       public string GroupReference { get; private set; } = string.Empty;
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
        public ICollection<Payment> Payments { get; private set; } = new List<Payment>();
       
        // --- DOMAIN LOGIC ---

        

        public void AssignGuest(int guestId)
        {
            if (guestId <= 0)
                throw new InvalidOperationException("GuestId must be valid");

            if (GuestId != 0)
                throw new InvalidOperationException("Guest already assigned");

            GuestId = guestId;
        }

        public void ValidateConfirmationInput(decimal amount, string inputGuestName)
        {
            if (Status != BookingStatus.Pending)
                throw new InvalidOperationException("Booking is not pending");

            if (amount <= 0)
                throw new InvalidOperationException("Invalid amount");
        }


public void MarkAsCheckedIn()
{
    if (Status != BookingStatus.Reserved)
        throw new InvalidOperationException("Only reserved bookings can be checked in");

    var today = DateTime.UtcNow.Date;
    
    // DEBUG: Check actual values
    Console.WriteLine($"Today (UTC): {today}");
    Console.WriteLine($"CheckIn.Date: {CheckIn.Date}");
    Console.WriteLine($"CheckOut.Date: {CheckOut.Date}");
    Console.WriteLine($"Comparison (today < CheckIn.Date): {today < CheckIn.Date}");
    
    if (today < CheckIn.Date)
        throw new InvalidOperationException("Cannot check in before scheduled check-in date");

    if (today >= CheckOut.Date)
        throw new InvalidOperationException("Cannot check in on or after check-out date");

    Status = BookingStatus.CheckedIn;

    AddDomainEvent(new BookingCheckedInDomainEvent(Id));
}
        public void MarkAsCheckedOut()
        {
            if (Status != BookingStatus.CheckedIn)
                throw new InvalidOperationException("Only checked-in bookings can be checked out");

            Status = BookingStatus.CheckedOut;

            AddDomainEvent(new BookingCheckedOutDomainEvent(Id));
        }


     public Payment AddPayment(
    decimal amount,
    string paymentMethod,
    string staffId,
    string guestFullName,
    string? staffRole = null)
{
    if (Status != BookingStatus.Pending)
        throw new InvalidOperationException("Only pending bookings can receive payment");

    if (amount <= 0)
        throw new InvalidOperationException("Payment amount must be greater than zero");

    var payment = new Payment(
        bookingId: Id,
        amount: amount,
        paymentMethod: paymentMethod,
        payeeName: guestFullName,
        staffId: staffId
    );

    Payments.Add(payment);

    
    Status = BookingStatus.Reserved;
      UpdatedAt = DateTime.UtcNow;

    
    AddDomainEvent(new PaymentConfirmedDomainEvent(Id));

    return payment;
}
public void ClearDomainEvents()
{
    _domainEvents.Clear();
}
public decimal CalculateAmount()
{
    if (Room == null)
        throw new InvalidOperationException("Room must be loaded to calculate amount.");

    var nights = (CheckOut.Date - CheckIn.Date).Days;

    if (nights <= 0)
        throw new InvalidOperationException("Invalid stay duration.");

    return Room.PricePerNight * nights;
}

        // --- FACTORY METHOD ---
       public static Booking Create(
    Guid roomId,
    int guestId,
    DateTime checkIn,
    DateTime checkOut,
    int occupants,
    string groupReference)
{
    var booking = new Booking
    {
        Id = Guid.NewGuid(),
        RoomId = roomId,
        GuestId = guestId,
        CheckIn = checkIn,
        CheckOut = checkOut,
        Occupants = occupants,
        GroupReference = groupReference,
        Status = BookingStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    booking.AddDomainEvent(
        new BookingCreatedDomainEvent(booking.Id, guestId));

    return booking;
}

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
