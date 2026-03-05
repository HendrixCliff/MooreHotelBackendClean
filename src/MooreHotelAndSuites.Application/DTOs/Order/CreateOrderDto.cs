using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.DTOs.Order
{
   public class CreateOrderDto
{
    public OrderSource Source { get; set; }

    public string? CustomerName { get; set; }
    public string? PhoneNumber { get; set; }


    public List<RoomItemDto> RoomItems { get; set; } = new();
}


public class RoomItemDto
{
    public string? RoomNumber { get; set; }  // ✅ Optional - null if single room
    public List<OrderItemDto> Items { get; set; } = new();
}


}