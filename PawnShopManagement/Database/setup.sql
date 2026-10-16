IF DB_ID(N'PawnShopDB') IS NULL CREATE DATABASE PawnShopDB;
GO
USE PawnShopDB;
GO
IF OBJECT_ID(N'dbo.Customers',N'U') IS NULL
CREATE TABLE dbo.Customers (
 CustomerID int IDENTITY PRIMARY KEY, FullName nvarchar(120) NOT NULL,
 IdentityCard nvarchar(12) NOT NULL, Phone nvarchar(10) NOT NULL, Address nvarchar(250) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.PawnContracts',N'U') IS NULL
CREATE TABLE dbo.PawnContracts (
 ContractID int IDENTITY PRIMARY KEY, CustomerID int NOT NULL,
 AssetName nvarchar(150) NOT NULL, AssetDetails nvarchar(2000) NULL,
 LoanAmount decimal(18,2) NOT NULL, InterestRate real NOT NULL,
 StartDate datetime2 NOT NULL, Status nvarchar(20) NOT NULL DEFAULT N'DangCam',
 CONSTRAINT FK_PawnContracts_Customers FOREIGN KEY(CustomerID) REFERENCES dbo.Customers(CustomerID),
 CONSTRAINT CK_PawnContracts_Loan CHECK(LoanAmount BETWEEN 500000 AND 1000000000),
 CONSTRAINT CK_PawnContracts_Rate CHECK(InterestRate BETWEEN 0 AND 100),
 CONSTRAINT CK_PawnContracts_Status CHECK(Status IN (N'DangCam',N'DaChuoc',N'DaThanhLy'))
);
GO
IF OBJECT_ID(N'dbo.AppUsers',N'U') IS NULL
CREATE TABLE dbo.AppUsers (
 Id int IDENTITY PRIMARY KEY, UserName nvarchar(50) NOT NULL,
 PasswordHash nvarchar(max) NOT NULL, Role nvarchar(20) NOT NULL,
 FailedLoginCount int NOT NULL DEFAULT 0, LockoutUntil datetime2 NULL,
 CONSTRAINT CK_AppUsers_Role CHECK(Role IN (N'Admin',N'Staff'))
);
GO
IF COL_LENGTH('dbo.AppUsers','FailedLoginCount') IS NULL
 ALTER TABLE dbo.AppUsers ADD FailedLoginCount int NOT NULL DEFAULT 0;
IF COL_LENGTH('dbo.AppUsers','LockoutUntil') IS NULL
 ALTER TABLE dbo.AppUsers ADD LockoutUntil datetime2 NULL;
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_AppUsers_UserName' AND object_id=OBJECT_ID(N'dbo.AppUsers'))
 CREATE UNIQUE INDEX IX_AppUsers_UserName ON dbo.AppUsers(UserName);
-- Stop with a useful message rather than silently removing duplicate customer records.
IF EXISTS(SELECT IdentityCard FROM dbo.Customers GROUP BY IdentityCard HAVING COUNT(*)>1)
 THROW 50001, N'Duplicate IdentityCard values: review customer records before creating the unique index.',1;
IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Customers') AND name=N'IdentityCard' AND max_length=-1)
 ALTER TABLE dbo.Customers ALTER COLUMN IdentityCard nvarchar(12) NOT NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_Customers_IdentityCard' AND object_id=OBJECT_ID(N'dbo.Customers'))
 CREATE UNIQUE INDEX IX_Customers_IdentityCard ON dbo.Customers(IdentityCard);
GO
-- No default credentials and no sample personal data are inserted.

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_PawnContracts_CustomerID' AND object_id=OBJECT_ID(N'dbo.PawnContracts'))
 CREATE INDEX IX_PawnContracts_CustomerID ON dbo.PawnContracts(CustomerID);
GO

-- Existing records have an unknown term until staff explicitly set it.
IF COL_LENGTH('dbo.PawnContracts','TermDays') IS NULL
 ALTER TABLE dbo.PawnContracts ADD TermDays int NULL;
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_PawnContracts_TermDays' AND parent_object_id=OBJECT_ID(N'dbo.PawnContracts'))
 ALTER TABLE dbo.PawnContracts WITH CHECK ADD CONSTRAINT CK_PawnContracts_TermDays CHECK(TermDays IS NULL OR TermDays BETWEEN 1 AND 3650);
GO
