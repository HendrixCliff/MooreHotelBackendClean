using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;
using System.Security.Claims;
using MooreHotelAndSuites.Application.DTOs.Menu;

namespace MooreHotelAndSuites.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _repo;

        public MenuController(IMenuRepository repo)
        {
            _repo = repo;
        }

        // Get all menu items
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAsync());

        // Get menu items by source (Kitchen, Bar, etc.)
        [HttpGet("source/{source}")]
        public async Task<IActionResult> GetBySource(OrderSource source)
            => Ok(await _repo.GetBySourceAsync(source));

        // Get single menu item
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
  }
 
       
[Authorize]
[HttpPost]
public async Task<IActionResult> Create(CreateMenuItemDto dto)
{
 
 var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    if (string.IsNullOrEmpty(userRole))
        return StatusCode(403, "No role found");
    
    if (userRole != "Admin" && userRole != "Manager")
        return StatusCode(403, "Admin or Manager only");
    
    var menuItem = new MenuItem(dto.Name, dto.Price, dto.Source);
    await _repo.AddAsync(menuItem);
    return CreatedAtAction(nameof(GetById), new { id = menuItem.Id }, menuItem);
}

        // Update menu item (Admin/Manager only)
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateMenuItemDto dto)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.UpdateDetails(dto.Name, dto.Price);
            item.SetAvailability(dto.IsAvailable);
            
            await _repo.UpdateAsync(item);
            return Ok(item);
        }

        // Delete menu item (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            await _repo.DeleteAsync(item);
            return NoContent();
        }
    }

  
}