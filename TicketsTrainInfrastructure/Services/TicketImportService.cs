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
            {
                throw new ArgumentException("Дані не можуть бути прочитані", nameof(stream));
            }

            using (XLWorkbook workBook = new XLWorkbook(stream))
            {
                // Перегляд усіх листів (в даному випадку типи квитків)
                foreach (IXLWorksheet worksheet in workBook.Worksheets)
                {
                    // worksheet.Name - назва типу квитка. Пробуємо знайти в БД, якщо відсутня, то створюємо новий
                    var typeName = worksheet.Name;
                    var ticketType = await _context.Set<TicketType>().FirstOrDefaultAsync(tt => tt.Name == typeName, cancellationToken);

                    if (ticketType == null)
                    {
                        ticketType = new TicketType
                        {
                            Name = typeName,
                            SeatNumber = 1, // За замовчуванням
                            TrainCarriage = 1, // За замовчуванням
                            Price = 100 // За замовчуванням
                        };

                        // Додати в контекст
                        _context.Set<TicketType>().Add(ticketType);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    // Перегляд усіх рядків                    
                    foreach (var row in worksheet.RowsUsed().Skip(1)) // пропустити перший рядок, бо це заголовок
                    {
                        await AddTicketAsync(row, cancellationToken, ticketType);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
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