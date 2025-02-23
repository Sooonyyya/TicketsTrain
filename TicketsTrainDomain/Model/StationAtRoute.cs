using System;
using System.Collections.Generic;

namespace TicketsTrainDomain.Model;

public partial class StationAtRoute : Entity
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public int RailwayStationId { get; set; }

    public virtual RailwayStation RailwayStation { get; set; } = null!;

    public virtual Route Route { get; set; } = null!;
}
