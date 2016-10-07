-- MySql Table Definitions

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
