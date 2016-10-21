-- MySql Table Definitions

DROP TABLE IF EXISTS Operator;
CREATE TABLE Operator 
(
    Id 						int(10) NOT NULL AUTO_INCREMENT,
    Username 				varchar(128) NOT NULL,
    Password 				varchar(128) NOT NULL,
    FirstName 				varchar(128) DEFAULT NULL,
    LastName 				varchar(128) DEFAULT NULL,    
	LastLoginDate 			datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	LastLoginAttempt 		datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PasswordChangeDate 		datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Disabled 				int(1) NOT NULL DEFAULT '0',
	LockedOut 				int(1) NOT NULL DEFAULT '0',
	PasswordNeverExpires 	int(1) NOT NULL DEFAULT '0',
	PasswordExpired 		int(1) NOT NULL DEFAULT '0',
	LoginAttempts 			int(5) NOT NULL DEFAULT '0', 
    DtCreated 				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModified 				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RowVer 					int(10) NOT NULL DEFAULT '0',
    PRIMARY KEY (Id)
) ENGINE=InnoDB AUTO_INCREMENT=1000001 DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS SessionLog;
CREATE TABLE SessionLog 
(
	Id              	int(10) NOT NULL AUTO_INCREMENT,
	SessionToken    	varchar(36) NOT NULL,
	Operator        	int(10) NOT NULL,
	LoggedOn        	datetime NOT NULL,
	LoggedOff       	datetime,
	LastTransmitted 	datetime NOT NULL,
	Status 				int(3) NOT NULL,
	DtCreated 			datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModified 			datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RowVer 				int(10) NOT NULL DEFAULT '0',
    PRIMARY KEY (Id),
    KEY Operator (Operator),
    CONSTRAINT FK_SessionLogOperator FOREIGN KEY (Operator) REFERENCES Operator (Id)
) ENGINE=InnoDB AUTO_INCREMENT=1000001 DEFAULT CHARSET=utf8;