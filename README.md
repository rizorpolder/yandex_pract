# CRUD API сервис для управления событиями (Events)

## Оглавление

- [Архитектура проекта](#архитектура-проекта)
- [Требования](#требования)
- [Установка](#установка)
- [Настройка подключения к БД](#настройка-подключения-к-бд)
- [Фильтрация](#фильтрация)
- [Бронирование событий](#Бронированиесобытий)
- [Примитивы синхронизации](#примитивы-синхронизации-использованные-в-проекте)
- [Тестирование](#тестирование)
- [Ошибки](#ошибки)

## Архитектура проекта

Проект построен по принципам слоистой (Clean/Onion) архитектуры и разделён на пять проектов/слоёв. Ключевое
правило — зависимости идут только "внутрь", к домену: `Domain` ни от кого не зависит, `Application` зависит
только от `Domain`, `Infrastructure` реализует интерфейсы `Application`, `Presentation` связывает
`Infrastructure`+`Application` через DI и содержит контроллеры/middleware, а `Web` — тонкий исполняемый хост
(`Program.cs`), который лишь подключает `Presentation`.

```
Web (host, Program.cs)
        │
        ▼
Presentation
        │
        ▼
Infrastructure ──► Application ──► Domain
```

### Domain

Ядро приложения: доменные сущности и бизнес-правила, не зависящие ни от EF Core, ни от ASP.NET, ни от какого-либо
внешнего слоя.

- `Domain.Models.Event.Event` — сущность события: инкапсулирует `Title`, `Description`, `StartAt`/`EndAt`,
  `TotalSeats`/`AvailableSeats`; резервирование и освобождение мест (`TryReserveSeats`, `ReleaseSeats`) и
  обновление полей (`UpdateEvent`) — это методы самой сущности, а не сервисов, чтобы бизнес-правила не
  "утекали" наружу.
- `Domain.Models.Booking.Booking` — сущность бронирования: `Id`, `EventId`, `Status`, `CreatedAt`, `ProcessedAt`,
  переходы состояния через `Confirm()`/`Reject()`.
- `Domain.Exceptions` — доменные исключения, например `NoAvailableSeatsException`, которое выбрасывается при
  попытке забронировать место в событии без свободных мест.

### Application

Слой сценариев использования (use cases) и бизнес-логики поверх домена. Не знает про EF Core или конкретную
БД — работает только через абстракции репозиториев.

- `Application.Services.Abstraction.Repositories` — интерфейсы `IEventRepository`, `IBookingRepository`,
  описывающие контракт хранилища (CRUD-операции над `Event`/`Booking`), которые реализует уже `Infrastructure`.
- `Application.Services.Abstraction.Services` — интерфейсы `IEventService`, `IBookingService`.
- `Application.Services.Abstraction.RequestResult` — обёртка `Result<T>` (`IsSuccess`/`Value`/`ErrorMessage`)
  для единообразной передачи результата операций без исключений в качестве управления потоком.
- `Application.Services.BookingService` — реализация `IBookingService`: создание бронирования с проверкой мест
  (`TryReserveSeats`) и защитой от гонок через `SemaphoreSlim`, получение бронирования по id.
- `Application.Services.BackgroundBookingService` — фоновый `BackgroundService`, асинхронно подтверждающий
  `Pending`-бронирования (`Confirm`) либо отклоняющий их с возвратом мест (`Reject` + `ReleaseSeats`) при сбое
  или отмене.
- `Application.Services.Mapping` — внутренние (`internal`) мапперы `EventMapper`/`BookingMapper` между доменными
  сущностями и DTO уровня Application/Presentation.
- Сервис `EventService` (реализация `IEventService`) и связанные с ним `EventDto`, `EventFilterService` — по
  историческим причинам всё ещё живут в неймспейсе `yandex_pract.*` (это исходный неймспейс проекта до
  разделения на слои), хотя по назначению они относятся именно к слою Application. Это единственное заметное
  расхождение между физическим неймспейсом и логическим слоем — стоит иметь в виду при дальнейшем рефакторинге.

### Infrastructure

Реализация абстракций `Application` поверх конкретной технологии хранения — EF Core + PostgreSQL.

- `Infrastructure.Repositories.EfEventRepository` / `EfBookingRepository` — реализации `IEventRepository` /
  `IBookingRepository` через `AppDbContext`.
- `yandex_pract.DbContext.AppDbContext` — EF Core `DbContext` с наборами `Events`/`Bookings`.
- `yandex_pract.Interceptors.DateTimeInterceptor` — перехватчик EF Core для нормализации `DateTime`-полей
  (например, приведение к UTC) при сохранении.
- Миграции EF Core, описывающие схему БД (таблицы `events`/`bookings`, CHECK-ограничения на количество мест и
  временной диапазон, внешние ключи, индексы — подробнее в разделе "Тестирование" → `MigrationsTests`).

### Presentation

Класс-библиотека со всем, что относится к HTTP-слою: контроллеры, middleware, конфигурация DI и Swagger.

- `Presentation.ServiceCollectionExtensions.AddPresentation(configuration)` — регистрирует контроллеры,
  Swagger, и вызывает `AddInfrastructure(configuration)` + `AddApplication()`, то есть именно здесь
  собираются воедино все нижележащие слои.
- `Presentation.ServiceCollectionExtensions.UsePresentation()` — настраивает middleware-пайплайн: применяет
  миграции БД через `UseInfrastructure()`, подключает кастомный `MyCustomMiddleware`, Swagger UI (только в
  Development), `UseHttpsRedirection`, `UseRouting`.
- `Presentation.ServiceCollectionExtensions.MapPresentationEndpoints()` — регистрирует маршруты контроллеров
  (`MapControllers()`).
- `Presentation.Middleware` — кастомные middleware (например, глобальная обработка ошибок, см. раздел
  "Ошибки" ниже).

Сам `Presentation` не содержит `Program.cs` и не запускается напрямую — это переиспользуемая библиотека,
которую подключает исполняемый хост.

### Web

Тонкий исполняемый проект (`src/Web/Web.csproj`) — единственная точка входа приложения. Содержит только
`Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();
app.UsePresentation();
app.MapPresentationEndpoints();
app.Run();
```

Сам `Web` не содержит бизнес-логики, контроллеров или конфигурации DI — вся эта работа делегирована в
`Presentation` (см. выше). Разделение `Web`/`Presentation` позволяет, например, переиспользовать
`Presentation` в другом хосте (тестовом `WebApplicationFactory`, воркере и т.п.) без необходимости
дублировать настройку DI.

## Требования

- .NET SDK 9.0
- PostgreSQL 13+ (используется как основное хранилище данных приложения через провайдер Npgsql)
- Docker (для интеграционных тестов)

## Установка

* Скачать проект из репозитория
* Запустить коммандную строку
* Выполнить команду cd \d путь к проекту
* Выполнить команду dotnet build
* Выполнить команду dotnet run (для запуска основного проекта

<code>dotnet run --project src/Web/Web.csproj</code>)

* перейти по ulr: http://localhost:5000/swagger/index.html

## Настройка подключения к БД

Приложению для работы требуется запущенный сервер PostgreSQL. Перед первым запуском:

1. Убедитесь, что PostgreSQL установлен и запущен (локально либо в Docker):

   <code>docker run --name pg-events -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -p 5432:5432 -d postgres:
   16</code>

2. Укажите строку подключения в `appsettings.json` (или `appsettings.Development.json`) в секции `ConnectionStrings`:

       {
         "ConnectionStrings": {
           "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=events_db;Username=postgres;Password=postgres"
         }
       }

3. При необходимости строку подключения можно переопределить через переменную окружения или `dotnet user-secrets`, не
   храня пароль в репозитории:

       dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=5432;Database=events_db;Username=postgres;Password=postgres" --project src/Web/Web.csproj

Управление схемой БД (EF Core Migrations)
Схема базы данных управляется миграциями EF Core.
При изменении моделей необходимо создавать новую миграцию и применять её к базе данных.

`AppDbContext` живёт в проекте `Infrastructure`, а запускаемый (startup) проект — `Web`. Поэтому команды `dotnet
ef` теперь принимают два флага: `--project` (где искать/создавать миграции) и `--startup-project` (откуда брать
конфигурацию и DI для применения миграций).

> Путь к `Infrastructure.csproj` ниже дан по аналогии с `src/Web/Web.csproj` (соседняя папка в `src/`) — если у
> вас он лежит иначе, поправьте путь под свою структуру.

Создание миграции

    dotnet ef migrations add InitialMigration --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

Применение миграций

    dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

Откат миграции

     dotnet ef database update PreviousMigrationName --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

Удаление последней миграции

    dotnet ef migrations remove --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

<b>Важно</b>  
В приложении используется Database.MigrateAsync(), а не EnsureCreated().
EnsureCreated() не подходит для продакшена и интеграционных тестов — он не создаёт CHECK‑ограничения, индексы и внешние
ключи.

## Фильтрация

Этот раздел описывает работу фильтрации, пагинации и выборки данных через Get/events

<b> Структура запроса:</b> 

    GET /events?title={string}&from={date}&to={date}&page={int}&pageSize={int}

Фильтрация осуществляется по:

- Названию:<code>?title=meet</code>
- Дате начала и окончания: <code> ?from=2024-01-01 </code>  <code>?to=2024-12-31</code>
- Комбинированный фильтр:<code>/events?title=meet&from=2024-01-01&to=2024-12-31&page=1&pageSize=5 </code>

Формат ответа PaginatedResultDto:

    "pageIndex": 1,
    "entriesCount": 42,
    "data": [
            {
                "id": "guid",
                "title": "string",
                "description": "string",
                "startAt": "2024-01-01T10:00:00",
                "endAt": "2024-01-01T11:00:00"
            }
            ]

## События

Модель `Event`
Класс `Event` представляет доменную сущность «Событие» и содержит основную информацию о мероприятии, включая параметры
бронирования мест.

### Основные поля

    `Id` — уникальный идентификатор события (Guid), генерируется автоматически при создании объекта.
    `Title` — название события.
    `Description` — описание события.
    `StartAt` / `EndAt` — время начала и окончания.
    `TotalSeats` — общее количество мест, доступных для бронирования.
    `AvailableSeats` — текущее количество свободных мест.

Изначально равно `TotalSeats`, уменьшается при бронировании и увеличивается при отмене/отклонении.

### Конструкторы

Создание события вручную

    public Event(string title, string description, DateTime startAt, DateTime endAt, int totalSeats)

Используется при создании новых событий в приложении.

Инициализирует:

    Id — новым Guid
    TotalSeats — значением параметра
    AvailableSeats — равным TotalSeats

Создание события из DTO

    public Event(EventDto dto)

Используется при загрузке данных из внешних источников (например, из JSON).
Переносит данные из DTO в доменную модель и выставляет:

    TotalSeats — из DTO
    AvailableSeats — равным TotalSeats

## Бронирование событий

Новые эндпоинты

    POST /events/{id}/book 

Создает новое бронирование для указанного события.

Описание:
Создает объект Booking, сохраняет его в базе и помещает в очередь фоновой обработки.
Возвращает идентификатор созданного бронирования и статус Pending.

Ответы:

- 202 Accepted — бронирование принято в обработку
- 404 Not Found — событие не найдено
- 409 Conflict - отсутствуют места на данное событие

  GET /bookings/{id}

- Возвращает текущее состояние бронирования.

Ответы:

- 200 OK — возвращает объект бронирования
- 404 Not Found — бронирование не найдено

  public class Booking
  {
  public Guid Id;
  public Guid EventId;
  public BookingStatus Status;
  public DateTime CreatedAt;
  public DateTime ProceedAt;

        public Booking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            CreatedAt = DateTime.UtcNow;
            Status = BookingStatus.Pending;
        }
  }

### Поля

    Id — уникальный идентификатор бронирования
    EventId — идентификатор события
    Status — текущий статус бронирования
    CreatedAt — время создания
    ProceedAt — время обработки (устанавливается фоновым сервисом)

### Статусы

    Pending — бронирование создано и ожидает обработки
    Confirmed — фоновый сервис подтвердил бронирование
    Rejected — статус для возможного расширения логики

### Логика фоновой обработки

Фоновый сервис (BackgroundBookingService) обрабатывает бронирования асинхронно.

#### Основные этапы:

- При создании бронирования оно помещается в очередь.
- BackgroundBookingService периодически извлекает элементы из очереди.
- Если очередь пуста, сервис делает небольшую задержку, чтобы избежать избыточной нагрузки.
- Для каждого бронирования выполняется задержка, имитирующая обработку.
- Статус бронирования изменяется на Confirmed.
- Поле ProceedAt заполняется текущим временем.
- Обновленный объект доступен через GET /bookings/{id}.

#### Пример сценария использования

1) Клиент отправляет запрос
   <code>POST /events/{eventId}/book</code>
   Сервер создает бронирование со статусом Pending и возвращает его Id.

2) Через короткое время фоновый сервис:
    - извлекает бронирование из очереди
    - выполняет задержку
    - обновляет статус на Confirmed

3) Клиент запрашивает
   `GET /bookings/{bookingId}`
   и получает обновленный статус.

4) После обработки клиент видит:
    - Status = Confirmed
    - ProceedAt содержит время обработки

### Примитивы синхронизации, использованные в проекте

В проекте используются два разных механизма синхронизации, каждый из которых решает свою задачу в разных слоях системы.

### 1) lock в BookingService спользуется для защиты критической секции при создании брони.

#### Зачем нужен

Метод CreateBookingAsync выполняет атомарную операцию:

* Получить событие
* Проверить доступные места
* Зарезервировать место
* Обновить событие
* Создать бронь

Без lock два параллельных потока могли бы:

- одновременно прочитать AvailableSeats = 1
- оба решить, что место есть
- оба создать бронь
- в итоге получить овербукинг

#### Что защищает

lock (_bookingLock) гарантирует, что только один поток может выполнять эту последовательность действий одновременно.
Это полностью устраняет гонку за места.

### 2) SemaphoreSlim в BackgroundBookingService

Используется для защиты записи в хранилище при параллельной обработке заявок.

#### Зачем нужен

Фоновый сервис обрабатывает Pending‑брони параллельно:

    var tasks = pending.Select(ProcessBookingAsync);
    await Task.WhenAll(tasks);

Но при этом:

- обновление брони (booking.Confirm() / booking.Reject())
- обновление события (event.ReleaseSeats() / event.Update())
- должно быть строго последовательным, чтобы не повредить данные.

#### Что защищает

SemaphoreSlim(1, 1) обеспечивает:

- параллельные задержки (Task.Delay)
- но последовательную запись в хранилище
  Это позволяет:
- обрабатывать много заявок одновременно
- но не допускать конфликтов при обновлении данных

### Пример сценария овербукинга (гонки потоков)

Ниже — реальный пример того, что происходило бы без синхронизации.
Условие:

    Событие имеет 5 мест.
    Поступает 20 параллельных запросов на бронирование.

Что происходит без lock

1) Потоки A и B одновременно читают AvailableSeats = 1
2) Оба считают, что место есть
3) Оба вызывают TryReserveSeats()
4) Оба уменьшают AvailableSeats до 0 (или даже до -1)
5) Оба создают бронь
   В итоге:

