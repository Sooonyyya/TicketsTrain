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
    public class TicketTypesController : Controller
    {
        private readonly TicketsTrainContext _context;

        public TicketTypesController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: TicketTypes
        public async Task<IActionResult> Index()
        {
            var ticketTypes = await _context.TicketTypes
                .Include(tt => tt.TicketTypeTrains)
                .ThenInclude(ttt => ttt.Train)
                .ToListAsync();
            return View(ticketTypes);
        }

        // GET: TicketTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes
                .Include(tt => tt.TicketTypeTrains)
                .ThenInclude(ttt => ttt.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticketType == null)
            {
                return NotFound();
            }

            return View(ticketType);
        }

        // GET: TicketTypes/Create
        public IActionResult Create()
        {
            ViewData["AvailableTrains"] = new MultiSelectList(
                _context.Trains.Select(t => new {
                    t.Id,
                    Display = $"{t.TrainName} ({t.Date:dd.MM.yyyy})"
                }),
                "Id", "Display"
            );
            return View();
        }

        // POST: TicketTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,SeatNumber,TrainCarriage,Price")] TicketType ticketType,
            int[] SelectedTrainIds)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketType);
                await _context.SaveChangesAsync();

                if (SelectedTrainIds != null)
                {
                    foreach (var trainId in SelectedTrainIds)
                    {
                        var ticketTypeTrain = new TicketTypeTrain
                        {
                            TicketTypeId = ticketType.Id,
                            TrainId = trainId
                        };
                        _context.TicketTypeTrains.Add(ticketTypeTrain);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["AvailableTrains"] = new MultiSelectList(
                _context.Trains.Select(t => new {
                    t.Id,
                    Display = $"{t.TrainName} ({t.Date:dd.MM.yyyy})"
                }),
                "Id", "Display"
            );
            return View(ticketType);
        }

        // GET: TicketTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes
                .Include(tt => tt.TicketTypeTrains)
                .FirstOrDefaultAsync(tt => tt.Id == id);

            if (ticketType == null)
            {
                return NotFound();
            }

            var selectedTrainIds = ticketType.TicketTypeTrains.Select(ttt => ttt.TrainId).ToArray();

            ViewData["AvailableTrains"] = new MultiSelectList(
                _context.Trains.Select(t => new {
                    t.Id,
                    Display = $"{t.TrainName} ({t.Date:dd.MM.yyyy})"
                }),
                "Id", "Display",
                selectedTrainIds
            );

            return View(ticketType);
        }

        // POST: TicketTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,SeatNumber,TrainCarriage,Price")] TicketType ticketType,
            int[] SelectedTrainIds)
        {
            if (id != ticketType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketType);
                    await _context.SaveChangesAsync();

                    // Оновлюємо зв'язки з потягами
                    var existingTicketTypeTrains = _context.TicketTypeTrains
                        .Where(ttt => ttt.TicketTypeId == ticketType.Id);
                    _context.TicketTypeTrains.RemoveRange(existingTicketTypeTrains);
                    await _context.SaveChangesAsync();

                    if (SelectedTrainIds != null)
                    {
                        foreach (var trainId in SelectedTrainIds)
                        {
                            var ticketTypeTrain = new TicketTypeTrain
                            {
                                TicketTypeId = ticketType.Id,
                                TrainId = trainId
                            };
                            _context.TicketTypeTrains.Add(ticketTypeTrain);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketTypeExists(ticketType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AvailableTrains"] = new MultiSelectList(
                _context.Trains.Select(t => new {
                    t.Id,
                    Display = $"{t.TrainName} ({t.Date:dd.MM.yyyy})"
                }),
                "Id", "Display",
                SelectedTrainIds
            );
            return View(ticketType);
        }

        // GET: TicketTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes
                .Include(tt => tt.TicketTypeTrains)
                .ThenInclude(ttt => ttt.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticketType == null)
            {
                return NotFound();
            }

            return View(ticketType);
        }

        // POST: TicketTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketType = await _context.TicketTypes
                .Include(tt => tt.TicketTypeTrains)
                .FirstOrDefaultAsync(tt => tt.Id == id);

            if (ticketType != null)
            {
                // Видаляємо всі зв'язки
                _context.TicketTypeTrains.RemoveRange(ticketType.TicketTypeTrains);
                // Видаляємо сам тип квитка
                _context.TicketTypes.Remove(ticketType);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TicketTypeExists(int id)
        {
            return _context.TicketTypes.Any(e => e.Id == id);
        }
    }
}