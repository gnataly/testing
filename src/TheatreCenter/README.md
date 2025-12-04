# Театральный центр – аутентификация, 2FA и E2E‑тесты

## Что реализовано по заданию
- **Аутентификация с 2FA:** логин теперь требует подтверждения одноразового кода из письма. Новые эндпоинты: `/api/v1/auth/verify-2fa`, `/api/v1/auth/unlock/request`, `/api/v1/auth/unlock/verify`, `/api/v1/auth/change-password`.
- **Двухфакторный код по email:** отправка через `SmtpEmailSender` на тестовый ящик `testnataly@mail.ru` (логин/пароль берутся из переменных окружения). Проверка в коде по хэшу, срок жизни настраивается.
- **Ограничение попыток и блокировка:** счётчики неудачных логинов/2FA, блокировка с уведомлением письмом, разблокировка по коду.
- **Смена пароля с плановой проверкой:** эндпоинт `change-password` валидирует текущий пароль и фиксирует дату смены.
- **Инфраструктура для секьюрных секретов:** настройки Email/Security в конфиге, пароли не хранятся в репозитории — читаются из env (`E2E_MAIL_*`).
- **Миграция БД:** добавлены поля безопасности в `Accounts` (последняя смена пароля, блокировки, коды 2FA/разблокировки, счётчики). Миграция `20251204060520_AddAccountSecurity`.
- **BDD E2E‑тесты:** проект `TheatreCenter.E2ETests` на LightBDD + TestServer. Покрыто: успешный 2FA‑логин, блокировка после лимита и разблокировка, смена пароля и повторный логин.

## Как запустить локально
1) **Задать окружение для почты** (используется один тестовый пользователь):
   ```bash
   export E2E_MAIL_USERNAME="testnataly@mail.ru"
   export E2E_MAIL_PASSWORD="Dh4DXueU4EYL8w0JNJg8"
   # опционально: E2E_MAIL_SMTP_HOST, E2E_MAIL_IMAP_HOST, E2E_MAIL_SMTP_PORT, E2E_MAIL_IMAP_PORT
   ```
2) **Обновить БД** (PostgreSQL по `appsettings.json` или своим параметрам):
   ```bash
   dotnet ef database update --project TheatreCenter.Data --startup-project TheatreCenter.Backend
   ```
3) **Запустить API**:
   ```bash
   dotnet run --project TheatreCenter.Backend
   ```
   Swagger: `http://localhost:5005/swagger`.

## Проверка функциональности вручную
- **Регистрация:** `POST /api/v1/auth/register` с `{"username": "<email>", "passwordHash": "<pwd>"}`.
- **Логин (первый шаг):** `POST /api/v1/auth/login` → ответ с `requiresTwoFactor=true` и `twoFactorChallengeId`.
- **Подтверждение 2FA:** `POST /api/v1/auth/verify-2fa` с `username`, `challengeId`, `code` из письма → вернётся JWT.
- **Лимит попыток / блокировка:** отправить несколько неверных кодов на `/verify-2fa` → будет 423 Locked и придёт письмо о блокировке.
- **Разблокировка:** `POST /api/v1/auth/unlock/request` → письмо с кодом; затем `POST /api/v1/auth/unlock/verify`.
- **Смена пароля:** `POST /api/v1/auth/change-password` (с авторизацией Bearer) с `username`, `currentPasswordHash`, `newPasswordHash`; после смены повторить логин + 2FA уже с новым паролем.

## Как прогнать E2E‑тесты
```bash
E2E_MAIL_USERNAME=testnataly@mail.ru \
E2E_MAIL_PASSWORD=Dh4DXueU4EYL8w0JNJg8 \
dotnet test TheatreCenter.E2ETests/TheatreCenter.E2ETests.csproj --filter AuthFlowFeature
```
Тесты поднимают API в памяти на SQLite, читают письма через IMAP и проверяют все сценарии (2FA‑логин, блок/разблокировка, смена пароля). Возможен предупреждающий вывод MSTest discovery — на успешность не влияет.

## CI/CD (GitHub Actions)
- Пайплайн: `.github/workflows/ci.yml` (push/PR). Шаги: restore → build → BDD E2E `dotnet test ... --filter AuthFlowFeature`.
- Секреты репозитория: `E2E_MAIL_USERNAME`, `E2E_MAIL_PASSWORD` (данные тестового ящика). Без них пайплайн остановится на шаге проверки секретов.

## Локальный .env
- Скопируйте `.env.example` → `.env` и заполните `E2E_MAIL_PASSWORD` (остальные значения можно оставить по умолчанию или поменять под свой SMTP/IMAP).
- Для запуска команд достаточно экспортировать его: `set -a && source .env && set +a`.
- **Windows (PowerShell):**
  ```powershell
  Copy-Item .env.example .env
  notepad .env   # вписать пароль
  Get-Content .env | ForEach-Object {
    if ($_ -match '^(\\w+)=(.*)$') { Set-Item -Path Env:$($Matches[1]) -Value $Matches[2] }
  }
  ```
  После этого команды `dotnet test`/`dotnet run` увидят переменные окружения текущей сессии.

## Файлы, которые важны
- Backend: `TheatreCenter.Backend/WebAPI/Controllers/AuthController.cs`, `Program.cs`, `appsettings.json`.
- Модель/миграции: `TheatreCenter.Domain/Models/Account.cs`, `TheatreCenter.Data/Migrations/20251204060520_AddAccountSecurity.*`.
- Сервисы: `TheatreCenter.Services/Services/AuthFlowService.cs`, `SmtpEmailSender.cs`, опции `Services/Options/*.cs`.
- DTO: `TheatreCenter.DTOs/Auth/TwoFactorDtos.cs`, `AuthDto.cs`, `AccountDto.cs`.
- Тесты: `TheatreCenter.E2ETests/*.cs`, проект `TheatreCenter.E2ETests.csproj`.
