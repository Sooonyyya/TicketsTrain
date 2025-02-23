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
    public class StationAtRoutesController : Controller
    {
        private readonly TicketsTrainContext _context;

        public StationAtRoutesController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: StationAtRoutes
        public async Task<IActionResult> Index()
        {
            var stationAtRoutes = await _context.StationAtRoutes
                .Include(s => s.RailwayStation)
                .Include(s => s.Route)
                .ToListAsync();

            return View(stationAtRoutes);
        }

        // GET: StationAtRoutes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var stationAtRoute = await _context.StationAtRoutes
                .Include(s => s.RailwayStation)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (stationAtRoute == null)
                return NotFound();

            return View(stationAtRoute);
        }

        // GET: StationAtRoutes/Create
        public IActionResult Create()
        {
            // Маршрути з відображенням "StartStation - EndStation"
            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    RouteDisplay = r.StartStation + " - " + r.EndStation
                }),
                "Id", "RouteDisplay"
            );

            ViewData["RailwayStationId"] = new SelectList(_context.RailwayStations, "Id", "Name");
            return View();
        }

        // POST: StationAtRoutes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RouteId,RailwayStationId")] StationAtRoute stationAtRoute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stationAtRoute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    RouteDisplay = r.StartStation + " - " + r.EndStation
                }),
                "Id", "RouteDisplay", stationAtRoute.RouteId
            );
            ViewData["RailwayStationId"] = new SelectList(_context.RailwayStations, "Id", "Name", stationAtRoute.RailwayStationId);
            return View(stationAtRoute);
        }

        // GET: StationAtRoutes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var stationAtRoute = await _context.StationAtRoutes.FindAsync(id);
            if (stationAtRoute == null)
                return NotFound();

            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    RouteDisplay = r.StartStation + " - " + r.EndStation
                }),
                "Id", "RouteDisplay", stationAtRoute.RouteId
            );
            ViewData["RailwayStationId"] = new SelectList(_context.RailwayStations, "Id", "Name", stationAtRoute.RailwayStationId);

            return View(stationAtRoute);
        }

        // POST: StationAtRoutes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RouteId,RailwayStationId")] StationAtRoute stationAtRoute)
        {
            if (id != stationAtRoute.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stationAtRoute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StationAtRouteExists(stationAtRoute.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    RouteDisplay = r.StartStation + " - " + r.EndStation
                }),
                "Id", "RouteDisplay", stationAtRoute.RouteId
            );
            ViewData["RailwayStationId"] = new SelectList(_context.RailwayStations, "Id", "Name", stationAtRoute.RailwayStationId);

            return View(stationAtRoute);
        }

        // GET: StationAtRoutes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var stationAtRoute = await _context.StationAtRoutes
                .Include(s => s.RailwayStation)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (stationAtRoute == null)
                return NotFound();

            return View(stationAtRoute);
        }

        // POST: StationAtRoutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stationAtRoute = await _context.StationAtRoutes.FindAsync(id);
            if (stationAtRoute != null)
            {
                _context.StationAtRoutes.Remove(stationAtRoute);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StationAtRouteExists(int id)
        {
            return _context.StationAtRoutes.Any(e => e.Id == id);
        }
    }
}
