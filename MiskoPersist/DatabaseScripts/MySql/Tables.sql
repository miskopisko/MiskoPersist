-- MySql Table Definitions

CREATE TABLE Operator 
(
    Id 			int(10) NOT NULL AUTO_INCREMENT,
    Username 	varchar(128) NOT NULL,
    Password 	varchar(128) NOT NULL,
    FirstName 	varchar(128) DEFAULT NULL,
    LastName 	varchar(128) DEFAULT NULL,
    DtCreated 	datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModified 	datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RowVer 		int(10) NOT NULL DEFAULT '0',
    PRIMARY KEY (Id)
) ENGINE=InnoDB AUTO_INCREMENT=1000001 DEFAULT CHARSET=utf8;
