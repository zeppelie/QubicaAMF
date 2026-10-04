USE CinemaCatalog;
GO

IF OBJECT_ID(N'dbo.Hall', N'U') IS NULL
CREATE TABLE dbo.Hall
(
    HallId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Hall PRIMARY KEY,
    Name   NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Hall_Name UNIQUE
);
GO

IF OBJECT_ID(N'dbo.Seat', N'U') IS NULL
CREATE TABLE dbo.Seat
(
    SeatId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Seat PRIMARY KEY,
    HallId     INT               NOT NULL CONSTRAINT FK_Seat_Hall REFERENCES dbo.Hall (HallId),
    RowLabel   NVARCHAR(2)       NOT NULL,
    SeatNumber INT               NOT NULL,
    CONSTRAINT UQ_Seat_Position UNIQUE (HallId, RowLabel, SeatNumber)
);
GO

IF OBJECT_ID(N'dbo.Movie', N'U') IS NULL
CREATE TABLE dbo.Movie
(
    MovieId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Movie PRIMARY KEY,
    Title           NVARCHAR(200)     NOT NULL,
    DurationMinutes INT               NOT NULL CONSTRAINT CK_Movie_Duration CHECK (DurationMinutes > 0)
);
GO

IF OBJECT_ID(N'dbo.Show', N'U') IS NULL
CREATE TABLE dbo.Show
(
    ShowId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Show PRIMARY KEY,
    MovieId  INT               NOT NULL CONSTRAINT FK_Show_Movie REFERENCES dbo.Movie (MovieId),
    HallId   INT               NOT NULL CONSTRAINT FK_Show_Hall REFERENCES dbo.Hall (HallId),
    StartsAt DATETIME2(0)      NOT NULL,
    CONSTRAINT UQ_Show_Hall_StartsAt UNIQUE (HallId, StartsAt)
);
GO
