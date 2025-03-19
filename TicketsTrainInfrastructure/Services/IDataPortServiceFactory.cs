using TicketsTrainDomain.Model;
namespace TicketsTrainInfrastructure.Services
{
    public interface IDataPortServiceFactory<TEntity>
   where TEntity : Entity
    {
        IImportService<TEntity> GetImportService(string contentType);
        IExportService<TEntity> GetExportService(string contentType);
    }

}
