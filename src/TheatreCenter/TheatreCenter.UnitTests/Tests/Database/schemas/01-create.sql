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