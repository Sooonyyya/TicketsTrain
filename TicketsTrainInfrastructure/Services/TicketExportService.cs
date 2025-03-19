using ClosedXML.Excel;
using TicketsTrainDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace TicketsTrainInfrastructure.Services
{
    public class TicketExportService : IExportService<Ticket>
    {
        private const string RootWorksheetName = "";

        private static readonly IReadOnlyList<string> HeaderNames =
            new string[]
            {
                "ID квитка",
                "Поїзд",
                "Пасажир",
                "Електронна пошта",
                "Станція відправлення",
                "Станція прибуття",
                "Дата поїздки",
                "Дата бронювання",
                "Тип квитка",
                "№ вагона",
                "№ місця",
                "Ціна"
            };

        private readonly DbContext _context;

        public TicketExportService(DbContext context)
        {
            _context = context;
        }

        private static void WriteHeader(IXLWorksheet worksheet)
        {
            for (int columnIndex = 0; columnIndex < HeaderNames.Count; columnIndex++)
            {
                worksheet.Cell(1, columnIndex + 1).Value = HeaderNames[columnIndex];
            }
            worksheet.Row(1).Style.Font.Bold = true;
        }

        private void WriteTicket(IXLWorksheet worksheet, Ticket ticket, int rowIndex)
        {
            var columnIndex = 1;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.Id;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.Train.TrainName;
            worksheet.Cell(rowIndex, columnIndex++).Value = $"{ticket.User.Name} {ticket.User.Surname}";
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.User.Email;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.DispatchStation.Name;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.ArrivalStation.Name;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.DateOfTravel.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.BookingDate.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.TicketTypeTrain.TicketType.Name;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.TicketTypeTrain.TicketType.TrainCarriage;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.TicketTypeTrain.TicketType.SeatNumber;
            worksheet.Cell(rowIndex, columnIndex++).Value = ticket.TicketTypeTrain.TicketType.Price;
        }

        private void WriteTickets(IXLWorksheet worksheet, ICollection<Ticket> tickets)
        {
            WriteHeader(worksheet);
            int rowIndex = 2;
            foreach (var ticket in tickets)
            {
                WriteTicket(worksheet, ticket, rowIndex);
                rowIndex++;
            }
        }

        private void WriteTicketsByType(XLWorkbook workbook, IEnumerable<IGrouping<string, Ticket>> ticketsByType)
        {
            // Для всіх типів квитків формуємо окремі сторінки
            foreach (var group in ticketsByType)
            {
                if (group.Any())
                {
                    var worksheet = workbook.Worksheets.Add(group.Key);
                    WriteTickets(worksheet, group.ToList());
                }
            }
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Input stream is not writable");
            }

            // Отримуємо всі квитки з доданими зв'язками
            var tickets = await _context.Set<Ticket>()
                .Include(t => t.Train)
                .Include(t => t.User)
                .Include(t => t.DispatchStation)
                .Include(t => t.ArrivalStation)
                .Include(t => t.TicketTypeTrain)
                .ThenInclude(ttt => ttt.TicketType)
                .ToListAsync(cancellationToken);

            // Створюємо Excel-документ
            var workbook = new XLWorkbook();

            // Додаємо загальний лист з усіма квитками
            var allTicketsWorksheet = workbook.Worksheets.Add("Всі квитки");
            WriteTickets(allTicketsWorksheet, tickets);

            // Групуємо квитки за типом і створюємо окремі листи для кожного типу
            var ticketsByType = tickets
                .GroupBy(t => t.TicketTypeTrain.TicketType.Name)
                .OrderBy(g => g.Key);

            WriteTicketsByType(workbook, ticketsByType);

            // Додаємо лист зі статистикою
            var statsWorksheet = workbook.Worksheets.Add("Статистика");

            statsWorksheet.Cell("A1").Value = "Статистика за квитками";
            statsWorksheet.Cell("A1").Style.Font.Bold = true;
            statsWorksheet.Cell("A1").Style.Font.FontSize = 14;

            statsWorksheet.Cell("A3").Value = "Загальна кількість квитків:";
            statsWorksheet.Cell("B3").Value = tickets.Count;

            statsWorksheet.Cell("A4").Value = "Загальна сума:";
            statsWorksheet.Cell("B4").Value = tickets.Sum(t => t.TicketTypeTrain.TicketType.Price);
            statsWorksheet.Cell("B4").Style.NumberFormat.Format = "# ### грн";

            statsWorksheet.Cell("A6").Value = "Статистика за типами квитків:";
            statsWorksheet.Cell("A6").Style.Font.Bold = true;

            int row = 7;
            foreach (var group in ticketsByType)
            {
                statsWorksheet.Cell(row, 1).Value = group.Key;
                statsWorksheet.Cell(row, 2).Value = group.Count();
                statsWorksheet.Cell(row, 3).Value = group.Sum(t => t.TicketTypeTrain.TicketType.Price);
                statsWorksheet.Cell(row, 3).Style.NumberFormat.Format = "# ### грн";
                row++;
            }

            statsWorksheet.Cell("A" + (row + 2)).Value = "Статистика за поїздами:";
            statsWorksheet.Cell("A" + (row + 2)).Style.Font.Bold = true;

            row += 3;
            var ticketsByTrain = tickets
                .GroupBy(t => t.Train.TrainName)
                .OrderByDescending(g => g.Count());

            foreach (var group in ticketsByTrain)
            {
                statsWorksheet.Cell(row, 1).Value = group.Key;
                statsWorksheet.Cell(row, 2).Value = group.Count();
                statsWorksheet.Cell(row, 3).Value = group.Sum(t => t.TicketTypeTrain.TicketType.Price);
                statsWorksheet.Cell(row, 3).Style.NumberFormat.Format = "# ### грн";
                row++;
            }

            statsWorksheet.Cell("A" + (row + 2)).Value = "Статистика за маршрутами:";
            statsWorksheet.Cell("A" + (row + 2)).Style.Font.Bold = true;

            row += 3;
            var ticketsByRoute = tickets
                .GroupBy(t => new { From = t.DispatchStation.Name, To = t.ArrivalStation.Name })
                .OrderByDescending(g => g.Count());

            foreach (var group in ticketsByRoute)
            {
                statsWorksheet.Cell(row, 1).Value = $"{group.Key.From} → {group.Key.To}";
                statsWorksheet.Cell(row, 2).Value = group.Count();
                statsWorksheet.Cell(row, 3).Value = group.Sum(t => t.TicketTypeTrain.TicketType.Price);
                statsWorksheet.Cell(row, 3).Style.NumberFormat.Format = "# ### грн";
                row++;
            }

            // Налаштовуємо ширину колонок
            foreach (var worksheet in workbook.Worksheets)
            {
                worksheet.Columns().AdjustToContents();
            }

            // Зберігаємо результат
            workbook.SaveAs(stream);
        }
    }
}