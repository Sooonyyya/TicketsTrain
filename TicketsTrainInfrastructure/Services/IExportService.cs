using TicketsTrainDomain.Model;
namespace TicketsTrainInfrastructure.Services
{
    public interface IExportService<TEntity>
where TEntity : Entity
    {
        Task WriteToAsync(Stream stream, CancellationToken cancellationToken);
    }

}
