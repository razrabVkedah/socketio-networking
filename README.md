# Rusleo Socket.IO Networking for Unity

Лёгкая клиент‑серверная сеть для Unity на **Socket.IO**: комнаты, RPC, сетевые переменные и плавная интерполяция состояний. Клиент — Unity/C#, сервер — Node.js + MongoDB.

> Проект демонстрирует сетевую архитектуру реального уровня: авторитет хоста, синхронизация по тикам, рефлексия атрибутов, rate‑limit на сервере, буфер состояний и интерполяция.

⚠️ **Важно:** за основу используется платный пакет из AssetStore — **Best Socket.IO**. Он не может быть помещён в публичный репозиторий. Поэтому любой форк этого проекта не будет запускаться «из коробки» — потребуется либо покупка пакета, либо замена на другой клиент Socket.IO.

⚠️ **Серверная часть** написана на Node.js, но сделана крайне просто и в текущем виде выглядит «черновой». Публично в репозиторий она не выложена (стыдно такое показывать 🙂). В README приведены только общие схемы и описание.

---

## ✨ Что внутри

* **Rooms & Lobby** — комнаты с хостом, списком клиентов и пингом; вход/выход, базовая защита от флуда.
* **Network Update (ticks)** — пакеты `network-update` с данными за кадр/тик.
* **RPC через атрибуты** — `[RPC(AuthorityMode, SyncMode)]` с рефлексией и вызовами «по имени».
* **Network Variables** — `NetworkVariable<T>` с режимами **Authority**, **SyncMode** (Calm/Forced) и **InterpolationType**.
* **Интерполяция** — буфер состояний, расчёт времени интерполяции (RTT+буфер+1/tick).
* **Ping** — оценка RTT клиентом и сервером (используется в лобби и интерполяции).
* **Transport** — Socket.IO поверх TCP/WebSocket.

> Ключевые сущности в клиенте: `NetworkMonoBehaviour`, `NetworkVariable<T>`, `RPCAttribute`, `NetworkUpdateHandler`, `RoomsManager`, `PingHandler`.

---

## 🧭 Навигация

* [Быстрый старт](#-быстрый-старт)
* [Архитектура](#-архитектура)
* [Примеры кода](#-примеры-кода)
* [Структура репозитория](#-структура-репозитория)
* [Цифры проекта](#-цифры-проекта)
* [Roadmap](#-roadmap)
* [Лицензия](#-лицензия)

---

## 🚀 Быстрый старт

### 1) Сервер (Node.js)

```bash
# 1. зависимости
npm i

# 2. переменные окружения (пример)
# .env
MONGO_URL=mongodb://localhost:27017/socketio
PORT=4000

# 3. запуск
node server.js  # или ваш entrypoint (index.js/app.js)
```

### 2) Клиент (Unity)

1. Открыть проект в Unity 2021+ (или адаптировать версию под Socket.IO клиент).
2. Указать URL сервера в конфиге (например, `ClientConfig.asset` / статическая строка в `Client.cs`).
3. Запустить сцену **Lobby** → создать/войти в комнату → открыть игровую сцену.

> **Примечание:** используется платный пакет **Best Socket.IO**

---

## 🧱 Архитектура

```
Unity Client                              Node.js Server
─────────────                            ───────────────
Client (SocketIO)  ← connect(guid) →  auth/session middleware (Mongo)
   │                                              │
RoomsManager  ← enter-room/tickRate/hostTime →  Rooms (host/clients)
   │                                              │
NetworkUpdateHandler  ←→  'network-update'  ←→  Room routing + rate‑limit
RPCHandler             ←→  'network-rpc'     ←→  ↕ (host ↔ clients)
VariablesHandler       ←→  'network-variable'←→  …
```

* **Authority:** HostToClient / ClientToHost / Both (проверка прав на клиенте; для продакшена добавить валидацию на сервере).
* **TickRate:** частота логических пакетов, настраивается из комнаты.
* **Interpolation:** «сдвинутое назад» время = `RTT.avg + safety + 1/tick`.
* **Rate‑limit:** ограничение частоты событий на сокет (anti‑flood).

---

## 🧪 Примеры кода

### RPC (вызов метода по сети)

```csharp
using ClientSocketIO.NetworkBehaviour.Attributes;
using UnityEngine;

public class Shooter : ClientSocketIO.NetworkBehaviour.NetworkMonoBehaviour
{
    [RPC(AuthorityMode.Both, SyncMode.Calm)]
    private void RpcShoot(Vector3 position)
    {
        // эффекты, локальная логика попадания
    }

    public void Shoot(Vector3 worldPos)
    {
        InvokeRPC((System.Action<Vector3>)RpcShoot, worldPos);
    }
}
```

### NetworkVariable (синхронизация значения)

```csharp
using ClientSocketIO.NetworkData;
using ClientSocketIO.NetworkData.NetworkVariables;
using UnityEngine;

public class Health : ClientSocketIO.NetworkBehaviour.NetworkMonoBehaviour
{
    [SerializeField] private NetworkVariableInt health = new(100, AuthorityMode.Both, SyncMode.Calm, InterpolationType.None);

    private void Start()
    {
        health.OnGetValueFromServer.AddListener(v => Debug.Log($"HP: {v}"));
    }

    public void Damage(int amount)
    {
        health.Value = Mathf.Max(0, health.Value - amount);
    }
}
```

---

## 🗂 Обобщённая структура сервера (нет в репозитории)

```
/Server (Node.js)
 ┣ src/
 ┃ ┣ server.ts|js (вход)
 ┃ ┣ rooms/ (Room, routing, rate‑limit)
 ┃ ┣ middleware/ (auth, guid‑session)
 ┃ ┗ db/ (mongo)
 ┣ .env.example
 ┗ package.json
```

---

## 📊 Цифры проекта

> На основе текущего снэпшота кода.

**Клиент (C#)**

* \~4 233 строк кода
* 87 классов, 3 struct, 2 interface, 5 enum
* 140 атрибутов (`[RPC]` и др.), 195 методов
* Упоминания ключевых сущностей: `SocketIO`×187, `NetworkVariable`×154, `Interpolation`×90, `AuthorityMode`×65, `SyncMode`×68

**Сервер (Node.js)**

* \~900 строк кода
* Найдены маркеры: `socket`×179, `room`×180, `mongo`×58, `ping`×23, `limit`×21

---

---

## 💡 Почему это интересно

* Сетевая архитектура с чёткими ролями: хост ↔ клиенты.
* Рефлексия атрибутов для RPC/переменных (удобное API для геймплея).
* Буфер/интерполяция состояний → плавность даже при нестабильном RTT.
* Сервер: Socket.IO + Mongo, middleware, защита от флуда.
* Путь к продакшену: валидация авторитета на сервере, реплей/предсказание ввода.

---

## 🗺 Roadmap

* [ ] Валидация RPC/Variables на сервере (authority‑checks).
* [ ] Унификация сериализации (Newtonsoft.Json везде).
* [ ] Константизировать системные RPC (спавн) и ID.
* [ ] Конфиги `tickRate/fps` per room (из .env/админки).
* [ ] Предсказание/откат ввода (client‑side prediction + reconciliation).

---

✦ Автор: [Rusleo](https://github.com/razrabVkedah)
