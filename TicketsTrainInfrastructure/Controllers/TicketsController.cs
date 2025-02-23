using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using TicketsTrainInfrastructure;

namespace TicketsTrainInfrastructure.Controllers
{
    public class TicketsController : Controller
    {
        private readonly TicketsTrainContext _context;

        public TicketsController(TicketsTrainContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Список квитків";
            var tickets = await _context.Tickets
                .Include(t => t.Train)
                .Include(t => t.User)
                .Include(t => t.TicketTypeTrain)
                    .ThenInclude(tt => tt.TicketType)
                .Include(t => t.ArrivalStation)
                .Include(t => t.DispatchStation)
                .ToListAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Деталі квитка";
            var ticket = await _context.Tickets
                .Include(t => t.Train)
                .Include(t => t.User)
                .Include(t => t.TicketTypeTrain)
                    .ThenInclude(tt => tt.TicketType)
                .Include(t => t.ArrivalStation)
                .Include(t => t.DispatchStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Створення квитка";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name");

            // Отримуємо унікальні міста зі станцій
            var stations = await _context.RailwayStations
                .Select(s => new { s.Id, s.CityTown })
                .OrderBy(s => s.CityTown)
                .ToListAsync();

            ViewData["Stations"] = new SelectList(stations, "Id", "CityTown");
            ViewData["Trains"] = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewData["TicketTypeTrains"] = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(new Ticket { DateOfTravel = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket, string action)
        {
            if (action == "UpdateTrains")
            {
                if (ticket.DateOfTravel != default && ticket.DispatchStationId != 0 && ticket.ArrivalStationId != 0)
                {
                    var dispatchStation = await _context.RailwayStations
                        .FirstOrDefaultAsync(s => s.Id == ticket.DispatchStationId);
                    var arrivalStation = await _context.RailwayStations
                        .FirstOrDefaultAsync(s => s.Id == ticket.ArrivalStationId);

                    if (dispatchStation != null && arrivalStation != null)
                    {
                        var availableTrains = await _context.Routes
                            .Include(r => r.TrainAtRoutes)
                                .ThenInclude(tar => tar.Train)
                            .Where(r => r.StartStation == dispatchStation.CityTown && r.EndStation == arrivalStation.CityTown)
                            .SelectMany(r => r.TrainAtRoutes)
                            .Select(tar => tar.Train)
                            .Where(t => t.Date == ticket.DateOfTravel)
                            .Distinct()
                            .ToListAsync();

                        ViewData["Trains"] = new SelectList(availableTrains, "Id", "TrainName");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Будь ласка, оберіть дату подорожі та станції відправлення і прибуття");
                }

                ViewData["TicketTypeTrains"] = new SelectList(Enumerable.Empty<SelectListItem>());
                PrepareViewData(ticket);
                return View(ticket);
            }

            if (action == "UpdateTicketTypes" && ticket.TrainId != 0)
            {
                var ticketTypes = await _context.TicketTypeTrains
                    .Include(tt => tt.TicketType)
                    .Where(tt => tt.TrainId == ticket.TrainId)
                    .Select(tt => new
                    {
                        tt.Id,
                        Name = $"{tt.TicketType.Name} (Ціна: {tt.TicketType.Price} грн)"
                    })
                    .ToListAsync();

                ViewData["TicketTypeTrains"] = new SelectList(ticketTypes, "Id", "Name");

                if (ticket.DispatchStationId != 0 && ticket.ArrivalStationId != 0)
                {
                    var trains = await GetAvailableTrains(ticket.DispatchStationId, ticket.ArrivalStationId, ticket.DateOfTravel);
                    ViewData["Trains"] = new SelectList(trains, "Id", "TrainName", ticket.TrainId);
                }

                PrepareViewData(ticket);
                return View(ticket);
            }

            // Створення нового квитка
            if (ticket.UserId != 0 && ticket.TrainId != 0 && ticket.TicketTypeTrainId != 0 &&
        ticket.DispatchStationId != 0 && ticket.ArrivalStationId != 0 && ticket.DateOfTravel != default)
            {
                var ticketToCreate = await _context.Tickets
                    .Include(t => t.Train)
                    .Include(t => t.User)
                    .Include(t => t.TicketTypeTrain)
                    .FirstOrDefaultAsync(t =>
                        t.TrainId == ticket.TrainId &&
                        t.UserId == ticket.UserId &&
                        t.DateOfTravel == ticket.DateOfTravel);

                if (ticketToCreate == null)
                {
                    var newTicket = new Ticket
                    {
                        UserId = ticket.UserId,
                        TrainId = ticket.TrainId,
                        TicketTypeTrainId = ticket.TicketTypeTrainId,
                        DispatchStationId = ticket.DispatchStationId,
                        ArrivalStationId = ticket.ArrivalStationId,
                        DateOfTravel = ticket.DateOfTravel,
                        BookingDate = DateOnly.FromDateTime(DateTime.Today) // Додаємо поточну дату для BookingDate
                    };

                    _context.Tickets.Add(newTicket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            PrepareViewData(ticket);
            return View(ticket);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Редагування квитка";
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            PrepareViewData(ticket);

            if (ticket.TrainId != 0)
            {
                var trains = await GetAvailableTrains(ticket.DispatchStationId, ticket.ArrivalStationId, ticket.DateOfTravel);
                ViewData["Trains"] = new SelectList(trains, "Id", "TrainName", ticket.TrainId);

                var ticketTypes = await _context.TicketTypeTrains
                    .Include(tt => tt.TicketType)
                    .Where(tt => tt.TrainId == ticket.TrainId)
                    .Select(tt => new
                    {
                        tt.Id,
                        Name = $"{tt.TicketType.Name} (Ціна: {tt.TicketType.Price} грн)"
                    })
                    .ToListAsync();

                ViewData["TicketTypeTrains"] = new SelectList(ticketTypes, "Id", "Name", ticket.TicketTypeTrainId);
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

            if (ticket.UserId != 0 && ticket.TrainId != 0 && ticket.TicketTypeTrainId != 0 &&
                ticket.DispatchStationId != 0 && ticket.ArrivalStationId != 0 && ticket.DateOfTravel != default)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            PrepareViewData(ticket);
            return View(ticket);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Видалення квитка";
            var ticket = await _context.Tickets
                .Include(t => t.Train)
                .Include(t => t.User)
                .Include(t => t.TicketTypeTrain)
                    .ThenInclude(tt => tt.TicketType)
                .Include(t => t.ArrivalStation)
                .Include(t => t.DispatchStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Train>> GetAvailableTrains(int departureStationId, int arrivalStationId, DateOnly travelDate)
        {
            var dispatchStation = await _context.RailwayStations
                .FirstOrDefaultAsync(s => s.Id == departureStationId);
            var arrivalStation = await _context.RailwayStations
                .FirstOrDefaultAsync(s => s.Id == arrivalStationId);

            if (dispatchStation == null || arrivalStation == null)
                return new List<Train>();

            return await _context.Routes
                .Include(r => r.TrainAtRoutes)
                    .ThenInclude(tar => tar.Train)
                .Where(r => r.StartStation == dispatchStation.CityTown && r.EndStation == arrivalStation.CityTown)
                .SelectMany(r => r.TrainAtRoutes)
                .Select(tar => tar.Train)
                .Where(t => t.Date == travelDate)
                .Distinct()
                .ToListAsync();
        }

        private void PrepareViewData(Ticket ticket)
        {
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name", ticket.UserId);

            var stations = _context.RailwayStations
                .Select(s => new { s.Id, s.CityTown })
                .OrderBy(s => s.CityTown)
                .ToList();

            ViewData["Stations"] = new SelectList(stations, "Id", "CityTown");
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}