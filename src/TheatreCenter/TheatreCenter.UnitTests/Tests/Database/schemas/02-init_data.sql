DROP TABLE IF EXISTS "AccountActorFavorites" CASCADE;
DROP TABLE IF EXISTS "AccountMusicalFavorites" CASCADE;
DROP TABLE IF EXISTS "AccountTheatreFavorites" CASCADE;
DROP TABLE IF EXISTS "CastMembers" CASCADE;
DROP TABLE IF EXISTS "ActorRoles" CASCADE;
DROP TABLE IF EXISTS "MusicalThemes" CASCADE;
DROP TABLE IF EXISTS "Shows" CASCADE;
DROP TABLE IF EXISTS "Roles" CASCADE;
DROP TABLE IF EXISTS "Musicals" CASCADE;
DROP TABLE IF EXISTS "Themes" CASCADE;
DROP TABLE IF EXISTS "Actors" CASCADE;
DROP TABLE IF EXISTS "Theatres" CASCADE;
DROP TABLE IF EXISTS "Accounts" CASCADE;


CREATE TABLE "Accounts" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "AccessLevel" TEXT NOT NULL,
    "LastFavoritesViewDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpgradeRequest" BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE "Actors" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "VoiceType" TEXT NULL,
    "Gender" TEXT NOT NULL,
    "BirthDate" DATE NOT NULL,
    "Height" INT NULL,
    "Weight" INT NULL,
    "AddInfo" TEXT NULL
);

CREATE TABLE "Theatres" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "AddInfo" TEXT NULL
);

CREATE TABLE "Musicals" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(100) NOT NULL,
    "Description" TEXT NOT NULL,
    "Duration" INTERVAL NOT NULL,
    "AgeRestriction" TEXT NOT NULL,
    "TheatreId" INT NOT NULL REFERENCES "Theatres"("Id") ON DELETE RESTRICT
);

CREATE TABLE "Roles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL,
    "MusicalId" INT NOT NULL REFERENCES "Musicals"("Id") ON DELETE CASCADE,
    "RoleType" VARCHAR(20) NOT NULL
);

CREATE TABLE "Shows" (
    "Id" SERIAL PRIMARY KEY,
    "Date" TIMESTAMPTZ NOT NULL,
    "MusicalId" INT NOT NULL REFERENCES "Musicals"("Id") ON DELETE CASCADE
);

CREATE TABLE "Themes" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL
);

CREATE TABLE "CastMembers" (
    "Id" SERIAL PRIMARY KEY,
    "ShowId" INT NOT NULL REFERENCES "Shows"("Id") ON DELETE CASCADE,
    "RoleId" INT NOT NULL REFERENCES "Roles"("Id") ON DELETE RESTRICT,
    "ActorId" INT NOT NULL REFERENCES "Actors"("Id") ON DELETE RESTRICT,
    "Comment" VARCHAR(200) NULL
);

-- Many-to-many tables
CREATE TABLE "ActorRoles" (
    "ActorId" INT NOT NULL REFERENCES "Actors"("Id") ON DELETE CASCADE,
    "RoleId" INT NOT NULL REFERENCES "Roles"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("ActorId", "RoleId")
);

CREATE TABLE "MusicalThemes" (
    "MusicalId" INT NOT NULL REFERENCES "Musicals"("Id") ON DELETE CASCADE,
    "ThemeId" INT NOT NULL REFERENCES "Themes"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("MusicalId", "ThemeId")
);

CREATE TABLE "AccountTheatreFavorites" (
    "AccountId" INT NOT NULL REFERENCES "Accounts"("Id") ON DELETE CASCADE,
    "TheatreId" INT NOT NULL REFERENCES "Theatres"("Id") ON DELETE CASCADE,
    "AddedDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY ("AccountId", "TheatreId")
);

CREATE TABLE "AccountMusicalFavorites" (
    "AccountId" INT NOT NULL REFERENCES "Accounts"("Id") ON DELETE CASCADE,
    "MusicalId" INT NOT NULL REFERENCES "Musicals"("Id") ON DELETE CASCADE,
    "AddedDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY ("AccountId", "MusicalId")
);

CREATE TABLE "AccountActorFavorites" (
    "AccountId" INT NOT NULL REFERENCES "Accounts"("Id") ON DELETE CASCADE,
    "ActorId" INT NOT NULL REFERENCES "Actors"("Id") ON DELETE CASCADE,
    "AddedDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY ("AccountId", "ActorId")
);

