/****** Script for SelectTopNRows command from SSMS  ******/
SELECT * INTO #APPS
  FROM [Auth].[AUTH].[Apps]

  CREATE TABLE [AUTH].[Apps] (
	AppId INT NOT NULL IDENTITY(1,1),
	AppName VARCHAR(100) NOT NULL
	PRIMARY KEY (AppId)
  )
  INSERT INTO [AUTH].[Apps]
  SELECT AppName FROM #APPS


  CREATE  TABLE [AUTH].[Permissions] (
	PermissionId int NOT NULL IDENTITY(1,1),
    PermissionName varchar(255) NOT NULL UNIQUE,
    PermissionType varchar(255) DEFAULT  'BASIC',
    PRIMARY KEY (PermissionId)
  )

  INSERT INTO [AUTH].[Permissions] VALUES ('modbase', 'BASIC')

    CREATE TABLE [AUTH].[AppPermissionGroup] (
	AppPermissionGroupId int NOT NULL ,
    PermissionId INT NOT NULL UNIQUE,
    AppId INT NOT NULL,
	AppPermissionGroupName VARCHAR(100) UNIQUE,
    PRIMARY KEY (AppPermissionGroupId,PermissionId),
	CONSTRAINT FK_PermissionId FOREIGN KEY (PermissionId)
    REFERENCES [AUTH].[Permissions](PermissionId),
	CONSTRAINT FK_AppId FOREIGN KEY (AppId)
    REFERENCES [AUTH].[Apps](AppId)
  )
    INSERT INTO [AUTH].[AppPermissionGroup] VALUES (1,1, 1, 'BASIC_PERMISSIONS')
	

CREATE TABLE [AUTH].[UserAppPermission] (
	UAPId int NOT NULL IDENTITY(1,1),
	UserId BIGINT NOT NULL,
	AppId INT NOT NULL,
	PermissionId INT NOT NULL,
	PRIMARY KEY (UAPId),
	CONSTRAINT FK_UserId FOREIGN KEY (UserId)
    REFERENCES [AUTH].[AppUsers](UserId),
	CONSTRAINT FK_AppId FOREIGN KEY (AppId)
    REFERENCES [AUTH].[Apps](AppId),
	CONSTRAINT FK_PermissionId FOREIGN KEY (PermissionId)
    REFERENCES [AUTH].[AppPermissions](PermissionId)
)
INSERT INTO [AUTH].[UserAppPermission] VALUES(1, 1, 1)
select * into #temp from auth.AppUsers

CREATE TABLE [AUTH].[AppUsers] (
	UserId BIGINT NOT NULL IDENTITY(1,1), 
	FirstName VARCHAR(100) NOT NULL,
	LastName VARCHAR(100) NOT NULL,
	Username VARCHAR(100) NOT NULL UNIQUE,
	Email VARCHAR(100) NOT NULL UNIQUE,
	[Password] VARCHAR(100) NOT NULL,
	Phone BIGINT ,
	Confirmed BIT DEFAULT 0,
	PRIMARY KEY (UserId),
)
INSERT INTO [AUTH].[AppUsers]
SELECT FirstName, LastName,	Username,	Email,	Password,	Phone,	Confirmed
FROM #temp
