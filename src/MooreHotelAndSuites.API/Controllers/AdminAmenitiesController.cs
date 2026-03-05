using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MooreHotelAndSuites.Application.DTOs.Amenities;
using MooreHotelAndSuites.Application.Interfaces.Services;

namespace MooreHotelAndSuites.API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenitiesController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityDto dto)
    {
        var amenity = await _amenityService.CreateAsync(dto);
        return Ok(amenity);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetById(Guid id)
    {
        var amenity = await _amenityService.GetByIdAsync(id);
        return amenity != null ? Ok(amenity) : NotFound();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var amenities = await _amenityService.GetAllAsync();
        return Ok(amenities);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateAmenity(Guid id, [FromBody] UpdateAmenityDto dto)
    {
        var updated = await _amenityService.UpdateAsync(id, dto);
        return updated != null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteAmenity(Guid id)
    {
        var deleted = await _amenityService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

}
