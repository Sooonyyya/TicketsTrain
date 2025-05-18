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
                .Include(t => t.TicketTypeTrain).ThenInclude(tt => tt.TicketType)
                .Include(t => t.ArrivalStation)
                .Include(t => t.DispatchStation)
                .ToListAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Створення квитка";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name");

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
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    if (ticket.DateOfTravel < today)
                    {
                        ModelState.AddModelError("DateOfTravel", "Неможливо обрати дату в минулому");
                        PrepareViewData(ticket);
                        return View(ticket);
                    }
                    var dispatchStation = await _context.RailwayStations.FirstOrDefaultAsync(s => s.Id == ticket.DispatchStationId);
                    var arrivalStation = await _context.RailwayStations.FirstOrDefaultAsync(s => s.Id == ticket.ArrivalStationId);

                    if (dispatchStation != null && arrivalStation != null)
                    {
                        var availableTrains = await _context.Routes
                            .Include(r => r.TrainAtRoutes).ThenInclude(tar => tar.Train)
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
                var ticketTypeTrains = await _context.TicketTypeTrains
                    .Include(tt => tt.TicketType)
                    .Where(tt => tt.TrainId == ticket.TrainId)
                    .ToListAsync();

                var ticketTypes = ticketTypeTrains
                    .GroupBy(tt => tt.TicketType.Name)
                    .Select(g => g.First())
                    .Select(tt => new
                    {
                        tt.Id,
                        Name = $"{tt.TicketType.Name} (Ціна: {tt.TicketType.Price} грн)"
                    })
                    .ToList();

                ViewData["TicketTypeTrains"] = new SelectList(ticketTypes, "Id", "Name");

                if (ticket.DispatchStationId != 0 && ticket.ArrivalStationId != 0)
                {
                    var trains = await GetAvailableTrains(ticket.DispatchStationId, ticket.ArrivalStationId, ticket.DateOfTravel);
                    ViewData["Trains"] = new SelectList(trains, "Id", "TrainName", ticket.TrainId);
                }

                PrepareViewData(ticket);
                return View(ticket);
            }

            if (ticket.UserId != 0 && ticket.TrainId != 0 && ticket.TicketTypeTrainId != 0 &&
                ticket.DispatchStationId != 0 && ticket.ArrivalStationId != 0 && ticket.DateOfTravel != default)
            {
                var ticketToCreate = await _context.Tickets
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
                        BookingDate = DateOnly.FromDateTime(DateTime.Today)
                    };

                    _context.Tickets.Add(newTicket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            PrepareViewData(ticket);
            return View(ticket);
        }

        private async Task<List<Train>> GetAvailableTrains(int departureStationId, int arrivalStationId, DateOnly travelDate)
        {
            var dispatchStation = await _context.RailwayStations.FirstOrDefaultAsync(s => s.Id == departureStationId);
            var arrivalStation = await _context.RailwayStations.FirstOrDefaultAsync(s => s.Id == arrivalStationId);

            if (dispatchStation == null || arrivalStation == null)
                return new List<Train>();

            return await _context.Routes
                .Include(r => r.TrainAtRoutes).ThenInclude(tar => tar.Train)
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
    }
}


