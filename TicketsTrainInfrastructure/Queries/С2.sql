SELECT DISTINCT u2.Name, u2.Surname, u2.Email
FROM [User] u2
WHERE u2.id <> (
    SELECT id FROM [User]
    WHERE Name = @UserName AND Surname = @UserSurname
)
AND NOT EXISTS (
    SELECT ttt.TicketType_id
    FROM Ticket tk1
    JOIN TicketType_Train ttt ON tk1.TicketType_Train_id = ttt.id
    WHERE tk1.User_id = (
        SELECT id FROM [User]
        WHERE Name = @UserName AND Surname = @UserSurname
    )
    EXCEPT
    SELECT ttt2.TicketType_id
    FROM Ticket tk2
    JOIN TicketType_Train ttt2 ON tk2.TicketType_Train_id = ttt2.id
    WHERE tk2.User_id = u2.id
)
