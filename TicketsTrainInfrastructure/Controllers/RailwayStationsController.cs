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
    public class RailwayStationsController : Controller
    {
        private readonly TicketsTrainContext _context;

        public RailwayStationsController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: RailwayStations
        public async Task<IActionResult> Index()
        {
            return View(await _context.RailwayStations.ToListAsync());
        }

        // GET: RailwayStations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var railwayStation = await _context.RailwayStations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (railwayStation == null)
                return NotFound();

            return View(railwayStation);
        }

        // GET: RailwayStations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RailwayStations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CityTown,Country")] RailwayStation railwayStation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(railwayStation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(railwayStation);
        }

        // GET: RailwayStations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var railwayStation = await _context.RailwayStations.FindAsync(id);
            if (railwayStation == null)
                return NotFound();

            return View(railwayStation);
        }

        // POST: RailwayStations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CityTown,Country")] RailwayStation railwayStation)
        {
            if (id != railwayStation.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(railwayStation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RailwayStationExists(railwayStation.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(railwayStation);
        }

        // GET: RailwayStations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var railwayStation = await _context.RailwayStations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (railwayStation == null)
                return NotFound();

            return View(railwayStation);
        }

        // POST: RailwayStations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var railwayStation = await _context.RailwayStations.FindAsync(id);
            if (railwayStation != null)
            {
                _context.RailwayStations.Remove(railwayStation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RailwayStationExists(int id)
        {
            return _context.RailwayStations.Any(e => e.Id == id);
        }
    }
}
