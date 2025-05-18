using System;
using System.Collections.Generic;

namespace TicketsTrainDomain.Model
{
    public class Query
    {
        public string QueryName { get; set; } = string.Empty;

        // Параметри для запитів
        public string? CityFrom { get; set; }          // @StartCity
        public string? CityTo { get; set; }            // @EndCity
        public DateTime? TravelDate { get; set; }      // @TravelDate
        public int? TrainId { get; set; }              // використовується як число у П1, П5
        public string? TicketTypeName { get; set; }    // може бути назва типу
        public int? UserId { get; set; }               // якщо буде потрібен
        public string? TrainName { get; set; }         // @TrainName (С1, С3)
        public string? UserName { get; set; }
        public string? UserSurname { get; set; }


        // Для результату
        public int ErrorFlag { get; set; } = 0;
        public string ErrorName { get; set; } = string.Empty;

        public List<string> TrainNames { get; set; } = new();
        public List<string> TicketTypeNames { get; set; } = new();
        public List<string> CustomerNames { get; set; } = new();
        public List<string> CustomerEmails { get; set; } = new();

    }
}
