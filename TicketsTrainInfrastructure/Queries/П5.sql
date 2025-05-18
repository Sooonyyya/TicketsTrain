SELECT DISTINCT t.TrainName
FROM Ticket tk
JOIN Train t ON tk.Train_id = t.id
JOIN TicketType_Train ttt ON tk.TicketType_Train_id = ttt.id
JOIN TicketType tt ON ttt.TicketType_id = tt.id
WHERE tt.Name = @TicketTypeName
