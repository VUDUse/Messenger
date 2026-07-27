# План развития проекта "Messenger"

---

## Фаза 0 — Безопасность и гигиена репозитория (делать первой, без исключений)

- [ ] `chore`: добавить `.gitignore` (bin/obj, appsettings.*.json с секретами, .vs)
- [ ] `refactor`: вынести строку подключения к SQL Server из `ApplicationContext.cs` в `appsettings.json` / переменные окружения; закоммитить только `appsettings.Example.json` с заглушками
- [ ] `feat`: хеширование паролей (BCrypt.Net-Next) для `Users.Password`; при логине сравнивать хеш, не строку
- [ ] `feat`: то же самое для `ChatRooms.Password` (или убрать пароль у чатов вовсе — см. Фазу 2)
- [ ] `refactor`: заменить `RoomID`/`Users.ID` со случайных строк на нормальные PK (`int identity` или `Guid`), подключить EF Core Migrations вместо `Database.EnsureCreated()`

---

## Фаза 1 — Полноценная регистрация, подтверждение почты и профиль (Ник#цифры)

Новая модель `Users`: `Email` (уникальный, `EmailConfirmed: bool`), `Nickname`, `Discriminator` (4 цифры), `PasswordHash`.
Публичный ID пользователя — строка вида `Ник#1234`, но в переписке отображается только `Nickname` (без `#1234`).

- [ ] `refactor`: обновить сущность `Users` — добавить `Email`, `EmailConfirmed`, `Discriminator`; убрать старое поле `Login` как способ входа
- [ ] `feat`: генерация уникального `Discriminator` при регистрации — при совпадении `Nickname` подбирать свободные 4 цифры (уникальность по паре `Nickname+Discriminator`, как в Discord)
- [ ] `feat`: сервис отправки почты (MailKit + SMTP); SMTP-креды хранить в конфиге, а не в коде (по аналогии с Фазой 0)
- [ ] `feat`: генерация кода подтверждения (6 цифр, TTL ~10 минут), сохранение кода во временной таблице/поле, привязанном к email
- [ ] `feat`: экран регистрации — поля Email, Nickname, Password; после сабмита открывается окно "Введите код из почты"
- [ ] `feat`: экран подтверждения кода — проверка кода, установка `EmailConfirmed = true`, кнопка "отправить код повторно" (с ограничением частоты)
- [ ] `refactor`: вход в аккаунт — по `Email + Password` вместо текущего `Login + Password`
- [ ] `feat`: смена ника в Settings — меняется только `Nickname`, `Discriminator` остаётся неизменным (это и есть постоянный идентификатор профиля)
- [ ] `refactor`: во всех местах, где отображается отправитель сообщения (чат, список участников) — показывать только `Nickname`; полный `Ник#цифры` показывать только в профиле пользователя

**Важно:** пока `EmailConfirmed = false`, не создавать запись в `Users` до подтверждения, а хранить "pending regisration" отдельно.

---

## Фаза 2 — Новая модель данных: личные чаты и группы

- [ ] `feat`: сущности `Conversations` (Type: Direct/Group), `ConversationMembers` (UserId, ConversationId, Role), `Messages` (Id, ConversationId, SenderId, Content, Type, Timestamp, IsEdited, IsDeleted)
- [ ] `refactor`: убрать логин/пароль у чат-комнаты — вступление в личный чат по `Nickname#Discriminator` собеседника, в группу — по инвайту/добавлению участника
- [ ] `refactor`: сервисный слой (`ChatService`, `UserService`) — вынести обращения к `DbContext` из code-behind страниц
- [ ] `feat`: перевести отправку/чтение сообщений на новую таблицу `Messages` вместо строки `ChatHistory`
- [ ] `feat`: UI создания группы (список участников, название, аватар опционально)

---

## Фаза 3 — Файлы, аудио- и видеосообщения (хранение — BLOB в БД)

- [ ] `feat`: добавить в `Messages` поля `AttachmentData (byte[])`, `AttachmentFileName`, `AttachmentMimeType`, `AttachmentSize`
- [ ] `feat`: отправка файла (диалог выбора файла → чтение в byte[] → сохранение в БД)
- [ ] `feat`: отображение файла в чате (иконка/имя + кнопка "скачать" → сохранить byte[] на диск)
- [ ] `feat`: запись аудио через NAudio, отправка как voice-message
- [ ] `feat`: запись видео с камеры, отправка видео-сообщения
- [ ] `chore`: ограничение на размер вложения (BLOB в SQL Server нужен разумный лимит, 10-20 МБ)

---

## Фаза 4 — Realtime через SignalR (главный архитектурный шаг)

Сейчас клиент напрямую ходит в SQL Server — для SignalR нужен промежуточный сервер.

- [ ] `feat`: новый проект `Messenger.Server` (ASP.NET Core) — здесь будет жить SignalR Hub и доступ к БД
- [ ] `feat`: `ChatHub` — методы `SendMessage`, события `ReceiveMessage`, `UserTyping`
- [ ] `feat`: клиент (WPF) подключается к хабу при входе, подписывается на события
- [ ] `refactor`: отправка сообщения теперь идёт через хаб (`hub.SendMessage(...)`), а не прямой `db.SaveChanges()` из клиента
- [ ] `refactor`: убрать `DispatcherTimer`-поллинг чата — обновление UI по событию из хаба
- [ ] `feat`: индикатор "печатает…"

---

## Фаза 5 — Автовход

- [ ] `refactor`: полностью убрать `CheckIPAddressLogin`
- [ ] `refactor`: `AuthService` — хранить токен не в реестре, а в файле `%AppData%\Messenger\auth.dat`, шифруя через `System.Security.Cryptography.ProtectedData`
- [ ] `feat`: явный logout с инвалидацией токена на сервере (Фаза 4 уже добавит серверную часть — токен можно проверять там же)

---

## Фаза 6 — Темы и стиль

- [ ] `refactor`: вынести текущие цвета/стили из `App.xaml` в отдельный `Themes/Dark.xaml`
- [ ] `feat`: добавить `Themes/Light.xaml`
- [ ] `feat`: переключатель темы в Settings + сохранение выбора (в том же файле конфигурации, что и токен, либо отдельно)

---

## Фаза 7 — Доп. функционал (по мере желания/времени)

- [ ] `feat`: статусы сообщений (доставлено/прочитано)
- [ ] `feat`: редактирование и удаление своих сообщений
- [ ] `feat`: аватары пользователей и групп
- [ ] `feat`: поиск по истории сообщений
- [ ] `feat`: toast-уведомления о новых сообщениях
- [ ] `feat`: локализация RU/EN
- [ ] `chore`: логирование через Serilog вместо тихих падений try/catch
- [ ] `test`: базовые unit-тесты для сервисного слоя (хотя бы ChatService/AuthService)
