# Обзор
Этот проект — клиент‑серверная сетевая прослойка для Unity, построенная на Socket.IO (TCP поверх WebSocket). Клиент (Unity/C#) соединяется с Node.js‑сервером, входит в «комнаты» (Rooms) и обменивается тремя типами сообщений: **network-update** (пакеты данных по тикам), **network-rpc** (удалённые вызовы методов, RPC) и **network-variable** (синхронизация сетевых переменных). Поддерживаются хост‑авторитет/клиент‑авторитет, интерполяция состояний и простая анти‑флуд защита на сервере.

---

# Архитектура (высокоуровнево)
```
Unity Client                              Node.js Server
─────────────                            ───────────────
Client (Best.SocketIO)  ← connect(guid) →  auth/session middleware
   │                                              │
RoomsManager  ← enter-room/tickRate/hostTime →  Rooms
   │                                              │
Handlers:                                         │
  NetworkUpdateHandler  ←→  'network-update'  ←→  Room routing (host↔clients)
  NetworkRpcHandler     ←→  'network-rpc'     ←→  Limitation (rate limit)
  NetworkVariableHandler←→  'network-variable'←→  ...
   │                                              │
NetworkMonoBehaviour: RPC + Variables + Components (например, NetworkTransform)
```

---

# Клиент (Unity)
## Соединение
- **Client** — единая точка подключения через Best.SocketIO. В запрос добавляется `guid` (см. AuthenticationManager). На события сокета навешиваются UnityEvents (Connected, Reconnect, Error и т.п.).
- После входа в комнату клиент получает от сервера `tickRate` (частоту логических тиков) и «локальное время хоста», чтобы синхронизировать время.

## RoomsManager
- Отвечает за список комнат, вход/выход и обработку `enter-room/leave-room`.
- При входе устанавливает:
  - `Client.IsHost` — флаг авторитета,
  - `NetworkUpdateHandler.TickRate` — частота тиков,
  - `NetworkUpdateHandler.HostTimeOffset` — смещение локального времени клиента относительно времени хоста.

## NetworkUpdateHandler
- Главный «тикер» клиента. Каждые `1 / TickRate` секунд отправляет накопленные данные (**ToSendData**) в пакетах `network-update`.
- Парсит входящие сообщения и маршрутизирует их по типам: RPC, Variables или «остальные» (обычно трансформы/компоненты).
- Время сервера оценивается как `Time.unscaledTime + HostTimeOffset/1000`. Для интерполяции используется «сдвинутое назад» время: `GetInterpolationTime()` учитывает средний пинг, небольшой безопасный буфер и 1/`TickRate`.

## NetworkMonoBehaviour (база)
- На `Awake()` регистрирует все RPC‑методы ([RPC]) и все NetworkVariable поля (рефлексия), добавляет себя в менеджер, подписывается на «отправлено на сервер», где просит свои переменные сбрасывать маркеры «начала изменений».
- Имеет `NetworkMonoBehaviourId` (присваивается у спавна/вручную). Этот ID — ключ маршрутизации для RPC/переменных/данных.
- **RPC**: локальный вызов `InvokeRPC` сериализует параметры и либо копится для «спокойной» отправки (Calm), либо шлётся сразу (Forced). Входящие RPC проверяют право авторитета и вызывают нужный метод по имени.
- **Network Variables**: generic‑класс `NetworkVariable<T>` держит значение, режимы **AuthorityMode** (HostToClient/ClientToHost/Both), **SyncMode** (Calm/Forced) и **InterpolationType**. При изменении значения поднимается событие, создаётся структура `NetworkVariableData` (с `serverTime`, а при интерполяции — также `beginChangesServerTime`) и отправляется либо в общий пакет, либо отдельным сообщением.
- **Интерполяция переменных**: входящие обновления буферизуются как «состояния» с временем начала изменений и временем фиксации; апдейт выбирает интервал по `GetInterpolationTime()` и интерполирует (Vector3/Quaternion/Int/… отдельными реализациями).

## NetworkTransform (пример компонента)
- Управляет выборочным синком позиция/поворот/масштаб (по осям, со своими триггерами изменения) и интерполяцией состояний с опорой на `NetworkUpdateHandler.GetInterpolationTime()`.
- В режиме авторитета объект либо применяет входящие состояния, либо генерирует выходящие дельты (микро‑порог, сборка `TransformData`, помещение в ToSendData).

## RPC/Spawner
- Спавн по сети реализован через «служебный» RPC с `networkMonoBehaviourId = 1`. Хост локально инстанциирует префаб, получает свободный `NetworkMonoBehaviourId`, формирует `SpawnParameters` и отправляет RPC всем; клиенты создают такой же объект у себя и выставляют тот же ID.

## Ping
- `PingHandler` периодически меряет RTT: хранит окно последних измерений и даёт среднее (по умолчанию возвращает 100мс, если данных нет). Это влияет на `GetInterpolationTime()` и отображения списка комнат (сервер отдаёт пинг хоста).

---

# Сервер (Node.js)
## Сессии и авторизация по guid
- При рукопожатии Socket.IO серверное middleware требует параметр `guid` и не разрешает несколько одновременных сессий для одного пользователя. Состояние хранится в Mongo (user.socketId). При повторном коннекте закрывает/отказывает второй сессии.

## Комнаты (Rooms)
- В памяти сервера поддерживается список активных комнат. У комнаты есть hostGuid, список клиентов, **tickRate/fps**, простая защита от флуда (**Limitation**) и «локальное время хоста» для вычисления serverTime.
- При подключении сокет попадает в `roomId` и получает `enter-room` с `isHost/tickRate/hostTime`. На стороне комнаты привязываются обработчики событий сокета: для хоста данные идут «на клиентов», для простого клиента — «к хосту».
- Рассылка:
  - **sendToHost(event, data)** — только хосту;
  - **sendToClients(event, data)** — всем кроме хоста.
- Защита от флуда: Limitation ограничивает частоту вызовов обработчиков от конкретного сокета (например, `tickRate` раз/сек для `network-update` и до 60 раз/сек для RPC/Variables).
- `getRoomData()` — агрегирует инфо для лобби: id, name, clientsCount, maxClientsCount, ping хоста.

## Ping
- Серверная часть измеряет пинг сокетов (модуль `sockets-ping`) и, среди прочего, отдаёт пинг хоста в данные комнаты.

---

# Поток данных
1) **Подключение**: Unity передаёт `guid` → middleware проверяет сессию → `Client.OnConnected`.
2) **Вход в комнату**: клиент просит `enter-room` → сервер помещает сокет в комнату и отвечает `enter-room{isHost,tickRate,hostTime}`.
3) **Тики**: `NetworkUpdateHandler` на клиенте шлёт пакет `network-update` раз в `1/TickRate`, сервер пересылает его соответствующей стороне (хост↔клиенты) с rate‑limit.
4) **RPC**: `InvokeRPC` сериализует параметры; сервер просто ретрансмитит в другую сторону.
5) **Переменные**: при изменении сериализуются и отправляются либо в общий пакет, либо отдельным событием (в зависимости от SyncMode).
6) **Интерполяция**: получатели накапливают состояния и интерполируют по сдвинутому назад времени (ping + safety buffer + 1/tick).

