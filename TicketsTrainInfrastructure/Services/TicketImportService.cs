using ClosedXML.Excel;
using TicketsTrainDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace TicketsTrainInfrastructure.Services
{
    public class TicketImportService : IImportService<Ticket>
    {
        private readonly TicketsTrainContext _context;

        public TicketImportService(TicketsTrainContext context)
        {
            _context = context;
        }

        public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Дані не можуть бути прочитані", nameof(stream));

            var errorMessages = new List<string>();
            var successMessages = new List<string>();

            using (XLWorkbook workBook = new XLWorkbook(stream))
            {
                foreach (IXLWorksheet worksheet in workBook.Worksheets)
                {
                    var typeName = worksheet.Name;
                    var ticketType = await _context.Set<TicketType>().FirstOrDefaultAsync(tt => tt.Name == typeName, cancellationToken);
                    if (ticketType == null)
                    {
                        ticketType = new TicketType { Name = typeName, SeatNumber = 1, TrainCarriage = 1, Price = 100 };
                        _context.Set<TicketType>().Add(ticketType);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        var (error, success) = await TryAddTicketAsync(row, cancellationToken, ticketType);
                        if (!string.IsNullOrEmpty(error)) errorMessages.Add(error);
                        if (!string.IsNullOrEmpty(success)) successMessages.Add(success);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (errorMessages.Any() || successMessages.Any())
            {
                var result = "";

                if (successMessages.Any())
                {
                    result += "<div class='alert alert-success'><strong>Імпортовано квитки:</strong><ul class='mb-0'>";
                    result += string.Join("", successMessages.Select(s => $"<li>{s}</li>"));
                    result += "</ul></div>";
                }

                if (errorMessages.Any())
                {
                    result += "<div class='alert alert-danger'><strong>Помилки:</strong><ul class='mb-0'>";
                    result += string.Join("", errorMessages.Select(e => $"<li>{e}</li>"));
                    result += "</ul></div>";
                }

                throw new Exception(result);
            }
        }


        private async Task<(string? error, string? success)> TryAddTicketAsync(IXLRow row, CancellationToken cancellationToken, TicketType ticketType)
        {
            var trainName = row.Cell(1).Value.ToString();
            var userName = row.Cell(2).Value.ToString();
            var userEmail = row.Cell(3).Value.ToString();
            var dispatchName = row.Cell(4).Value.ToString();
            var arrivalName = row.Cell(5).Value.ToString();
            var travelDateStr = row.Cell(6).Value.ToString();

            var train = await _context.Set<Train>().FirstOrDefaultAsync(t => t.TrainName == trainName, cancellationToken);
            if (train == null)
            {
                train = new Train
                {
                    TrainName = trainName,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    Duration = 120,
                    DestinationId = 1,
                    NumberOfSeats = 100,
                    NumberOfCarriages = 5
                };
                _context.Set<Train>().Add(train);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);
            if (user == null)
            {
                user = new User
                {
                    Name = userName,
                    Email = userEmail,
                    Surname = "", // Більше не "Imported"
                    PhoneNumber = 380000000
                };
                _context.Set<User>().Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var dispatch = await GetOrCreateStationAsync(dispatchName, cancellationToken);
            var arrival = await GetOrCreateStationAsync(arrivalName, cancellationToken);

            if (!DateTime.TryParse(travelDateStr, out DateTime travelDate))
            {
                return ($"Некоректна дата: '{travelDateStr}' (рядок {row.RowNumber()})", null);
            }

            var date = DateOnly.FromDateTime(travelDate);

            bool exists = await _context.Set<Ticket>().AnyAsync(t =>
                t.TrainId == train.Id &&
                t.UserId == user.Id &&
                t.DateOfTravel == date &&
                t.DispatchStationId == dispatch.Id &&
                t.ArrivalStationId == arrival.Id,
                cancellationToken);

            if (exists)
            {
                return ($"Квиток для потяга '{trainName}', користувача '{userEmail}', дата '{date:yyyy-MM-dd}', маршрут '{dispatchName} → {arrivalName}' вже існує.", null);
            }

            var ticketTypeTrain = await GetOrCreateTicketTypeTrainAsync(train.Id, ticketType.Id, cancellationToken);

            var ticket = new Ticket
            {
                TrainId = train.Id,
                Train = train,
                UserId = user.Id,
                User = user,
                DateOfTravel = date,
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                TicketTypeTrainId = ticketTypeTrain.Id,
                TicketTypeTrain = ticketTypeTrain,
                DispatchStationId = dispatch.Id,
                DispatchStation = dispatch,
                ArrivalStationId = arrival.Id,
                ArrivalStation = arrival
            };

            _context.Set<Ticket>().Add(ticket);

            return (null, $"Потяг '{trainName}', користувач '{userEmail}', дата '{date:yyyy-MM-dd}', маршрут '{dispatchName} → {arrivalName}'");
        }



        private async Task AddTicketAsync(IXLRow row, CancellationToken cancellationToken, TicketType ticketType)
        {
            // Пошук або створення потяга
            var trainName = row.Cell(1).Value.ToString();
            var train = await _context.Set<Train>().FirstOrDefaultAsync(t => t.TrainName == trainName, cancellationToken);

            if (train == null)
            {
                train = new Train
                {
                    TrainName = trainName,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    Duration = 120, // За замовчуванням 2 години
                    DestinationId = 1, // За замовчуванням
                    NumberOfSeats = 100, // За замовчуванням
                    NumberOfCarriages = 5 // За замовчуванням
                };

                _context.Set<Train>().Add(train);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Пошук або створення пасажира (користувача)
            var userName = row.Cell(2).Value.ToString();
            var userEmail = row.Cell(3).Value.ToString();
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    Name = userName,
                    Email = userEmail,
                    Surname = "Imported",
                    PhoneNumber = 380000000 // За замовчуванням
                };

                _context.Set<User>().Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Пошук або створення станцій
            var dispatchStationName = row.Cell(4).Value.ToString();
            var arrivalStationName = row.Cell(5).Value.ToString();

            var dispatchStation = await GetOrCreateStationAsync(dispatchStationName, cancellationToken);
            var arrivalStation = await GetOrCreateStationAsync(arrivalStationName, cancellationToken);

            // Отримання дати поїздки
            DateTime travelDate;
            if (DateTime.TryParse(row.Cell(6).Value.ToString(), out travelDate))
            {
                // Для TicketTypeTrain
                var ticketTypeTrain = await GetOrCreateTicketTypeTrainAsync(train.Id, ticketType.Id, cancellationToken);

                // Створення нового квитка
                var ticket = new Ticket
                {
                    TrainId = train.Id,
                    Train = train,
                    UserId = user.Id,
                    User = user,
                    DateOfTravel = DateOnly.FromDateTime(travelDate),
                    BookingDate = DateOnly.FromDateTime(DateTime.Today),
                    TicketTypeTrainId = ticketTypeTrain.Id,
                    TicketTypeTrain = ticketTypeTrain,
                    DispatchStationId = dispatchStation.Id,
                    DispatchStation = dispatchStation,
                    ArrivalStationId = arrivalStation.Id,
                    ArrivalStation = arrivalStation
                };

                _context.Set<Ticket>().Add(ticket);
            }
        }

        private async Task<RailwayStation> GetOrCreateStationAsync(string stationName, CancellationToken cancellationToken)
        {
            var station = await _context.Set<RailwayStation>().FirstOrDefaultAsync(s => s.Name == stationName, cancellationToken);

            if (station == null)
            {
                station = new RailwayStation
                {
                    Name = stationName,
                    CityTown = "Imported",
                    Country = "Imported"
                };

                _context.Set<RailwayStation>().Add(station);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return station;
        }

        private async Task<TicketTypeTrain> GetOrCreateTicketTypeTrainAsync(int trainId, int ticketTypeId, CancellationToken cancellationToken)
        {
            var ticketTypeTrain = await _context.Set<TicketTypeTrain>()
                .FirstOrDefaultAsync(ttt => ttt.TrainId == trainId && ttt.TicketTypeId == ticketTypeId, cancellationToken);

            if (ticketTypeTrain == null)
            {
                ticketTypeTrain = new TicketTypeTrain
                {
                    TrainId = trainId,
                    TicketTypeId = ticketTypeId
                };

                _context.Set<TicketTypeTrain>().Add(ticketTypeTrain);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return ticketTypeTrain;
        }
    }
}