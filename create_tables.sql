-- CREATE TABLES
CREATE TABLE Role (
    RoleId INT NOT NULL PRIMARY KEY,
    RoleName NVARCHAR(15) NOT NULL
);

CREATE TABLE Employee (
    EmployeeId INT IDENTITY(1, 1) PRIMARY KEY,
    Email NVARCHAR(50) UNIQUE,
    Password NVARCHAR(100),
    RoleId INT NOT NULL
);

CREATE TABLE TicketStatus (
    StatusId INT NOT NULL PRIMARY KEY,
    StatusName NVARCHAR(50)
);

CREATE TABLE Ticket (
    TicketId VARCHAR(36) NOT NULL PRIMARY KEY,
    Reason NVARCHAR(25),
    Amount FLOAT(53) NOT NULL,
    Description NVARCHAR(250),
    StatusId INT NOT NULL,
	RequestDate DATETIME,
    EmployeeId INT NOT NULL
);

CREATE TABLE Session (
    SessionId VARCHAR(36) PRIMARY KEY, -- GUID
    EmployeeId INT NOT NULL UNIQUE,
    LastRequest DATETIME2 NOT NULL,
);

-- ADD FOREIGN KEYS
ALTER TABLE Employee ADD CONSTRAINT FK_EmployeeRoleId
    FOREIGN KEY (RoleId) REFERENCES Role (RoleId) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE Ticket ADD CONSTRAINT FK_TicketStatusId
    FOREIGN KEY (StatusId) REFERENCES TicketStatus (StatusId) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE Ticket ADD CONSTRAINT FK_TicketEmployeeId
    FOREIGN KEY (EmployeeId) REFERENCES Employee (EmployeeId) ON DELETE CASCADE ON UPDATE NO ACTION;
GO
ALTER TABLE Session ADD CONSTRAINT FK_EmployeeId
    FOREIGN KEY (EmployeeId) REFERENCES Employee (EmployeeId) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO