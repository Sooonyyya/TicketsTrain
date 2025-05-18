SELECT DISTINCT t2.TrainName
FROM Train t2
WHERE t2.TrainName <> @TrainName
AND NOT EXISTS (
    SELECT sar1.RailwayStation_id
    FROM Train t1
    JOIN TrainAtRoute tar1 ON t1.id = tar1.Train_id
    JOIN StationAtRoute sar1 ON tar1.Route_id = sar1.Route_id
    WHERE t1.TrainName = @TrainName
    AND NOT EXISTS (
        SELECT 1
        FROM TrainAtRoute tar2
        JOIN StationAtRoute sar2 ON tar2.Route_id = sar2.Route_id
        WHERE tar2.Train_id = t2.id
        AND sar2.RailwayStation_id = sar1.RailwayStation_id
    )
)
AND NOT EXISTS (
    SELECT sar2.RailwayStation_id
    FROM TrainAtRoute tar2
    JOIN StationAtRoute sar2 ON tar2.Route_id = sar2.Route_id
    WHERE tar2.Train_id = t2.id
    AND NOT EXISTS (
        SELECT 1
        FROM Train t1
        JOIN TrainAtRoute tar1 ON t1.id = tar1.Train_id
        JOIN StationAtRoute sar1 ON tar1.Route_id = sar1.Route_id
        WHERE t1.TrainName = @TrainName
        AND sar1.RailwayStation_id = sar2.RailwayStation_id
    )
);
