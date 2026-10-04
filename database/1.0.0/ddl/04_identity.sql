USE CinemaIdentity;
GO

IF OBJECT_ID(N'dbo.UserAccount', N'U') IS NULL
CREATE TABLE dbo.UserAccount
(
    UserId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserAccount PRIMARY KEY,
    UserName     NVARCHAR(50)      NOT NULL CONSTRAINT UQ_UserAccount_UserName UNIQUE,
    PasswordHash NVARCHAR(200)     NOT NULL,
    Role         NVARCHAR(20)      NOT NULL CONSTRAINT DF_UserAccount_Role DEFAULT N'Customer',
    CONSTRAINT CK_UserAccount_Role CHECK (Role IN (N'Customer', N'Admin'))
);
GO
