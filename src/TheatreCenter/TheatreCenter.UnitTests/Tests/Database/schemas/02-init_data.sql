
--DROP TABLE IF EXISTS "CastMembers" CASCADE;
--DROP TABLE IF EXISTS "ActorRoles" CASCADE;
--DROP TABLE IF EXISTS "MusicalThemes" CASCADE;
--DROP TABLE IF EXISTS "Shows" CASCADE;
--DROP TABLE IF EXISTS "Roles" CASCADE;
--DROP TABLE IF EXISTS "Musicals" CASCADE;
--DROP TABLE IF EXISTS "Themes" CASCADE;
--DROP TABLE IF EXISTS "Actors" CASCADE;
--DROP TABLE IF EXISTS "Theatres" CASCADE;
--DROP TABLE IF EXISTS "Accounts" CASCADE;





---- Create indexes for performance
--CREATE UNIQUE INDEX "IX_Accounts_Username" ON "Accounts"("Username");
--CREATE INDEX "IX_Shows_Date" ON "Shows"("Date");
--CREATE INDEX "IX_Shows_MusicalId" ON "Shows"("MusicalId");
--CREATE INDEX "IX_Roles_MusicalId" ON "Roles"("MusicalId");
--CREATE INDEX "IX_Musicals_TheatreId" ON "Musicals"("TheatreId");
--CREATE INDEX "IX_CastMembers_ShowId" ON "CastMembers"("ShowId");
--CREATE INDEX "IX_CastMembers_ActorId" ON "CastMembers"("ActorId");
--CREATE INDEX "IX_CastMembers_RoleId" ON "CastMembers"("RoleId");
--CREATE UNIQUE INDEX "IX_Themes_Name" ON "Themes"("Name");
--CREATE UNIQUE INDEX "IX_Theatres_Name" ON "Theatres"("Name");

---- Insert test theatres
--INSERT INTO "Theatres" ("Name", "AddInfo")
--VALUES 
--('Большой театр', 'Исторический театр в центре города');

---- Insert test actors
--INSERT INTO "Actors" ("Name", "VoiceType", "Gender", "BirthDate", "Height", "Weight", "AddInfo")
--VALUES 
--('Иван Петров', 'Bass', 'Male', '1985-05-15', 180, 75, 'Опытный актер мюзиклов');


---- Insert test musicals
--INSERT INTO "Musicals" ("Title", "Description", "Duration", "AgeRestriction", "TheatreId")
--VALUES 
--('Призрак оперы', 'Классический мюзикл о загадочном призраке', '02:30:00', 'SixteenPlus', 1);

---- Insert test roles
--INSERT INTO "Roles" ("Name", "MusicalId", "RoleType")
--VALUES 
--('Призрак', 1, 'Main');

---- Insert test shows
--INSERT INTO "Shows" ("Date", "MusicalId")
--VALUES 
--('2025-12-01 19:00:00+00', 1);




