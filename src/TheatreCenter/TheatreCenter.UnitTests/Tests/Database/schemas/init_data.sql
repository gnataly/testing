-- Insert test accounts
INSERT INTO "Accounts" ("Username", "PasswordHash", "AccessLevel", "LastFavoritesViewDate", "UpgradeRequest")
VALUES 
('admin_user', 'fake_hash_admin', 'Admin', NOW() AT TIME ZONE 'UTC', false),
('test_user', 'fake_hash_user', 'User', NOW() AT TIME ZONE 'UTC', false);

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