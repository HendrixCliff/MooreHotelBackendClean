

namespace MooreHotelAndSuites.Application.DTOs.Menu
{
     public class UpdateMenuItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}

   