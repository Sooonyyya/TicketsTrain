using System;
using System.Collections.Generic;

namespace TicketsTrainDomain.Model;

public partial class TrainAtRoute : Entity
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public int RouteId { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual Train Train { get; set; } = null!;
}