- создано 6, 7, 10 или даже 20 броней
- AvailableSeats становится отрицательным
- система допускает овербукинг

#### Почему так происходит

Потоки читают и изменяют общее состояние одновременно, без защиты.

#### Как решено в проекте

- lock в BookingService делает операцию резервирования атомарной
- SemaphoreSlim в BackgroundBookingService делает обновление хранилища последовательным

В результате:

- максимум создаётся ровно столько броней, сколько мест
- остальные запросы получают NoAvailableSeatsException
- AvailableSeats никогда не уходит в минус

## Тестирование

Проект содержит два типа тестов: юнит‑тесты и интеграционные тесты.

1) Юнит‑тесты (InMemory EF Core):

Используют:

        <code> UseInMemoryDatabase(Guid.NewGuid().ToString())</code>

Проверяют:

- бизнес‑логику сервисов
- фильтрацию
- пагинацию
- работу фонового сервиса (частично)
- Не требуют PostgreSQL.

Запуск:

    dotnet test

2) Интеграционные тесты (PostgreSQL + Testcontainers)
   Интеграционные тесты:

- запускают реальный PostgreSQL внутри Docker;
- автоматически применяют миграции EF Core;
- проверяют все методы репозиториев (EventRepository и BookingRepository);
- проверяют CHECK‑ограничения, FOREIGN KEY, индексы;
- проверяют конкурентность (overbooking);
- проверяют работу BackgroundBookingService;
- очищают БД между тестами через TRUNCATE TABLE events, bookings.

<b>Требования</b>

- установлен Docker
- возможность запускать контейнеры (WSL2, Linux, macOS)

<b>Запуск</b>

    dotnet test

## Ошибки

В результате возникновения ошибок, будет отображен ответ формата json в котором присутствуют следующие поля:

- Status - числовой статус ошибки, который соответствует стандартным кодам ответа HTTP
- Detail - подробное описание возникшей ошибки