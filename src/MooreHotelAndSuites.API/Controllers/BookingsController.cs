using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MooreHotelAndSuites.Application.DTOs.Booking;
using MooreHotelAndSuites.Application.Features.Bookings.Commands.CreateBooking;
using MooreHotelAndSuites.Application.Features.Bookings.Commands.CheckIn;
using MooreHotelAndSuites.Application.Features.Bookings.Commands.CheckOut;
using MooreHotelAndSuites.Application.Features.Bookings.Queries.GetAllBookings;
using MooreHotelAndSuites.Application.Features.Bookings.Queries.GetBookingById;
using MooreHotelAndSuites.Application.Features.Bookings.Commands.ConfirmPayment;
using MooreHotelAndSuites.Application.DTOs.Payments;
using MooreHotelAndSuites.Application.Features.Bookings.Commands.ProcessPayment;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

   
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequestDto dto)
        => Ok(await _mediator.Send(new CreateBookingCommand(dto)));

    
    [Authorize(Roles = "Staff,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _mediator.Send(new GetAllBookingsQuery()));

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _mediator.Send(new GetBookingByIdQuery(id)));

[Authorize]
[HttpPost("{id}/pay")]
public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentDto? dto)
{
    var command = new ProcessPaymentCommand(id, dto?.ReturnUrl ?? "");
    var result = await _mediator.Send(command);
    
    return Ok(result);
}

[Authorize(Roles = "Staff,Admin")]
[HttpPost("{id}/confirm-payment")]
public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentDto dto)
{
    var staffId = User.Identity?.Name ?? "Unknown";

    
    var command = new ConfirmBookingPaymentCommand(id, dto.PaymentMethod, staffId);

    var result = await _mediator.Send(command);
    return Ok(result);
}

[Authorize(Roles = "Staff,Admin")]
[HttpGet("pending")]
public async Task<IActionResult> GetPending()
{
    var result = await _mediator.Send(new GetPendingBookingsQuery());
    return Ok(result);
}


    [Authorize(Roles = "Staff,Admin")]
    [HttpPost("{id}/check-in")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        await _mediator.Send(new CheckInBookingCommand(id));
        return NoContent();
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost("{id}/check-out")]
    public async Task<IActionResult> CheckOut(Guid id)
    {
        await _mediator.Send(new CheckOutBookingCommand(id));
        return NoContent();
    }
}