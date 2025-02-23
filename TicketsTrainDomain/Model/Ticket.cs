using System;
using System.Collections.Generic;

namespace TicketsTrainDomain.Model;

public partial class Ticket : Entity
{
    public int Id { get; set; }

    public int TrainId { get; set; }
    public int UserId { get; set; }

    public DateOnly DateOfTravel { get; set; }
    public DateOnly BookingDate { get; set; }
    public int TicketTypeTrainId { get; set; }

    public int ArrivalStationId { get; set; }

    public int DispatchStationId { get; set; }

    public virtual RailwayStation ArrivalStation { get; set; } = null!;

    public virtual RailwayStation DispatchStation { get; set; } = null!;

    public virtual TicketTypeTrain TicketTypeTrain { get; set; } = null!;

    public virtual Train Train { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