---

# Запуск и окружение
> Набор файлов сервера указывает на Node.js + MongoDB. Конкретная точка входа (файл) в выгрузке не видна, поэтому уточни рабочий `index.js`/`app.js`.

**Сервер** (пример):
1. Установить Node.js 18+ и запустить MongoDB (локально или Atlas).
2. Настроить .env (имя базы/коллекций, адрес Mongo).
3. Установить зависимости `npm i`. Запустить `node <entry.js>`.

**Клиент (Unity)**:
1. Открыть проект в Unity (версию подбираем по пакету Best.SocketIO).
2. В `Config.MainServerUrl` прописать адрес сервера.
3. Запустить сцену лобби (Rooms UI). Войти/создать комнату, затем сцену игровую.

---

# Расширение/пример использования
## Пример RPC
```csharp
using ClientSocketIO.NetworkBehaviour.Attributes;
using UnityEngine;

public class Shooter : ClientSocketIO.NetworkBehaviour.NetworkMonoBehaviour
{
    [RPC(AuthorityMode.Both, SyncMode.Calm)]
    private void RpcShoot(Vector3 position)
    {
        // визуал эффекты, спавн пули локально
    }

    public void Shoot(Vector3 position)
    {
        // Вызов локально + отправка по сети (режим зависит от RPCAttribute)
        InvokeRPC((System.Action)RpcShoot, position);
    }
}
```

