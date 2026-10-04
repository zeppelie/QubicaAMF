USE CinemaCatalog;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Hall)
BEGIN
    INSERT INTO dbo.Hall (Name)
    VALUES (N'Hall 1'), (N'Hall 2');

    INSERT INTO dbo.Seat (HallId, RowLabel, SeatNumber)
    SELECT h.HallId, r.RowLabel, n.SeatNumber
    FROM dbo.Hall AS h
    CROSS JOIN (VALUES (N'A'), (N'B'), (N'C'), (N'D'), (N'E')) AS r (RowLabel)
    CROSS JOIN (VALUES (1), (2), (3), (4), (5), (6), (7), (8)) AS n (SeatNumber)
    WHERE h.Name = N'Hall 1';

    INSERT INTO dbo.Seat (HallId, RowLabel, SeatNumber)
    SELECT h.HallId, r.RowLabel, n.SeatNumber
    FROM dbo.Hall AS h
    CROSS JOIN (VALUES (N'A'), (N'B'), (N'C')) AS r (RowLabel)
    CROSS JOIN (VALUES (1), (2), (3), (4), (5), (6)) AS n (SeatNumber)
    WHERE h.Name = N'Hall 2';

    INSERT INTO dbo.Movie (Title, DurationMinutes)
    VALUES (N'The Big Lebowski', 117),
           (N'Blade Runner', 117),
           (N'Cinema Paradiso', 155);

    DECLARE @tomorrow DATETIME2(0) = DATEADD(DAY, 1, CAST(CAST(SYSUTCDATETIME() AS DATE) AS DATETIME2(0)));

    INSERT INTO dbo.Show (MovieId, HallId, StartsAt)
    SELECT m.MovieId, h.HallId, DATEADD(HOUR, s.StartHour, @tomorrow)
    FROM (VALUES (N'The Big Lebowski', N'Hall 1', 18),
                 (N'Blade Runner',     N'Hall 1', 21),
                 (N'Cinema Paradiso',  N'Hall 2', 20)) AS s (Title, HallName, StartHour)
    JOIN dbo.Movie AS m ON m.Title = s.Title
    JOIN dbo.Hall  AS h ON h.Name = s.HallName;
END
GO
