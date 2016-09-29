-- Operator - An uperator is an entity that owns accounts
DROP TABLE IF EXISTS Operator;
CREATE TABLE Operator
(
    Id         				INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
    Username   				VARCHAR (40),
    Password   				VARCHAR (128),
    FirstName  				VARCHAR (128),
    LastName   				VARCHAR (128),
    LastLoginDate        	DATETIME NOT NULL DEFAULT (DATETIME('NOW') ),
    LastLoginAttempt     	DATETIME NOT NULL DEFAULT (DATETIME('NOW') ),
    PasswordChangeDate		DATETIME NOT NULL DEFAULT (DATETIME('NOW') ),
    Disabled             	INT NOT NULL DEFAULT (0),
    LockedOut            	INT NOT NULL DEFAULT (0),
    PasswordNeverExpires	INT NOT NULL DEFAULT (0),
    PasswordExpired      	INT NOT NULL DEFAULT (0),
    LoginAttempts        	INTEGER NOT NULL DEFAULT (0),
    DtCreated  				DATETIME NOT NULL DEFAULT (DATETIME('NOW')),
    DtModified 				DATETIME NOT NULL DEFAULT (DATETIME('NOW')),
    RowVer     				INTEGER NOT NULL DEFAULT (0) 
);
INSERT INTO SQLITE_SEQUENCE (NAME, SEQ) VALUES ('Operator', 1000000);