/*using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using TicketsTrainInfrastructure;
using TicketsTrainInfrastructure.Services;

namespace TicketsTrainInfrastructure.Controllers
{
    public class TicketsController : Controller
    {
        private readonly TicketsTrainContext _context;
        private readonly IDataPortServiceFactory<Ticket> _ticketDataPortServiceFactory;

        public TicketsController(TicketsTrainContext context, IDataPortServiceFactory<Ticket> ticketDataPortServiceFactory)
        {
            _context = context;
            _ticketDataPortServiceFactory = ticketDataPortServiceFactory;
        }

        public async Task<IActionResult> Index(int? departureStationId, int? arrivalStationId, string travelDate,
            decimal? minPrice, decimal? maxPrice, string trainNumber, int? ticketTypeId)
        {
            ViewData["Title"] = "Список квитків";

            // Підготовка списків для форми пошуку
            var stations = await _context.RailwayStations
                .Select(s => new { s.Id, s.CityTown })
                .OrderBy(s => s.CityTown)
                .ToListAsync();

            ViewData["Stations"] = new SelectList(stations, "Id", "CityTown");

            // Додаємо список типів квитків для фільтрації (унікальні значення)
            var ticketTypes = await _context.TicketTypes
                .Select(tt => new { tt.Id, tt.Name })
                .OrderBy(tt => tt.Name)
                .ToListAsync();

            // Видаляємо дублікати після отримання даних з бази
            var uniqueTicketTypes = ticketTypes
                .GroupBy(t => t.Name)
                .Select(g => g.First())
                .ToList();

            ViewData["TicketTypes"] = new SelectList(uniqueTicketTypes, "Id", "Name");

            // Пошук квитків
            var query = _context.Tickets
                .Include(t => t.Train)
                .Include(t => t.User)
                .Include(t => t.TicketTypeTrain)
                    .ThenInclude(tt => tt.TicketType)
                .Include(t => t.ArrivalStation)
                .Include(t => t.DispatchStation)
                .AsQueryable();

            if (departureStationId.HasValue)
            {
                query = query.Where(t => t.DispatchStationId == departureStationId.Value);
                ViewBag.SelectedDepartureStationId = departureStationId.Value;
            }

            if (arrivalStationId.HasValue)
            {
                query = query.Where(t => t.ArrivalStationId == arrivalStationId.Value);
                ViewBag.SelectedArrivalStationId = arrivalStationId.Value;
            }

            if (!string.IsNullOrEmpty(travelDate))
            {
                if (DateOnly.TryParse(travelDate, out var date))
                {
                    query = query.Where(t => t.DateOfTravel == date);
                    ViewBag.SelectedDate = travelDate;
                }
            }

            // Фільтрація за ціною
            if (minPrice.HasValue)
            {
                query = query.Where(t => t.TicketTypeTrain.TicketType.Price >= minPrice.Value);
                ViewBag.MinPrice = minPrice.Value;
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(t => t.TicketTypeTrain.TicketType.Price <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice.Value;
            }

            // Фільтрація за номером потяга
            if (!string.IsNullOrEmpty(trainNumber))
            {
                query = query.Where(t => t.Train.TrainName.Contains(trainNumber));
                ViewBag.TrainNumber = trainNumber;
            }

            // Фільтрація за типом квитка
            if (ticketTypeId.HasValue)
            {
                // Отримуємо назву типу квитка для пошуку
                var ticketTypeName = await _context.TicketTypes
                    .Where(tt => tt.Id == ticketTypeId.Value)
                    .Select(tt => tt.Name)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(ticketTypeName))
                {
                    // Шукаємо за назвою типу квитка, а не за ID
                    query = query.Where(t => t.TicketTypeTrain.TicketType.Name == ticketTypeName);
                }

                ViewBag.SelectedTicketTypeId = ticketTypeId.Value;
            }

            var tickets = await query.ToListAsync();

            return View(tickets);
        }

        // GET: Tickets/Import
        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel, CancellationToken cancellationToken = default)
        {
            if (fileExcel != null && fileExcel.Length > 0)
            {
                var importService = _ticketDataPortServiceFactory.GetImportService("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                try
                {
                    using var stream = fileExcel.OpenReadStream();
                    await importService.ImportFromStreamAsync(stream, cancellationToken);
                    TempData["SuccessMessage"] = "Квитки успішно імпортовано з файлу.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }



                return RedirectToAction(nameof(Import));
            }

            ModelState.AddModelError("", "Будь ласка, оберіть файл для завантаження.");
            return View();
        }

        // GET: Tickets/Export
        [HttpGet]
        public async Task<IActionResult> Export(
            [FromQuery] string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            CancellationToken cancellationToken = default)
        {
            var exportService = _ticketDataPortServiceFactory.GetExportService(contentType);

            var memoryStream = new MemoryStream();

            await exportService.WriteToAsync(memoryStream, cancellationToken);

            await memoryStream.FlushAsync(cancellationToken);
            memoryStream.Position = 0;

            return new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = $"tickets_{DateTime.UtcNow.ToString("yyyy-MM-dd")}.xlsx"
            };
        }

        // Інші методи залишаються без змін

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
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    if (ticket.DateOfTravel < today)
                    {
                        ModelState.AddModelError("DateOfTravel", "Неможливо обрати дату в минулому");
                        PrepareViewData(ticket);
                        return View(ticket);
                    }
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
                var ticketTypeTrains = await _context.TicketTypeTrains
                    .Include(tt => tt.TicketType)
                    .Where(tt => tt.TrainId == ticket.TrainId)
                    .ToListAsync();

                // Групуємо після отримання даних з бази
                var ticketTypes = ticketTypeTrains
                    .GroupBy(tt => tt.TicketType.Name)
                    .Select(g => g.First())
                    .Select(tt => new
                    {
                        tt.Id,
                        Name = $"{tt.TicketType.Name} (Ціна: {tt.TicketType.Price} грн)"
                    })
                    .ToList();

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

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Редагування квитка";
            var ticket = await _context.Tickets
                .Include(t => t.Train)
                .Include(t => t.User)
                .Include(t => t.TicketTypeTrain)
                    .ThenInclude(tt => tt.TicketType)
                .Include(t => t.ArrivalStation)
                .Include(t => t.DispatchStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            // Додаємо тільки список пасажирів
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name", ticket.UserId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int UserId)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            if (UserId != 0)
            {
                ticket.UserId = UserId;

                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name", UserId);

            return View(ticket);
        }

        // DELETE: Tickets/Delete/5
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


        // GET: Tickets/TicketMaster
        public async Task<IActionResult> TicketMaster()
        {
            ViewData["Title"] = "Майстер бронювання квитків";

            // Підготуємо станції для вибору
            var stations = await _context.RailwayStations
                .Select(s => new { s.Id, s.CityTown })
                .OrderBy(s => s.CityTown)
                .ToListAsync();

            ViewData["Stations"] = new SelectList(stations, "Id", "CityTown");

            // За замовчуванням встановлюємо дату на завтра
            ViewData["DefaultDate"] = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd");

            // Підготуємо користувачів для вибору
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name");

            return View();
        }

        // POST: Tickets/TicketMaster
        [HttpPost]
        public async Task<IActionResult> TicketMaster(int departureStationId, int arrivalStationId,
            string travelDate, int? userId, int? trainId, int? ticketTypeId, string selectedDateVal = null)
        {
            // Підготовка базових даних
            var stations = await _context.RailwayStations
                .Select(s => new { s.Id, s.CityTown })
                .OrderBy(s => s.CityTown)
                .ToListAsync();

            ViewData["Stations"] = new SelectList(stations, "Id", "CityTown");

            // Встановлення значення для дати
            DateOnly parsedTravelDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            // Спочатку перевіряємо selectedDateVal (збережене значення з форми)
            if (!string.IsNullOrEmpty(selectedDateVal) && DateOnly.TryParse(selectedDateVal, out var selectedDate))
            {
                parsedTravelDate = selectedDate;
            }
            // Якщо не знайдено в selectedDateVal, перевіряємо travelDate
            else if (!string.IsNullOrEmpty(travelDate) && DateOnly.TryParse(travelDate, out var date))
            {
                parsedTravelDate = date;
            }

            // Зберігаємо дату в кількох місцях для консистентності
            ViewData["DefaultDate"] = parsedTravelDate.ToString("yyyy-MM-dd");
            ViewData["SelectedDate"] = parsedTravelDate;
            ViewBag.TravelDate = parsedTravelDate.ToString("yyyy-MM-dd");

            ViewData["Users"] = new SelectList(_context.Users, "Id", "Name", userId);

            var departureStation = await _context.RailwayStations.FindAsync(departureStationId);
            var arrivalStation = await _context.RailwayStations.FindAsync(arrivalStationId);

            if (departureStation == null || arrivalStation == null)
            {
                ModelState.AddModelError("", "Будь ласка, виберіть станції відправлення та прибуття");
                return View();
            }

            // Зберігаємо вибрані значення для станцій
            ViewData["SelectedDepartureId"] = departureStationId;
            ViewData["SelectedArrivalId"] = arrivalStationId;
            ViewData["SelectedUserId"] = userId;
            ViewData["SelectedTrainId"] = trainId;

            // Шукаємо потяги для вибраного маршруту і дати
            var availableTrains = await _context.Routes
                .Include(r => r.TrainAtRoutes)
                    .ThenInclude(tar => tar.Train)
                .Where(r => r.StartStation == departureStation.CityTown && r.EndStation == arrivalStation.CityTown)
                .SelectMany(r => r.TrainAtRoutes)
                .Select(tar => tar.Train)
                .Where(t => t.Date == parsedTravelDate)
                .Distinct()
                .ToListAsync();

            ViewData["AvailableTrains"] = new SelectList(availableTrains, "Id", "TrainName", trainId);

            // Якщо потяг вибрано, шукаємо доступні типи квитків
            if (trainId.HasValue)
            {
                var ticketTypeTrains = await _context.TicketTypeTrains
                    .Include(tt => tt.TicketType)
                    .Where(tt => tt.TrainId == trainId.Value)
                    .ToListAsync();

                // Отримуємо список унікальних типів квитків для фільтра
                var availableTicketTypes = ticketTypeTrains
                    .Select(tt => tt.TicketType.Name)
                    .Distinct()
                    .ToList();

                ViewBag.AvailableTicketTypes = availableTicketTypes;

                // Отримуємо TicketTypeTrains з групуванням для видалення дублікатів
                var ticketTypes = ticketTypeTrains
                    .GroupBy(tt => tt.TicketType.Name)
                    .Select(g => g.First())
                    .Select(tt => new
                    {
                        tt.Id,
                        Name = $"{tt.TicketType.Name} (Ціна: {tt.TicketType.Price} грн)"
                    })
                    .ToList();

                ViewData["TicketTypes"] = new SelectList(ticketTypes, "Id", "Name", ticketTypeId);
            }

            // Якщо всі поля заповнені, створюємо квиток
            if (userId.HasValue && trainId.HasValue && ticketTypeId.HasValue)
            {
                var ticket = new Ticket
                {
                    UserId = userId.Value,
                    TrainId = trainId.Value,
                    TicketTypeTrainId = ticketTypeId.Value,
                    DispatchStationId = departureStationId,
                    ArrivalStationId = arrivalStationId,
                    DateOfTravel = parsedTravelDate,
                    BookingDate = DateOnly.FromDateTime(DateTime.Today)
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Квиток успішно створено!";
                return RedirectToAction(nameof(TicketMaster));
            }

            return View();
        }
    }
}*/