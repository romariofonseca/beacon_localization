# VirtualBorder <!-- omit in toc -->

A **.NET-based system** for monitoring “virtual boundaries” in factories and warehouses — tracking devices through MQTT, displaying them on a real-time map, and exposing a Web API for MES/WMS integration. 

> **Status**: Proof-of-Concept – functional, but still evolving
> **Target Framework**: .NET 7 → 8 (LTS compatible)
> **Database**: Embedded SQLite
> **Map Engine**: Leaflet.js + OpenStreetMap

---

## Table of Contents <!-- omit in toc -->

1. Overview
2. Architecture
3. Core Features
4. Prerequisites
5. Installation and Execution
6. Configuration
7. Database Structure
8. REST Endpoints
9. MQTT Flow
10. Offline Build / Local NuGet
11. Quick Test
12. Roadmap
13. Contributing
14. License

---

## Overview

**VirtualBorder** solves a common challenge in production lines — knowing, in real time, *where* each mobile device (AGV, robot, forklift, etc.) is located and whether it has crossed restricted areas (“virtual boundaries”). 

It:

* **Consumes** position data through **MQTT** (`virtualborder/posto`) sent by ESP32 devices, PLCs, or gateways.
* **Stores** readings in **SQLite** for auditing and analytics.
* **Renders** an **interactive map** (Leaflet) displaying:

  * Latest position & timestamp by MAC address
  * Movement history trail (direction arrows)
  * Green / orange / red status indicators based on the elapsed time since the last reading
* **Exposes** a **RESTful Web API** for MES/WMS integration:

  * Map management (`/api/maps`)
  * Order & product import
  * Device and event queries

---

## Architecture

```text
┌─────────────┐     MQTT       ┌─────────────┐
│   Devices   │───(broker)───▶│ VirtualBorder│
│ (ESP/PLC)   │               │   Web API    │
└─────────────┘               │    .NET 8    │
                              └─────┬────────┘
                                    │ EF Core
                              ┌─────▼──────┐
                              │  SQLite    │
                              └────────────┘
```

* **Web/API Layer**: ASP.NET Core Minimal API + Razor Pages dashboard
* **Data Layer**: Entity Framework Core (code-first)
* **MQTT Service**: MQTTnet running as a background service
* **Frontend**: Razor + Leaflet + Fetch/AJAX

---

## Core Features

| Category            | Description                                                                             |
| ------------------- | --------------------------------------------------------------------------------------- |
| **Tracking**        | Consumes MQTT topics, stores latest positions, and updates the UI through SSE/WebSocket |
| **Dynamic Map**     | Zoom support, OSM layers, MAC/line filtering, and latency legend indicators             |
| **CSV/JSON Import** | Endpoints for orders (`/api/orders`) and products (`/api/items`) with schema validation |
| **Alerts**          | Optional Webhook/Telegram notifications when a device crosses a boundary                |
| **Offline Mode**    | Local NuGet feed + Docker Compose (Mosquitto + .NET app)                                |

---

## Prerequisites

| Software        | Minimum Version                      |
| --------------- | ------------------------------------ |
| **.NET SDK**    | 8.0.x (or 7.0.x)                     |
| **SQLite**      | No client required – embedded binary |
| **Node/NPM**    | Optional – frontend asset build      |
| **MQTT Broker** | Eclipse Mosquitto 2.x                |

---

## Installation and Execution

```bash
# 1. Clone
git clone https://github.com/<your-user>/VirtualBorder.git
cd VirtualBorder

# 2. Restore & build
dotnet restore
dotnet build -c Release

# 3. Apply database migrations
dotnet ef database update

# 4. Run
dotnet run --project src/VirtualBorder
```

The application starts at:

* `http://localhost:5050`
* Dashboard: `http://localhost:5050/dashboard`

### Quick Docker Setup

```bash
docker compose up -d
```

Includes both the MQTT broker and the .NET application.

---

## Configuration

`appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "Default": "Data Source=virtualborder.db"
  },
  "Mqtt": {
    "Broker": "10.241.210.95",
    "Port": 1883,
    "Topic": "virtualborder/posto"
  },
  "Map": {
    "DefaultZoom": 18,
    "TileProvider": "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
  }
}
```

Environment variables override these settings.

---

## Database Structure

| Table      | Main Fields                      | Description                |
| ---------- | -------------------------------- | -------------------------- |
| `devices`  | `id, mac, nome, created_at`      | Registered devices         |
| `maps`     | `id, nome, png_blob`             | Overlay maps               |
| `readings` | `id, mac, lat, lon, dataHora`    | MQTT readings              |
| `orders`   | `order, client, type, launch, …` | Imported production orders |

To recreate migrations:

```bash
dotnet ef migrations add Init
```

---

## REST Endpoints

| Method | Route                  | Description                      |
| ------ | ---------------------- | -------------------------------- |
| GET    | `/api/devices`         | List devices                     |
| POST   | `/api/devices`         | Create/update device             |
| GET    | `/api/maps/{id}`       | Retrieve PNG/base64 map          |
| POST   | `/api/maps`            | Upload a new map                 |
| POST   | `/api/orders/import`   | Import order CSV                 |
| GET    | `/api/readings/latest` | Retrieve latest position per MAC |

Swagger documentation is available at `/swagger`.

---

## MQTT Flow

```text
Topic: virtualborder/posto
Payload (JSON):
{
  "mac": "AA:BB:CC:DD:EE:FF",
  "lat": -3.12345,
  "lon": -60.98765,
  "dataHora": "2025-04-24T21:00:00-03:00"
}
```

* Recommended **QoS**: `1`
* Readings are persisted and the dashboard is updated through Server-Sent Events.

---

## Offline Build / Local NuGet

1. On an online machine:

```bash
dotnet restore --runtime win-x64 --packages ./nupkgs
```

2. Copy `nupkgs` to the offline machine.

3. Create `nuget.config`:

```xml
<configuration>
  <packageSources>
    <add key="OfflineFeed" value="C:\nuget\nupkgs" />
  </packageSources>
</configuration>
```

4. Run:

```bash
dotnet restore --source OfflineFeed
```

---

## Quick Test

```bash
mosquitto_pub -h localhost -t virtualborder/posto -m '{
  "mac":"DE:AD:BE:EF:01",
  "lat":-3.1201,
  "lon":-60.0123,
  "dataHora":"'"$(date -Iseconds)"'"
}'
```

Open the dashboard — the marker should blink green.

---

## Roadmap

* [ ] JWT authentication
* [ ] Boundary crossing logs
* [ ] PDF reports (time spent in each zone)
* [ ] GPX/KML export
* [ ] Multi-arch Docker support (arm64/amd64)

---

## Contributing

1. Fork the repository and create a branch (`feature/...`)
2. Run `dotnet test`
3. Open a Pull Request following the project template

---

## License

Distributed under the **MIT License**.
See `LICENSE` for additional details.