-- Create indexes for performance
CREATE UNIQUE INDEX "IX_Accounts_Username" ON "Accounts"("Username");
CREATE INDEX "IX_Shows_Date" ON "Shows"("Date");
CREATE INDEX "IX_Shows_MusicalId" ON "Shows"("MusicalId");
CREATE INDEX "IX_Roles_MusicalId" ON "Roles"("MusicalId");
CREATE INDEX "IX_Musicals_TheatreId" ON "Musicals"("TheatreId");
CREATE INDEX "IX_CastMembers_ShowId" ON "CastMembers"("ShowId");
CREATE INDEX "IX_CastMembers_ActorId" ON "CastMembers"("ActorId");
CREATE INDEX "IX_CastMembers_RoleId" ON "CastMembers"("RoleId");
CREATE UNIQUE INDEX "IX_Themes_Name" ON "Themes"("Name");
CREATE UNIQUE INDEX "IX_Theatres_Name" ON "Theatres"("Name");

-- Insert test accounts
INSERT INTO "Accounts" ("Username", "PasswordHash", "AccessLevel", "LastFavoritesViewDate", "UpgradeRequest")
VALUES 
('admin', 'fake_hash_admin', 'Admin', NOW() AT TIME ZONE 'UTC', false),
('user', 'fake_hash_user', 'User', NOW() AT TIME ZONE 'UTC', false);

-- Insert test theatres
INSERT INTO "Theatres" ("Name", "AddInfo")
VALUES 
('Большой театр', 'Исторический театр в центре города'),
('Малый театр', 'Камерный театр с современными постановками');

-- Insert test actors
INSERT INTO "Actors" ("Name", "VoiceType", "Gender", "BirthDate", "Height", "Weight", "AddInfo")
VALUES 
('Иван Петров', 'Bass', 'Male', '1985-05-15', 180, 75, 'Опытный актер мюзиклов'),
('Мария Сидорова', 'MezzoSoprano', 'Female', '1990-08-20', 165, 55, 'Молодая талантливая актриса');

-- Insert test themes
INSERT INTO "Themes" ("Name")
VALUES 
('Романтика'),
('Драма'),
('Комедия');

-- Insert test musicals
INSERT INTO "Musicals" ("Title", "Description", "Duration", "AgeRestriction", "TheatreId")
VALUES 
('Призрак оперы', 'Классический мюзикл о загадочном призраке', '02:30:00', 'SixteenPlus', 1),
('Кошки', 'Мюзикл о жизни кошек в большом городе', '02:00:00', 'AllAges', 2);

-- Insert test roles
INSERT INTO "Roles" ("Name", "MusicalId", "RoleType")
VALUES 
('Призрак', 1, 'Main'),
('Кристина', 1, 'Main'),
('Гризабелла', 2, 'Main'),
('Старый Ворюга', 2, 'Supporting');

-- Insert test shows
INSERT INTO "Shows" ("Date", "MusicalId")
VALUES 
('2025-12-01 19:00:00+00', 1),
('2025-12-02 18:30:00+00', 2);

-- Insert test cast members
INSERT INTO "CastMembers" ("ShowId", "RoleId", "ActorId", "Comment")
VALUES 
(1, 1, 1, 'Основной состав'),
(1, 2, 2, 'Основной состав');

-- Insert test actor-roles relationships
INSERT INTO "ActorRoles" ("ActorId", "RoleId")
VALUES 
(1, 1),
(2, 2),
(2, 3);

-- Insert test musical-themes relationships
INSERT INTO "MusicalThemes" ("MusicalId", "ThemeId")
VALUES 
(1, 1),
(1, 2),
(2, 3);

-- Insert test favorites
INSERT INTO "AccountTheatreFavorites" ("AccountId", "TheatreId", "AddedDate")
VALUES 
(2, 1, NOW() AT TIME ZONE 'UTC');

INSERT INTO "AccountMusicalFavorites" ("AccountId", "MusicalId", "AddedDate")
VALUES 
(2, 1, NOW() AT TIME ZONE 'UTC');

INSERT INTO "AccountActorFavorites" ("AccountId", "ActorId", "AddedDate")
VALUES 
(2, 1, NOW() AT TIME ZONE 'UTC');