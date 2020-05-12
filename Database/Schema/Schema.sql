--
-- File generated with SQLiteStudio v3.2.1 on Tue May 12 15:53:28 2020
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
    GuildId           BIGINT   REFERENCES Guild (Id) 
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
    GuildId                BIGINT   REFERENCES Guild (Id) 
);


-- Table: MarathonProject
DROP TABLE IF EXISTS MarathonProject;

CREATE TABLE MarathonProject (
    Id               INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created          DATETIME,
    Modified         DATETIME,
    GuildId          BIGINT   REFERENCES Guild (Id),
    EventId          INTEGER  REFERENCES EventVolunteerAssignment (Id),
    Name             STRING,
    Description      STRING,
    DueDate          DATETIME,
    ReminderText     STRING,
    LastReminderSent DATETIME
);


-- Table: MarathonProjectAssignment
DROP TABLE IF EXISTS MarathonProjectAssignment;

CREATE TABLE MarathonProjectAssignment (
    Id                INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created           DATETIME,
    Modified          DATETIME,
    GuildId           BIGINT   REFERENCES Guild (Id),
    UserId            BIGINT   REFERENCES User (Id),
    MarathonProjectId INTEGER  REFERENCES MarathonProject (Id) 
);


-- Table: MarathonTask
DROP TABLE IF EXISTS MarathonTask;

CREATE TABLE MarathonTask (
    Id          INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created     DATETIME,
    Modified    DATETIME,
    Name        STRING,
    Description STRING,
    GuildId     BIGINT   REFERENCES Guild (Id),
    EventId     INTEGER  REFERENCES Event (Id) 
);


-- Table: MarathonTaskAssignment
DROP TABLE IF EXISTS MarathonTaskAssignment;

CREATE TABLE MarathonTaskAssignment (
    Id             INTEGER  PRIMARY KEY AUTOINCREMENT,
    Created        DATETIME,
    Modified       DATETIME,
    TaskStartTime  DATETIME,
    GuildId        BIGINT   REFERENCES Guild (Id),
    UserId         BIGINT   REFERENCES User (Id),
    MarathonTaskId INTEGER  REFERENCES MarathonTask (Id) 
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
