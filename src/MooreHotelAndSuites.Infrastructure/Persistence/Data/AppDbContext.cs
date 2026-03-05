using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Infrastructure.Identity;

namespace MooreHotelAndSuites.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<RoomImage> RoomImages => Set<RoomImage>();
        public DbSet<RoomReview> RoomReviews => Set<RoomReview>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // RoomAmenity
            builder.Entity<RoomAmenity>()
                .HasKey(ra => new { ra.RoomId, ra.AmenityId });

            builder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Room)
                .WithMany(r => r.RoomAmenities)
                .HasForeignKey(ra => ra.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityId);

            // RoomImage
            builder.Entity<RoomImage>()
                .HasIndex(i => new { i.RoomId, i.DisplayOrder })
                .IsUnique();

            builder.Entity<RoomImage>()
                .HasOne(i => i.Room)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking - Store Status as INTEGER (not string)
            builder.Entity<Booking>(b =>
            {
                b.HasKey(x => x.Id);
                
                // Remove .HasConversion<string>() to store as int
                b.Property(x => x.Status)
                    .HasConversion<int>();  // Store as integer
                
                b.HasOne(bk => bk.Room)
                 .WithMany(r => r.Bookings)
                 .HasForeignKey(bk => bk.RoomId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment - Store Status as INTEGER
            builder.Entity<Payment>(p =>
            {
                p.HasKey(x => x.Id);
                
                p.Property(x => x.Amount)
                    .HasColumnType("decimal(18,2)");
                
                // Store as integer, not string
                p.Property(x => x.Status)
                    .HasConversion<int>();
                
                p.HasOne(x => x.Booking)
                 .WithMany(b => b.Payments)
                 .HasForeignKey(x => x.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ServiceOrder
            builder.Entity<ServiceOrder>(e =>
            {
                e.HasKey(x => x.Id);
                
                e.OwnsMany(x => x.Items, b =>
                {
                    b.WithOwner().HasForeignKey("OrderId");
                    b.Property<Guid>("Id");
                    b.HasKey("Id");
                });
            });

            // MenuItem
            builder.Entity<MenuItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();
            });

            // RoomReview
            builder.Entity<RoomReview>()
                .HasOne(r => r.Room)
                .WithMany(room => room.Reviews)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}