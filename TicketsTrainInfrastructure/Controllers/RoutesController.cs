using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using TicketsTrainInfrastructure;
using Route = TicketsTrainDomain.Model.Route;

namespace TicketsTrainInfrastructure.Controllers
{
    public class RoutesController : Controller
    {
        private readonly TicketsTrainContext _context;

        public RoutesController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: Routes
        public async Task<IActionResult> Index()
        {
            // Підтягуємо маршрути з їхніми станціями
            var routes = await _context.Routes
                .Include(r => r.StationAtRoutes)
                    .ThenInclude(sar => sar.RailwayStation)
                .ToListAsync();

            return View(routes);
        }

        // GET: Routes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var route = await _context.Routes
                .Include(r => r.StationAtRoutes)
                    .ThenInclude(sar => sar.RailwayStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (route == null)
                return NotFound();

            return View(route);
        }

        // GET: Routes/Create
        public IActionResult Create()
        {
            // MultiSelectList зі списком доступних станцій
            ViewData["AvailableStations"] = new MultiSelectList(_context.RailwayStations, "Id", "Name");
            return View();
        }

        // POST: Routes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartStation,EndStation")] Route route, int[] SelectedStationIds)
        {
            if (ModelState.IsValid)
            {
                // Спочатку зберігаємо сам маршрут
                _context.Add(route);
                await _context.SaveChangesAsync();

                // Потім зберігаємо зв’язки маршрут-станція в таблиці StationAtRoute
                foreach (var stationId in SelectedStationIds)
                {
                    var stationAtRoute = new StationAtRoute
                    {
                        RouteId = route.Id,
                        RailwayStationId = stationId
                    };
                    _context.StationAtRoutes.Add(stationAtRoute);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Якщо щось не так, повертаємо MultiSelectList і саму модель
            ViewData["AvailableStations"] = new MultiSelectList(_context.RailwayStations, "Id", "Name", SelectedStationIds);
            return View(route);
        }

        // GET: Routes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Завантажуємо маршрут разом зі станціями
            var route = await _context.Routes
                .Include(r => r.StationAtRoutes)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
                return NotFound();

            // Масив id станцій, які вже прив’язані до цього маршруту
            var selectedStationIds = route.StationAtRoutes.Select(sar => sar.RailwayStationId).ToArray();

            ViewData["AvailableStations"] = new MultiSelectList(_context.RailwayStations, "Id", "Name", selectedStationIds);
            return View(route);
        }

        // POST: Routes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartStation,EndStation")] Route route, int[] SelectedStationIds)
        {
            if (id != route.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Оновлюємо основну інформацію про маршрут
                    _context.Update(route);
                    await _context.SaveChangesAsync();

                    // Спочатку видаляємо старі зв’язки
                    var existingStations = _context.StationAtRoutes.Where(s => s.RouteId == route.Id);
                    _context.StationAtRoutes.RemoveRange(existingStations);
                    await _context.SaveChangesAsync();

                    // Додаємо нові зв’язки
                    foreach (var stationId in SelectedStationIds)
                    {
                        var stationAtRoute = new StationAtRoute
                        {
                            RouteId = route.Id,
                            RailwayStationId = stationId
                        };
                        _context.StationAtRoutes.Add(stationAtRoute);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteExists(route.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AvailableStations"] = new MultiSelectList(_context.RailwayStations, "Id", "Name", SelectedStationIds);
            return View(route);
        }

        // GET: Routes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var route = await _context.Routes
                .Include(r => r.StationAtRoutes)
                    .ThenInclude(sar => sar.RailwayStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (route == null)
                return NotFound();

            return View(route);
        }

        // POST: Routes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route != null)
            {
                _context.Routes.Remove(route);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RouteExists(int id)
        {
            return _context.Routes.Any(e => e.Id == id);
        }
    }
}
