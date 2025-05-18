using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketsTrainDomain.Model;

namespace TicketsTrainInfrastructure.Controllers
{
    public class QueriesController : Controller
    {
        private const string CONNECTION = "Server=SofiaHP\\SQLEXPRESS; Database=TicketsTrain; Trusted_Connection=True; TrustServerCertificate=True; MultipleActiveResultSets=True";

        private readonly Dictionary<string, string> QueryPaths = new()
        {
            ["П1"] = "Queries/П1.sql",
            ["П2"] = "Queries/П2.sql",
            ["П3"] = "Queries/П3.sql",
            ["П4"] = "Queries/П4.sql",
            ["П5"] = "Queries/П5.sql",
            ["С1"] = "Queries/С1.sql",
            ["С2"] = "Queries/С2.sql",
            ["С3"] = "Queries/С3.sql",
        };

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Execute(Query model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            if (!QueryPaths.TryGetValue(model.QueryName, out var path))
            {
                model.ErrorFlag = 1;
                model.ErrorName = "Невідомий запит.";
                return View("Result", model);
            }

            string query = System.IO.File.ReadAllText(path);

            try
            {
                using var conn = new SqlConnection(CONNECTION);
                conn.Open();
                using var cmd = new SqlCommand(query, conn);

                if (query.Contains("@StartCity") && !string.IsNullOrEmpty(model.CityFrom))
                    cmd.Parameters.AddWithValue("@StartCity", model.CityFrom);

                if (query.Contains("@EndCity") && !string.IsNullOrEmpty(model.CityTo))
                    cmd.Parameters.AddWithValue("@EndCity", model.CityTo);

                if (query.Contains("@TravelDate") && model.TravelDate.HasValue)
                    cmd.Parameters.AddWithValue("@TravelDate", model.TravelDate.Value);

                if (query.Contains("@TrainId") && model.TrainId.HasValue)
                    cmd.Parameters.AddWithValue("@TrainId", model.TrainId.Value);

                if (query.Contains("@TicketTypeName") && !string.IsNullOrEmpty(model.TicketTypeName))
                    cmd.Parameters.AddWithValue("@TicketTypeName", model.TicketTypeName);

                if (query.Contains("@UserId") && model.UserId.HasValue)
                    cmd.Parameters.AddWithValue("@UserId", model.UserId.Value);

                if (query.Contains("@TrainName") && !string.IsNullOrEmpty(model.TrainName))
                    cmd.Parameters.AddWithValue("@TrainName", model.TrainName);

                if (query.Contains("@City") && !string.IsNullOrEmpty(model.CityFrom))
                    cmd.Parameters.AddWithValue("@City", model.CityFrom);
                if (query.Contains("@UserName") && !string.IsNullOrEmpty(model.UserName))
                    cmd.Parameters.AddWithValue("@UserName", model.UserName);

                if (query.Contains("@UserSurname") && !string.IsNullOrEmpty(model.UserSurname))
                    cmd.Parameters.AddWithValue("@UserSurname", model.UserSurname);


                using var reader = cmd.ExecuteReader();

                switch (model.QueryName)
                {
                    case "П1":
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            string surname = reader.GetString(1);
                            model.CustomerNames.Add($"{name} {surname}");
                        }
                        break;

                    case "П4":
                    case "П5":
                        while (reader.Read())
                        {
                            model.TrainNames.Add(reader.GetString(0));
                        }
                        break;




                    case "П2":
                        while (reader.Read())
                            model.TicketTypeNames.Add(reader.GetString(0));
                        break;

                    case "П3":
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            string surname = reader.GetString(1);
                            string email = reader.GetString(2);
                            model.CustomerNames.Add(name + " " + surname);
                            model.CustomerEmails.Add(email);
                        }
                        break;

                    case "С1":
                        while (reader.Read())
                        {
                            model.CustomerNames.Add(reader.GetString(0));   // u.Name
                            model.CustomerEmails.Add(reader.GetString(1));  // u.Email
                        }
                        break;

                    case "С2":
                        while (reader.Read())
                        {
                            model.CustomerNames.Add(reader.GetString(0) + " " + reader.GetString(1));
                            model.CustomerEmails.Add(reader.GetString(2));
                        }
                        break;


                    case "С3":
                        while (reader.Read())
                        {
                            model.TrainNames.Add(reader.GetString(0)); // перша колонка — назва потяга
                        }
                        break;

                }

                if (model.TrainNames.Count == 0 && model.TicketTypeNames.Count == 0 && model.CustomerNames.Count == 0)
                {
                    model.ErrorFlag = 1;
                    model.ErrorName = "Немає результатів за обраним запитом.";
                }
            }
            catch (Exception ex)
            {
                model.ErrorFlag = 1;
                model.ErrorName = ex.Message;
            }

            return View("Result", model);
        }
    }
}
