SELECT u2.Name, u2.Email
FROM [User] u2
WHERE u2.id <> (
    SELECT id FROM [User]
    WHERE Name = @UserName AND Surname = @UserSurname
)
AND NOT EXISTS (
    SELECT tk1.Train_id
    FROM Ticket tk1
    JOIN [User] u1 ON u1.id = tk1.User_id
    WHERE u1.Name = @UserName AND u1.Surname = @UserSurname
    AND NOT EXISTS (
        SELECT 1
        FROM Ticket tk2
        WHERE tk2.User_id = u2.id
        AND tk2.Train_id = tk1.Train_id
    )
);
