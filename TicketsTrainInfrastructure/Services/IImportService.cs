﻿using TicketsTrainDomain.Model;
namespace TicketsTrainInfrastructure.Services
{
        public interface IImportService<TEntity>
       where TEntity : Entity
        {
            Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken);
        }

    }
