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
    public class TrainAtRoutesController : Controller
    {
        private readonly TicketsTrainContext _context;

        public TrainAtRoutesController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: TrainAtRoutes
        public async Task<IActionResult> Index()
        {
            var ticketsTrainContext = _context.TrainAtRoutes
                .Include(t => t.Route)
                .Include(t => t.Train);

            return View(await ticketsTrainContext.ToListAsync());
        }

        // GET: TrainAtRoutes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var trainAtRoute = await _context.TrainAtRoutes
                .Include(t => t.Route)
                .Include(t => t.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainAtRoute == null)
                return NotFound();

            return View(trainAtRoute);
        }

        // GET: TrainAtRoutes/Create
        public IActionResult Create()
        {
            // Для Route можна відобразити, наприклад, "StartStation - EndStation"
            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display"
            );

            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName");
            return View();
        }

        // POST: TrainAtRoutes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainId,RouteId")] TrainAtRoute trainAtRoute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainAtRoute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display", trainAtRoute.RouteId
            );
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName", trainAtRoute.TrainId);

            return View(trainAtRoute);
        }

        // GET: TrainAtRoutes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var trainAtRoute = await _context.TrainAtRoutes.FindAsync(id);
            if (trainAtRoute == null)
                return NotFound();

            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display", trainAtRoute.RouteId
            );
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName", trainAtRoute.TrainId);

            return View(trainAtRoute);
        }

        // POST: TrainAtRoutes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainId,RouteId")] TrainAtRoute trainAtRoute)
        {
            if (id != trainAtRoute.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainAtRoute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainAtRouteExists(trainAtRoute.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(
                _context.Routes.Select(r => new {
                    r.Id,
                    Display = r.StartStation + " - " + r.EndStation
                }),
                "Id", "Display", trainAtRoute.RouteId
            );
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName", trainAtRoute.TrainId);

            return View(trainAtRoute);
        }

        // GET: TrainAtRoutes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var trainAtRoute = await _context.TrainAtRoutes
                .Include(t => t.Route)
                .Include(t => t.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainAtRoute == null)
                return NotFound();

            return View(trainAtRoute);
        }

        // POST: TrainAtRoutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainAtRoute = await _context.TrainAtRoutes.FindAsync(id);
            if (trainAtRoute != null)
            {
                _context.TrainAtRoutes.Remove(trainAtRoute);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TrainAtRouteExists(int id)
        {
            return _context.TrainAtRoutes.Any(e => e.Id == id);
        }
    }
}
