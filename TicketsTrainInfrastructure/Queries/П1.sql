SELECT DISTINCT u.Name, u.Surname
FROM Ticket tk
JOIN [User] u ON tk.User_id = u.id
JOIN Train t ON tk.Train_id = t.id
WHERE t.TrainName = @TrainName
