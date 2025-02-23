using System;
using System.Collections.Generic;


namespace TicketsTrainDomain.Model;

public partial class TicketTypeTrain : Entity
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public int TicketTypeId { get; set; }

    public virtual TicketType TicketType { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual Train Train { get; set; } = null!;
}
