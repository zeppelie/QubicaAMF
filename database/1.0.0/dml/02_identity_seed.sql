USE CinemaIdentity;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.UserAccount WHERE UserName = N'admin')
    INSERT INTO dbo.UserAccount (UserName, PasswordHash, Role)
    VALUES (N'admin', N'AQAAAAIAAYagAAAAEF6/mNOo2NzdvLQQ2c+1P9hs0Gq0uXJ+HjeIVENHDWk4zu4FMd/sCybNKKAbHpQing==', N'Admin');
GO
