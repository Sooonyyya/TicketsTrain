using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using TicketsTrainInfrastructure;

namespace TicketsTrainInfrastructure.Controllers
{
    public class TrainsController : Controller
    {
        private readonly TicketsTrainContext _context;

        public TrainsController(TicketsTrainContext context)
        {
            _context = context;
        }

        private async Task CreateTrainTicketTypeConnections(Train train)
        {
            // Отримуємо всі типи квитків
            var ticketTypes = await _context.TicketTypes.ToListAsync();

            // Створюємо зв'язки для кожного типу квитка
            foreach (var ticketType in ticketTypes)
            {
                var ticketTypeTrain = new TicketTypeTrain
                {
                    TrainId = train.Id,
                    TicketTypeId = ticketType.Id,
                    Train = train,
                    TicketType = ticketType
                };

                _context.TicketTypeTrains.Add(ticketTypeTrain);
            }

            await _context.SaveChangesAsync();
        }

        // GET: Trains
        public async Task<IActionResult> Index()
        {
            var trains = await _context.Trains
                .Include(t => t.TrainAtRoutes)
                .ThenInclude(tar => tar.Route)
                .ToListAsync();

            return View(trains);
        }

        // GET: Trains/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var train = await _context.Trains
                .Include(t => t.TrainAtRoutes)
                .ThenInclude(tar => tar.Route)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (train == null)
                return NotFound();

            return View(train);
        }

        // GET: Trains/Create
        public IActionResult Create()
        {
            ViewData["AvailableRoutes"] = new MultiSelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display"
            );
            return View();
        }

        // POST: Trains/Create
        // POST: Trains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainName,Date,Duration,NumberOfSeats,NumberOfCarriages")] Train train, int[] SelectedRouteIds)
        {
            // Перевіряємо, чи дата не в минулому
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (train.Date < today)
            {
                ModelState.AddModelError("Date", "Неможливо створити потяг на дату в минулому");
                ViewData["AvailableRoutes"] = new MultiSelectList(
                    _context.Routes.Select(r => new {
                        r.Id,
                        Display = r.StartStation + " - " + r.EndStation
                    }),
                    "Id", "Display",
                    SelectedRouteIds
                );
                return View(train);
            }
            if (ModelState.IsValid)
            {
                _context.Trains.Add(train);
                await _context.SaveChangesAsync();

                foreach (var routeId in SelectedRouteIds)
                {
                    var trainAtRoute = new TrainAtRoute
                    {
                        TrainId = train.Id,
                        RouteId = routeId
                    };
                    _context.TrainAtRoutes.Add(trainAtRoute);
                }
                await _context.SaveChangesAsync();

                // Створюємо зв'язки з типами квитків
                await CreateTrainTicketTypeConnections(train);

                return RedirectToAction(nameof(Index));
            }

            ViewData["AvailableRoutes"] = new MultiSelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display",
                SelectedRouteIds
            );
            return View(train);
        }

        // GET: Trains/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var train = await _context.Trains
                .Include(t => t.TrainAtRoutes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (train == null)
                return NotFound();

            var selectedRouteIds = train.TrainAtRoutes.Select(tar => tar.RouteId).ToArray();

            ViewData["AvailableRoutes"] = new MultiSelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display",
                selectedRouteIds
            );

            return View(train);
        }

        // POST: Trains/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainName,Date,Duration,NumberOfSeats,NumberOfCarriages")] Train train, int[] SelectedRouteIds)
        {
            if (id != train.Id)
                return NotFound();

            // Перевіряємо, чи дата не в минулому
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (train.Date < today)
            {
                ModelState.AddModelError("Date", "Неможливо змінити дату потяга на дату в минулому");
                ViewData["AvailableRoutes"] = new MultiSelectList(
                    _context.Routes.Select(r => new {
                        r.Id,
                        Display = r.StartStation + " - " + r.EndStation
                    }),
                    "Id", "Display",
                    SelectedRouteIds
                );
                return View(train);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(train);
                    await _context.SaveChangesAsync();

                    var existingTrainAtRoutes = _context.TrainAtRoutes
                        .Where(tar => tar.TrainId == train.Id);
                    _context.TrainAtRoutes.RemoveRange(existingTrainAtRoutes);
                    await _context.SaveChangesAsync();

                    foreach (var routeId in SelectedRouteIds)
                    {
                        var trainAtRoute = new TrainAtRoute
                        {
                            TrainId = train.Id,
                            RouteId = routeId
                        };
                        _context.TrainAtRoutes.Add(trainAtRoute);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainExists(train.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AvailableRoutes"] = new MultiSelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display",
                SelectedRouteIds
            );
            return View(train);
        }

        // GET: Trains/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var train = await _context.Trains
                .Include(t => t.TrainAtRoutes)
                .ThenInclude(tar => tar.Route)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (train == null)
                return NotFound();

            return View(train);
        }

        // POST: Trains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var train = await _context.Trains
                .Include(t => t.TrainAtRoutes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (train != null)
            {
                _context.TrainAtRoutes.RemoveRange(train.TrainAtRoutes);
                _context.Trains.Remove(train);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TrainExists(int id)
        {
            return _context.Trains.Any(e => e.Id == id);
        }
    }
}