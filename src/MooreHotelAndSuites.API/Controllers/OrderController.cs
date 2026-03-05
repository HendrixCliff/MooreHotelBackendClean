using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Application.DTOs.Order;
using MooreHotelAndSuites.Application.DTOs.Laundry;
using MooreHotelAndSuites.Infrastructure.Data;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Application.Interfaces.Identity;
using System.Security.Claims;

namespace MooreHotelAndSuites.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly IOrderRepository _repo;
        private readonly ICurrentUserService _currentUser;

        public OrderController(
            IOrderService service,
            IOrderRepository repo,
            ICurrentUserService currentUser)
        {
            _service = service;
            _repo = repo;
            _currentUser = currentUser;
        }

        // Authenticated or unauthenticated users
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            // Get user ID if authenticated
            var userId = _currentUser.UserId;
            
            var (id, amount) = await _service.CreateOrderAsync(dto, userId);

            return Ok(new
            {
                orderId = id,
                amount,
                paymentRequired = true
            });
        }

                [Authorize(Roles = "Admin,Manager,Bar,Kitchen,Laundry")]
       [HttpGet("orders/all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _repo.GetAllOrdersAsync();
            
            return Ok(orders.Select(o => new 
            { 
                Id = o.Id,
                RoomNumber = o.RoomNumber,
                CustomerName = o.CustomerName,
                PhoneNumber = o.PhoneNumber,
                GuestName = o.Guest?.FullName ?? o.CustomerName,
                GuestPhone = o.Guest?.PhoneNumber ?? o.PhoneNumber,
                Source = o.Source,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                StatusText = o.Status switch
                {
                    OrderStatus.PendingPayment => "Pending Payment",
                    OrderStatus.Confirmed => "Confirmed",
                    OrderStatus.Served => "Served",
                    OrderStatus.Cancelled => "Cancelled",
                    _ => "Unknown"
                },
                
                RoomItems = o.Items
                    .GroupBy(i => i.RoomNumber)
                    .Select(g => new 
                    {
                        RoomNumber = g.Key ?? "N/A",
                        Items = g.Select(i => new 
                        {
                            MenuName = i.MenuName,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Total = i.Total
                        }).ToList()
                    }).ToList()
            }));
        }

                [Authorize(Roles = "Admin,Manager,Bar,Kitchen,Laundry")]
        [HttpGet("orders/all/pending")]
        public async Task<IActionResult> GetAllPendingOrders()
        {
            var orders = await _repo.GetAllPendingOrdersAsync();
            
            return Ok(orders.Select(o => new 
            { 
                Id = o.Id,
                RoomNumber = o.RoomNumber,
                CustomerName = o.CustomerName,
                PhoneNumber = o.PhoneNumber,
                GuestName = o.Guest?.FullName ?? o.CustomerName,
                GuestPhone = o.Guest?.PhoneNumber ?? o.PhoneNumber,
                Source = o.Source,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                StatusText = "Pending Payment",
              
                RoomItems = o.Items
                    .GroupBy(i => i.RoomNumber)
                    .Select(g => new 
                    {
                        RoomNumber = g.Key ?? "N/A",
                        Items = g.Select(i => new 
                        {
                            MenuName = i.MenuName,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Total = i.Total
                        }).ToList()
                    }).ToList()
            }));
        }

        [Authorize(Roles = "Admin,Manager,Bar,Kitchen,Laundry")]
        [HttpGet("orders/all/confirmed")]
        public async Task<IActionResult> GetAllConfirmedOrders()
        {
            var orders = await _repo.GetAllConfirmedOrdersAsync();
            
            return Ok(orders.Select(o => new 
            { 
                Id = o.Id,
                RoomNumber = o.RoomNumber,
                CustomerName = o.CustomerName,
                PhoneNumber = o.PhoneNumber,
                GuestName = o.Guest?.FullName ?? o.CustomerName,
                GuestPhone = o.Guest?.PhoneNumber ?? o.PhoneNumber,
                Source = o.Source,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                StatusText = "Confirmed",
                // ✅ Group items by RoomNumber (NOT o.RoomItems)
                RoomItems = o.Items
                    .GroupBy(i => i.RoomNumber)
                    .Select(g => new 
                    {
                        RoomNumber = g.Key ?? "N/A",
                        Items = g.Select(i => new 
                        {
                            MenuName = i.MenuName,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Total = i.Total
                        }).ToList()
                    }).ToList()
            }));
        }
      
      
        [HttpPost("laundry")]
        public async Task<IActionResult> Laundry(CreateLaundryOrderDto dto)
        {
            var userId = _currentUser.UserId;
            
            var (id, amount) = await _service.CreateLaundryOrderAsync(dto, userId);

            return Ok(new { orderId = id, amount });
        }

[Authorize(Roles = "Admin,Manager")]
[HttpPost("confirm-payment")]
public async Task<IActionResult> Confirm(ConfirmOrderPaymentDto dto)
{
    try
    {
        await _service.ConfirmPaymentAsync(dto);
        return Ok(new { message = "Payment confirmed successfully" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

        // Staff endpoints (require authentication)
        [Authorize(Roles = "Admin,Manager,Kitchen")]
        [HttpGet("dashboard/kitchen")]
        public async Task<IActionResult> Kitchen()
        {
            var orders = await _repo.GetConfirmedBySourcesAsync(
                OrderSource.Kitchen,
                OrderSource.RoomService);

            return Ok(orders);
        }

        [Authorize(Roles = "Admin,Manager,Bar")]
        [HttpGet("dashboard/bar")]
        public async Task<IActionResult> Bar()
        {
            var orders = await _repo.GetConfirmedBySourcesAsync(
                OrderSource.Bar);

            return Ok(orders);
        }

        [Authorize(Roles = "Admin,Manager,Laundry")]
        [HttpGet("dashboard/laundry")]
        public async Task<IActionResult> LaundryBoard()
        {
            var orders = await _repo.GetConfirmedBySourcesAsync(
                OrderSource.Laundry);

            return Ok(orders);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("dashboard/event")]
        public async Task<IActionResult> Event()
        {
            var orders = await _repo.GetConfirmedBySourcesAsync(
                OrderSource.EventHall);

            return Ok(orders);
        }

        [Authorize(Roles = "Admin,Manager,Kitchen,Bar,Laundry")]
        [HttpPost("serve")]
        public async Task<IActionResult> Serve(ServeOrderDto dto)
        {
            await _service.MarkServedAsync(dto);
            return Ok("served");
        }
    }
}