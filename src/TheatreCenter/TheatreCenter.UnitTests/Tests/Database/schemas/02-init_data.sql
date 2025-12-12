BEGIN;

TRUNCATE "AccountActorFavorites", "AccountMusicalFavorites", "AccountTheatreFavorites",
        "CastMembers", "ActorRoles", "MusicalThemes", "Shows", "Roles",
        "Musicals", "Themes", "Actors", "Theatres", "Accounts"
        RESTART IDENTITY CASCADE;

-- Аккаунты
INSERT INTO "Accounts" ("Username", "PasswordHash", "AccessLevel", "LastFavoritesViewDate", "UpgradeRequest") VALUES
('admin', 'hash_admin', 'Admin', NOW(), false),
('user1', 'hash_user1', 'User', NOW(), false),
('user2', 'hash_user2', 'User', NOW(), false);

-- Театры
INSERT INTO "Theatres" ("Name", "AddInfo") VALUES
('Большой театр', 'Исторический театр в центре города'),
('Театр на Неве', 'Камерная сцена у реки');

-- Темы
INSERT INTO "Themes" ("Name") VALUES
('Любовь'),
('Приключения'),
('Рок-опера');

-- Мюзиклы
INSERT INTO "Musicals" ("Title", "Description", "Duration", "AgeRestriction", "TheatreId") VALUES
('Призрак оперы', 'Классика о загадочном призраке оперы', '02:30:00', 'SixteenPlus', 1),
('Алые паруса', 'История надежды и моря', '02:00:00', 'TwelvePlus', 2);

-- Связка мюзикл-тема
INSERT INTO "MusicalThemes" ("MusicalId", "ThemeId") VALUES
(1, 1),
(1, 3),
(2, 2);

-- Роли
INSERT INTO "Roles" ("Name", "MusicalId", "RoleType") VALUES
('Призрак', 1, 'Main'),
('Кристина', 1, 'Supporting'),
('Делл', 2, 'Main'),
('Ассоль', 2, 'Supporting');

-- Актеры
INSERT INTO "Actors" ("Name", "VoiceType", "Gender", "BirthDate", "Height", "Weight", "AddInfo") VALUES
('Иван Петров', 'Bass', 'Male', '1985-05-15', 182, 80, 'Солист со стажем'),
('Анна Смирнова', 'Soprano', 'Female', '1990-02-10', 168, 56, 'Лирическое сопрано'),
('Сергей Орлов', 'Tenor', 'Male', '1992-11-03', 177, 72, 'Молодой тенор'),
('Мария Кузнецова', 'MezzoSoprano', 'Female', '1988-07-21', 170, 60, 'Любит камерные партии');

-- Спектакли
INSERT INTO "Shows" ("Date", "MusicalId") VALUES
((NOW() + INTERVAL '7 day'), 1),
((NOW() + INTERVAL '14 day'), 1),
((NOW() + INTERVAL '10 day'), 2);

-- Касты (распределение)
INSERT INTO "CastMembers" ("ShowId", "RoleId", "ActorId", "Comment") VALUES
(1, 1, 1, 'Основной состав'),
(1, 2, 2, 'Основной состав'),
(2, 1, 3, 'Дубль'),
(2, 2, 4, 'Дубль'),
(3, 3, 3, 'Главный герой'),
(3, 4, 4, 'Главная героиня');

-- Избранное (пример)
INSERT INTO "AccountTheatreFavorites" ("AccountId", "TheatreId", "AddedDate") VALUES
(2, 1, NOW()),
(2, 2, NOW());

INSERT INTO "AccountMusicalFavorites" ("AccountId", "MusicalId", "AddedDate") VALUES
(2, 1, NOW()),
(3, 2, NOW());

INSERT INTO "AccountActorFavorites" ("AccountId", "ActorId", "AddedDate") VALUES
(2, 1, NOW()),
(2, 2, NOW()),
(3, 3, NOW());

COMMIT;
