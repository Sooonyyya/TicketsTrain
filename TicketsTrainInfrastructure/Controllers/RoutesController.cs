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
            var routes = await _context.Routes.ToListAsync();
            return View(routes);
        }

        // GET: Routes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var route = await _context.Routes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (route == null)
                return NotFound();

            return View(route);
        }

        // GET: Routes/Create
        public IActionResult Create()
        {
            // Create SelectList objects for start and end station dropdowns using CityTown field
            ViewData["StartStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown");
            ViewData["EndStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown");

            return View();
        }

        // POST: Routes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartStation,EndStation")] Route route)
        {
            if (ModelState.IsValid)
            {
                _context.Add(route);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, repopulate the dropdown lists with CityTown field
            ViewData["StartStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown");
            ViewData["EndStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown");
            return View(route);
        }

        // GET: Routes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var route = await _context.Routes
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
                return NotFound();

            // Create SelectList objects for start and end station dropdowns using CityTown field
            ViewData["StartStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown", route.StartStation);
            ViewData["EndStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown", route.EndStation);

            return View(route);
        }

        // POST: Routes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartStation,EndStation")] Route route)
        {
            if (id != route.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(route);
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

            // If validation fails, repopulate the dropdown lists with CityTown field
            ViewData["StartStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown", route.StartStation);
            ViewData["EndStationList"] = new SelectList(_context.RailwayStations, "CityTown", "CityTown", route.EndStation);
            return View(route);
        }

        // GET: Routes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var route = await _context.Routes
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