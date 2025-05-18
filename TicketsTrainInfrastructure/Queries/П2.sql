SELECT DISTINCT t.TrainName, rs.CityTown
FROM Ticket tk
JOIN Train t ON tk.Train_id = t.id
JOIN RailwayStation rs ON tk.DispatchStation_id = rs.id
WHERE rs.CityTown = @StartCity;
