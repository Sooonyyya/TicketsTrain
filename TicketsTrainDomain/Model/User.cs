using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TicketsTrainDomain.Model;

public partial class User : Entity//IdentityUser
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введіть ім’я")]
    [StringLength(50, ErrorMessage = "Ім’я не повинно перевищувати 50 символів")]
    public string Name { get; set; } = null!;
    [NotMapped]
    [Required(ErrorMessage = "Введіть номер телефону")]
    [RegularExpression(@"^\d{7,10}$", ErrorMessage = "Номер телефону має містити від 7 до 10 цифр без +380")]
    public string? PhoneNumberString
    {
        get => PhoneNumber.ToString();
        set
        {
            if (int.TryParse(value, out var number))
                PhoneNumber = number;
        }
    }

    public int PhoneNumber { get; set; }

    [Required(ErrorMessage = "Введіть прізвище")]
    [StringLength(50, ErrorMessage = "Прізвище не повинно перевищувати 50 символів")]
    public string Surname { get; set; } = null!;
    [Required(ErrorMessage = "Введіть електронну пошту")]
    [EmailAddress(ErrorMessage = "Невірний формат електронної пошти")]
    public string Email { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
