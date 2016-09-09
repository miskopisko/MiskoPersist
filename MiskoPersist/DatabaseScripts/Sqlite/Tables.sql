-- Operator - An uperator is an entity that owns accounts
DROP TABLE IF EXISTS 'Operator';
CREATE TABLE 'Operator'
( 
    Id				INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL,
    Username		VARCHAR(40),
    Password		VARCHAR(32),
	FirstName		VARCHAR(128),
	LastName		VARCHAR(128),
	DtCreated		DATETIME NOT NULL DEFAULT (DATETIME('NOW')),
    DtModified		DATETIME NOT NULL DEFAULT (DATETIME('NOW')),
    RowVer			INTEGER  NOT NULL DEFAULT (0) 
);
INSERT INTO SQLITE_SEQUENCE (NAME, SEQ) VALUES ('Operator', 1000000);