using System;
using System.Collections.Generic;

namespace TicketsTrainDomain.Model;

public partial class RailwayStation : Entity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string CityTown { get; set; } = null!;

    public string Country { get; set; } = null!;

    public virtual ICollection<StationAtRoute> StationAtRoutes { get; set; } = new List<StationAtRoute>();

    public virtual ICollection<Ticket> TicketArrivalStations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketDispatchStations { get; set; } = new List<Ticket>();
}
