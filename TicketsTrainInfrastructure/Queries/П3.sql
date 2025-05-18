SELECT DISTINCT u.Name, u.Surname, u.Email
FROM Ticket tk
JOIN [User] u ON tk.User_id = u.id
WHERE tk.DateOfTravel = @TravelDate
