DECLARE @UserId INT;

SELECT @UserId = u.id
FROM [User] u
WHERE u.Name = @UserName AND u.Surname = @UserSurname;

SELECT DISTINCT u2.Name, u2.Surname, u2.Email
FROM [User] u2
WHERE u2.id <> @UserId
AND NOT EXISTS (
    SELECT DISTINCT tt.Name
    FROM Ticket t
    JOIN TicketType_Train ttt ON t.TicketType_Train_id = ttt.id
    JOIN TicketType tt ON ttt.TicketType_id = tt.id
    WHERE t.User_id = @UserId

    EXCEPT

    SELECT DISTINCT tt2.Name
    FROM Ticket t2
    JOIN TicketType_Train ttt2 ON t2.TicketType_Train_id = ttt2.id
    JOIN TicketType tt2 ON ttt2.TicketType_id = tt2.id
    WHERE t2.User_id = u2.id
)
AND NOT EXISTS (
    SELECT DISTINCT tt2.Name
    FROM Ticket t2
    JOIN TicketType_Train ttt2 ON t2.TicketType_Train_id = ttt2.id
    JOIN TicketType tt2 ON ttt2.TicketType_id = tt2.id
    WHERE t2.User_id = u2.id

    EXCEPT

    SELECT DISTINCT tt.Name
    FROM Ticket t
    JOIN TicketType_Train ttt ON t.TicketType_Train_id = ttt.id
    JOIN TicketType tt ON ttt.TicketType_id = tt.id
    WHERE t.User_id = @UserId
)
