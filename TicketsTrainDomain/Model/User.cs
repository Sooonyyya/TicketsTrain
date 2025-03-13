using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TicketsTrainDomain.Model;

public partial class User : Entity//IdentityUser
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int PhoneNumber { get; set; }

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
