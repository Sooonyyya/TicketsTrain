using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;

namespace TicketsTrainInfrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly TicketsTrainContext _context;

        public ChartsController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET api/Charts/tickets-by-date
        [HttpGet("tickets-by-date")]
        public async Task<IActionResult> GetTicketsByDate()
        {
            try
            {
                var data = await _context.Tickets
                    .GroupBy(t => t.DateOfTravel)
                    .OrderBy(g => g.Key) // сортування за DateTime
                    .Select(g => new
                    {
                        date = g.Key.ToString("dd.MM.yyyy"),
                        ticketCount = g.Count()
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        // GET api/Charts/trains-by-passengers
        [HttpGet("trains-by-passengers")]
        public async Task<IActionResult> GetTrainsByPassengers()
        {
            try
            {
                var data = await _context.Tickets
                    .Include(t => t.Train)
                    .GroupBy(t => t.Train.TrainName)
                    .Select(g => new
                    {
                        trainName = g.Key,
                        passengerCount = g.Count()
                    })
                    .OrderByDescending(x => x.passengerCount)
                    .Take(10)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}