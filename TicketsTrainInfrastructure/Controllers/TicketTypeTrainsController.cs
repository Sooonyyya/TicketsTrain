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
    public class TicketTypeTrainsController : Controller
    {
        private readonly TicketsTrainContext _context;

        public TicketTypeTrainsController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: TicketTypeTrains
        public async Task<IActionResult> Index()
        {
            var ticketTypeTrains = await _context.TicketTypeTrains
                .Include(tt => tt.TicketType)
                .Include(tt => tt.Train)
                .ToListAsync();
            return View(ticketTypeTrains);
        }

        // GET: TicketTypeTrains/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ticketTypeTrain = await _context.TicketTypeTrains
                .Include(tt => tt.TicketType)
                .Include(tt => tt.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticketTypeTrain == null)
                return NotFound();

            return View(ticketTypeTrain);
        }

        // GET: TicketTypeTrains/Create
        public IActionResult Create()
        {
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName");
            return View();
        }

        // POST: TicketTypeTrains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketTypeId,TrainId")] TicketTypeTrain ticketTypeTrain)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketTypeTrain);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticketTypeTrain.TicketTypeId);
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName", ticketTypeTrain.TrainId);
            return View(ticketTypeTrain);
        }

        // GET: TicketTypeTrains/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ticketTypeTrain = await _context.TicketTypeTrains.FindAsync(id);
            if (ticketTypeTrain == null)
                return NotFound();

            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticketTypeTrain.TicketTypeId);
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName", ticketTypeTrain.TrainId);
            return View(ticketTypeTrain);
        }

        // POST: TicketTypeTrains/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketTypeId,TrainId")] TicketTypeTrain ticketTypeTrain)
        {
            if (id != ticketTypeTrain.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketTypeTrain);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TicketTypeTrains.Any(e => e.Id == ticketTypeTrain.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticketTypeTrain.TicketTypeId);
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "TrainName", ticketTypeTrain.TrainId);
            return View(ticketTypeTrain);
        }

        // GET: TicketTypeTrains/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var ticketTypeTrain = await _context.TicketTypeTrains
                .Include(tt => tt.TicketType)
                .Include(tt => tt.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticketTypeTrain == null)
                return NotFound();

            return View(ticketTypeTrain);
        }

        // POST: TicketTypeTrains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketTypeTrain = await _context.TicketTypeTrains.FindAsync(id);
            if (ticketTypeTrain != null)
            {
                _context.TicketTypeTrains.Remove(ticketTypeTrain);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