## Пример NetworkVariable
```csharp
using ClientSocketIO.NetworkData;
using ClientSocketIO.NetworkData.NetworkVariables;
using UnityEngine;

public class Health : ClientSocketIO.NetworkBehaviour.NetworkMonoBehaviour
{
    [SerializeField] private NetworkVariableInt health = new(100, AuthorityMode.Both, SyncMode.Calm, InterpolationType.None);

    private void Start()
    {
        health.OnGetValueFromServer.AddListener(v => Debug.Log($"HP synced: {v}"));
    }

    public void Damage(int amount)
    {
        health.Value = Mathf.Max(0, health.Value - amount);
    }
}
```

---

# Известные нюансы/задачи
- **RPCAttribute.SyncMode не присваивается в конструкторе.** В текущем коде значение SyncMode из атрибута, вероятно, всегда по умолчанию. Стоит сохранить параметр в поле.
- **Magic number `networkMonoBehaviourId = 1`** для системных RPC (спавн). Лучше вынести в константу/отдельный сервис.
- **Безопасность**: проверка авторитета есть на клиенте, но доверять клиенту опасно; на сервере сейчас чистая ретрансляция. Для продакшена потребуется серверная валидация.
- **Серверный `tickRate/fps`**: `Room.fps=1` и `tickRate=2` — минимальны, вероятно, для отладки. Настроить под реальные нагрузки.
- **Стабильность JSON**: клиент использует Unity JsonUtility и Newtonsoft.Json вперемешку; полезно унифицировать сериализацию.
- **Синхронизация времени**: сейчас — через `hostTime` и локальный offset. Для жёстких требований к задержке добавить NTP/скользящую коррекцию дрейфа.
- **Отсутствие «reconciliation»**: интерполяция есть, но нет предсказания/отката ввода. Это нормально для кооператива/нестрогих игр.

---

# Справочник событий
- **network-update** — пакет данных по тикам; внутри может быть `NetworkRpcData`, `NetworkVariableData` и другие `BaseNetworkData`.
- **network-rpc** — удалённые вызовы.
- **network-variable** — отдельный канал для переменных (Forced) и обратная доставка (Calm) через общий `network-update`.
- **enter-room / leave-room** — вход/выход из комнаты.
- **ping** — измерение RTT.

---

# Глоссарий
- **AuthorityMode** — кто имеет право изменять/вызывать (HostToClient / ClientToHost / Both).
- **SyncMode** — способ доставки (Calm — копим и отправляем в такт; Forced — немедленно).
- **InterpolationType** — схема интерполяции значений (None / Interpolate / LagrangeInterpolation).
- **TickRate** — частота пакетов `network-update`.
- **HostTimeOffset** — смещение локального времени клиента относительно «локального времени хоста».

---

# Что можно допилить быстро
- Починить присваивание `SyncMode` в `RPCAttribute` и добавить `InvokeHereToo` в конструктор.
- Вынести системный RPC (спавн) в отдельный сервис с явными событиями.
- Добавить server‑side валидацию авторитета для RPC/Variables.
- Сконфигурировать нормальные `tickRate/fps` и вынести в конфиги.
- Унифицировать сериализацию (например, везде Newtonsoft.Json).

