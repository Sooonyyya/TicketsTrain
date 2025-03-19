using TicketsTrainDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace TicketsTrainInfrastructure.Services
{
    public class TicketDataPortServiceFactory
        : IDataPortServiceFactory<Ticket>
    {
        private readonly TicketsTrainContext _context;

        public TicketDataPortServiceFactory(TicketsTrainContext context)
        {
            _context = context;
        }

        public IImportService<Ticket> GetImportService(string contentType)
        {
            if (contentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return new TicketImportService(_context);
            }
            throw new NotImplementedException($"No import service implemented for tickets with content type {contentType}");
        }

        public IExportService<Ticket> GetExportService(string contentType)
        {
            if (contentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return new TicketExportService(_context);
            }
            throw new NotImplementedException($"No export service implemented for tickets with content type {contentType}");
        }
    }
}