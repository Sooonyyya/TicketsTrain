using System;
using System.Collections.Generic;

namespace TicketsTrainDomain.Model;

public partial class Route : Entity
{
    public int Id { get; set; }

    public string StartStation { get; set; } = null!;

    public string EndStation { get; set; } = null!;

    public virtual ICollection<StationAtRoute> StationAtRoutes { get; set; } = new List<StationAtRoute>();

    public virtual ICollection<TrainAtRoute> TrainAtRoutes { get; set; } = new List<TrainAtRoute>();
}
