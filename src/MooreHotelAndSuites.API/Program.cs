using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.API.Hubs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MooreHotelAndSuites.API.Mappings;
using MooreHotelAndSuites.Infrastructure.Services;
using MooreHotelAndSuites.Application.Notifications;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Application.Interfaces.Auditing;
using MooreHotelAndSuites.Application.Services;
using MooreHotelAndSuites.Application.EventHandlers;
using MooreHotelAndSuites.Infrastructure.Data;
using MooreHotelAndSuites.Infrastructure.Identity;
using MooreHotelAndSuites.Infrastructure.Persistence.Repositories;
using MooreHotelAndSuites.Infrastructure.Auth;
using MooreHotelAndSuites.Infrastructure.Auditing;
using MooreHotelAndSuites.Domain.Abstractions;
using MooreHotelAndSuites.Domain.Events;
using MooreHotelAndSuites.Application.Interfaces.Events;
using MooreHotelAndSuites.Infrastructure.Events;
using MooreHotelAndSuites.API.Middleware;
using MooreHotelAndSuites.Application.EventHandlers.Orders;
using Microsoft.AspNetCore.SignalR;
using MooreHotelAndSuites.Infrastructure.Notifications;
using MooreHotelAndSuites.API.Realtime;
using MooreHotelAndSuites.Application.DTOs.Payments;
using MooreHotelAndSuites.Application.Interfaces.Identity;
using MooreHotelAndSuites.Application.Interfaces.Realtime;
using CloudinaryDotNet;
using DotNetEnv;
using System.Text;
using MediatR;
using MooreHotelAndSuites.Application.Features.Bookings.Commands.CreateBooking;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;


services.AddMediatR(typeof(MooreHotelAndSuites.Application.Features.Bookings.Commands.CreateBooking.CreateBookingCommandHandler).Assembly);

var jwtKey = builder.Configuration["Jwt:Key"]?.Trim();
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT Key is not configured.");


var keyBytes = Encoding.UTF8.GetBytes(jwtKey);


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context => 
        {
            Console.WriteLine($"Auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context => 
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
         RoleClaimType = ClaimTypes.Role,  // Not "role"
         NameClaimType = ClaimTypes.Name 
    };
});
// ----------------- Database -----------------
// services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(
//         "Host=ep-square-cherry-a883j1zp.eastus2.azure.neon.tech;" +
//         "Port=5432;" +
//         "Database=MooreHotelAndSuite;" +
//         "Username=neondb_owner;" +
//         "Password=npg_yI9Jovi3Sjtg;" +
//         "Ssl Mode=Require;" +
//         "Trust Server Certificate=true;",
//         b => b.MigrationsAssembly("MooreHotelAndSuites.Infrastructure")
//    ));
var dbConnection =
    $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
    "Ssl Mode=Require;" +
    "Trust Server Certificate=true;" +
    "Pooling=true;" +
    "Maximum Pool Size=20;" +
    "Timeout=15;" +
    "Command Timeout=30;" +
    "Keepalive=30;";

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnection));


// ----------------- SMTP -----------------
var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "25");
var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
var smtpFrom = Environment.GetEnvironmentVariable("SMTP_FROM");


services.Configure<SmtpSettings>(options =>
{
    options.Host = Environment.GetEnvironmentVariable("SMTP_HOST")!;
    options.Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!);
    options.Username = Environment.GetEnvironmentVariable("SMTP_USERNAME")!;
    options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!;
    options.From = Environment.GetEnvironmentVariable("SMTP_FROM")!;
});


var paystackSecretKey = Environment.GetEnvironmentVariable("PAYSTACK_SECRET_KEY");
var paystackPublicKey = Environment.GetEnvironmentVariable("PAYSTACK_PUBLIC_KEY");

if (!string.IsNullOrEmpty(paystackSecretKey))
{
    builder.Services.Configure<PaystackSettings>(options =>
    {
        options.SecretKey = paystackSecretKey;
        options.PublicKey = paystackPublicKey ?? "";
        options.BaseUrl = "https://api.paystack.co";
    });
    builder.Services.AddScoped<IPaystackService, PaystackService>();
}
// ----------------- Admin Seed -----------------
var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

services.AddSignalR();

var cloudName = builder.Configuration["Cloudinary:CloudName"];
var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

var cloudinaryAccount = new Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(cloudinaryAccount);

services.AddSingleton(cloudinary);


services.AddControllers();
services.AddAutoMapper(typeof(AutoMapperProfile));


services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Moore Hotel & Suites API",
        Version = "v1"
    });

   
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT like: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});





services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

var allowedOrigins = new[] { "https://localhost:3000", "http://localhost:4200" }; // replace with your frontend URLs

services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // required for SignalR if you use authentication
    });
});







builder.Services.AddScoped<AppDbContext>();
services.AddScoped<IHotelRepository, HotelRepository>();
services.AddScoped<IRoomRepository, RoomRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<IGuestRepository, GuestRepository>();
services.AddScoped<IAuditLogWriter, AuditLogWriter>();
services.AddScoped<IAuditAnalyticsRepository, AuditAnalyticsRepository>();
services.AddScoped<IBookingReadRepository, BookingReadRepository>();
services.AddScoped<IAuditLogReadRepository, AuditLogReadRepository>();
services.AddScoped<INotificationRepository, NotificationRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
services.AddScoped<IAmenityRepository, AmenityRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IMenuRepository, MenuRepository>();

services.AddScoped<IEmailService, EmailService>();
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IAdminManagementService, AdminManagementService>();
services.AddScoped<IRoomCommandService, RoomCommandService>();
services.AddScoped<IRoomQueryService, RoomQueryService>();
services.AddScoped<IHotelService, HotelService>();
services.AddScoped<IGuestService, GuestService>();
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
services.AddScoped<IUserManagementService, UserManagementService>();
services.AddScoped<IAuditAnalyticsService, AuditAnalyticsService>();
services.AddScoped<IOperationsService, OperationsService>();
services.AddScoped<UserManager<ApplicationUser>>();
services.AddScoped<SignInManager<ApplicationUser>>();
services.Configure<PaystackSettings>(builder.Configuration.GetSection("PaystackSettings"));
services.AddScoped<IPaystackService, PaystackService>();
services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
services.AddScoped<NotificationRouter>();
services.AddScoped<IAmenityService, AmenityService>();
services.AddScoped<IDomainEventHandler<OrderCreatedEvent>, OrderCreatedHandler>();
services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

services.AddScoped<INotificationChannelHandler, KitchenNotificationHandler>();
services.AddScoped<INotificationChannelHandler, BarNotificationHandler>();
services.AddScoped<INotificationChannelHandler, RoomServiceNotificationHandler>();
services.AddScoped<INotificationChannelHandler, EventServiceNotificationHandler>();
services.AddScoped<INotificationChannelHandler, LaundryNotificationHandler>();

services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();


services.AddScoped<
    IDomainEventHandler<OrderPaymentConfirmedEvent>,
    OrderPaymentConfirmedHandler>();
services.AddSignalR();
services.AddScoped<INotificationService, NotificationService>();

services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IIdentityLookupService, IdentityEmailService>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.Initialize(db);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);

    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Moore Hotel & Suites API v1");
        c.RoutePrefix = "swagger";
    });
}


app.MapHub<NotificationHub>("/hubs/notifications");
app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuditLogMiddleware>();
app.MapControllers();

app.MapGet("/", () => "Moore Hotel & Suites API is running");

app.Run();
