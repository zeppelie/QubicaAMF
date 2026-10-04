USE CinemaBooking;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Booking', N'U') IS NULL
CREATE TABLE dbo.Booking
(
    BookingId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Booking PRIMARY KEY,
    UserId      INT               NOT NULL,
    CreatedAt   DATETIME2(0)      NOT NULL CONSTRAINT DF_Booking_CreatedAt DEFAULT SYSUTCDATETIME(),
    CancelledAt DATETIME2(0)      NULL
);
GO

IF OBJECT_ID(N'dbo.BookedSeat', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookedSeat
    (
        BookedSeatId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BookedSeat PRIMARY KEY,
        BookingId    INT               NOT NULL CONSTRAINT FK_BookedSeat_Booking REFERENCES dbo.Booking (BookingId),
        ShowId       INT               NOT NULL,
        SeatId       INT               NOT NULL,
        CancelledAt  DATETIME2(0)      NULL
    );

    CREATE UNIQUE INDEX UX_BookedSeat_Show_Seat
        ON dbo.BookedSeat (ShowId, SeatId)
        WHERE CancelledAt IS NULL;

    CREATE INDEX IX_BookedSeat_Booking ON dbo.BookedSeat (BookingId);
END
GO
