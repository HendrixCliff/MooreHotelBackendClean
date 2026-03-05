using MooreHotelAndSuites.Domain.Enums;


namespace MooreHotelAndSuites.Application.DTOs.Menu
{
      public class CreateMenuItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public OrderSource Source { get; set; }
    }
}