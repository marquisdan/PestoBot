--
-- File generated with SQLiteStudio v3.2.1 on Tue Jul 28 10:11:44 2020
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: DebugPerson
DROP TABLE IF EXISTS DebugPerson;

CREATE TABLE DebugPerson (
    Id        INTEGER NOT NULL,
    FirstName TEXT    NOT NULL,
    LastName  TEXT    NOT NULL,
    CONSTRAINT PK_Person PRIMARY KEY (
        Id
    )
);


-- Table: Event
DROP TABLE IF EXISTS Event;

CREATE TABLE Event (
    Id                INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created           DATETIME,
    Modified          DATETIME,
    Name              STRING   UNIQUE,
    StartDate         DATETIME,
    EndDate           DATETIME,
    ScheduleCloseDate DATETIME,
    ScheduleUrl       STRING,
    ApplicationUrl    STRING,
    Charity           STRING,
    CharityUrl        STRING,
    DonationUrl       STRING,
    CreatorId         BIGINT,
    CreatorUsername   STRING,
    GuildId           BIGINT   REFERENCES Guild (Id) 
);


-- Table: EventAssignment
DROP TABLE IF EXISTS EventAssignment;

CREATE TABLE EventAssignment (
    Id               INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created          DATETIME,
    Modified         DATETIME,
    AssignmentType   INTEGER,
    ProjectDueDate   DATETIME,
    TaskStartTime    DATETIME,
    ReminderText     TEXT,
    LastReminderSent DATETIME,
    ProjectTaskId    BIGINT,
    GuildId          BIGINT   REFERENCES Guild (Id),
    UserId           BIGINT   REFERENCES User (Id),
    EventId          BIGINT   REFERENCES Event (Id) 
);


-- Table: EventTask
DROP TABLE IF EXISTS EventTask;

CREATE TABLE EventTask (
    Id          INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created     DATETIME,
    Modified    DATETIME,
    Name        STRING,
    Description STRING,
    GuildId     BIGINT   REFERENCES Guild (Id),
    EventId     INTEGER  REFERENCES Event (Id) 
);


-- Table: EventVolunteerAssignment
DROP TABLE IF EXISTS EventVolunteerAssignment;

CREATE TABLE EventVolunteerAssignment (
    Id       INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created  DATETIME,
    Modified DATETIME,
    GuildId  BIGINT   REFERENCES Guild (Id),
    EventId  INTEGER  REFERENCES Event (Id),
    UserId   BIGINT   REFERENCES User (Id) 
);


-- Table: GlobalSettings
DROP TABLE IF EXISTS GlobalSettings;

CREATE TABLE GlobalSettings (
    Id                    INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created               DATETIME,
    Modified              DATETIME,
    DebugRemindersEnabled BOOLEAN  DEFAULT (0),
    DebugReminderHour     INTEGER,
    DebugReminderMinutes  INTEGER
);


-- Table: Guild
DROP TABLE IF EXISTS Guild;

CREATE TABLE Guild (
    Id             BIGINT   PRIMARY KEY
                            NOT NULL
                            UNIQUE,
    Created        DATETIME,
    Modified       DATETIME,
    JoinDate       DATETIME,
    Name           STRING,
    OwnerUsername  STRING,
    LastConnection DATETIME,
    OwnerId        BIGINT
);


-- Table: GuildSettings
DROP TABLE IF EXISTS GuildSettings;

CREATE TABLE GuildSettings (
    Id                     INTEGER  PRIMARY KEY AUTOINCREMENT
                                    NOT NULL
                                    UNIQUE,
    Created                DATETIME,
    Modified               DATETIME,
    Prefix                 STRING   DEFAULT ('!'),
    ProjectReminderChannel BIGINT,
    TaskReminderChannel    BIGINT,
    RunnerReminderChannel  BIGINT,
    DebugReminderChannel   BIGINT,
    GuildId                BIGINT   REFERENCES Guild (Id) 
);


-- Table: ServerAdminAssignment
DROP TABLE IF EXISTS ServerAdminAssignment;

CREATE TABLE ServerAdminAssignment (
    Id       INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created  DATETIME,
    Modified DATETIME,
    GuildId  BIGINT   REFERENCES Guild (Id),
    UserId   BIGINT   REFERENCES User (Id) 
);


-- Table: User
DROP TABLE IF EXISTS User;

CREATE TABLE User (
    Id          BIGINT   PRIMARY KEY
                         UNIQUE
                         NOT NULL,
    Created     DATETIME,
    Modified    DATETIME,
    Username    STRING,
    DiscordName STRING,
    IsVolunteer BOOLEAN
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
