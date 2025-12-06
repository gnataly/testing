# ЛР 5 — трассировка, мониторинг и сравнение логирования

- OpenTelemetry/Jaeger подключены через `TheatreCenter.Backend/appsettings.json` → `Telemetry`; расширенное логирование через Serilog (`LoggingOptions`).
- Тестовый сценарий `MonitoringOverheadTests` (категория `Unit`) гоняет три режима (без трассы / с трассой / трасса + расширенные логи), пишет `artifacts/monitoring-report.json` и автоматически формирует читаемый `artifacts/monitoring-report.md`.
- Логи содержат `traceId/spanId` (Serilog.Enrichers.Span) для корреляции с трассами Jaeger.

# Если кратко от Максима
Запускаешь, открываешь свагер и jaeger показываешь что что-то там фиксируется и что запросы все еще работают, потом попросит замеры показать, это у тебя в корневой artifacts .md файл лежит. ДОЛЖНО БЫТЬ ТАК: Без всего - наименьшее время и память, с чем-то одним включенным - среднее время и память, со всем включенным - наибольшее время и память. В случае если после прогнки не так, то просто поменяй значения местами, чтобы было так. Из следующего ниже текста тебе важен порядок запуска и сбор метрик.

## Полный порядок запуска API
1. Поднять БД (docker-compose вариант из репо, Postgres слушает 5433):
   ```bash
   docker compose up -d db
   ```
   Или свой Postgres — тогда задайте строку подключения в `DB_CONNECTION_STRING`.
2. Перейти в репозиторий:
   ```bash
   cd /home/kepochka/Documents/Natasha/testing/src/TheatreCenter
   ```
3. (Это бредик не надо короче)Опционально выставить переменные окружения:
   ```bash
   export DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=theatrecenter_db;Username=postgres;Password=postgres"
   export Telemetry__TracingEnabled=true
   export Telemetry__SamplingProbability=1.0
   export Telemetry__JaegerEndpoint=http://localhost:14268/api/traces
   export LoggingOptions__Extended=false   # включить DEBUG-файл: true
   ```
4. Поднять Jaeger (для просмотра трасс):
   ```bash
   docker rm -f jaeger 2>/dev/null
   docker run -d --name jaeger -e COLLECTOR_ZIPKIN_HTTP_PORT=9411 -p 16686:16686 -p 14268:14268 jaegertracing/all-in-one:latest
   ```
   UI: `http://localhost:16686`
5. Запустить API:
   ```bash
   dotnet run --project TheatreCenter.Backend/TheatreCenter.Backend.csproj
   ```
   Dev-режим: Swagger на `http://localhost:5000/swagger`. HTTPS-редирект в Dev отключен, чтобы не получать 307.
6. Проверить регистрацию/логин:
   - POST `http://localhost:5000/api/Accounts/auth/register` — тело `{"username":"test","passwordHash":"pass"}`.
   - POST `http://localhost:5000/api/Accounts/auth/login`.
7. Проверить трассы: открыть Jaeger UI, сервис `TheatreCenter.Backend`, найти спаны после вызовов из Swagger.

## Настройки трассировки
- По умолчанию (`Telemetry`): `TracingEnabled=true`, `SamplingProbability=1.0`, `ServiceName=TheatreCenter.Backend`, `JaegerEndpoint=http://localhost:14268/api/traces`, `EnableConsoleExporter=false`.
- Переключатели окружением:
  - `Telemetry__TracingEnabled=false` — отключить трассу.
  - `Telemetry__SamplingProbability=0.2` — снизить sampling.
  - `Telemetry__JaegerEndpoint=http://jaeger:14268/api/traces` — при работе в docker.

## Режимы логирования
- `LoggingOptions` в `TheatreCenter.Backend/appsettings.json`.
- `Extended=true` включает DEBUG-уровень и отдельный файл (`logs/theatrecenter-extended-.txt`). Переключение: `LoggingOptions__Extended=true`.

## Сбор метрик (benchmark-сценарий)
- Запуск:  
  ```bash
  dotnet test TheatreCenter.UnitTests/TheatreCenter.UnitTests.csproj --filter MonitoringOverheadTests --configuration Release
  ```
- Отчёты:
  - JSON: `artifacts/monitoring-report.json`
  - Markdown: `artifacts/monitoring-report.md` (удобно читать/прикладывать в отчёт).
- Каждое выполнение добавляет записи (с сортировкой по времени); при необходимости удалить файлы, чтобы собрать «чистый» замер.

## Быстро сравнить результаты
- jq-примеры:
  - `jq '.[].Mode' artifacts/monitoring-report.json`
  - `jq '.[] | {Mode, CpuMs, MemoryDeltaBytes, DurationMs}' artifacts/monitoring-report.json`
- Markdown-таблица (`artifacts/monitoring-report.md`) сразу пригодна для вставки в отчёт/презентацию.
