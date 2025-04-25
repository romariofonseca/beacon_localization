# VirtualBorder <!-- omit in toc -->

Um **sistema .NET** para monitorar “fronteiras virtuais” em fábricas e armazéns— rastreando dispositivos via MQTT, exibindo-os num mapa em tempo real e expondo uma Web API para integração com MES/WMS.

> **Estado**: Proof‑of‑Concept – _funcional, porém ainda em evolução_  
> **Framework alvo**: .NET 7 → 8 (compatível com LTS)  
> **Banco de dados**: SQLite embutido  
> **Mapa**: Leaflet.js + OpenStreetMap  

---

## Índice <!-- omit in toc -->

1. [Visão geral](#visão-geral)  
2. [Arquitetura](#arquitetura)  
3. [Funcionalidades principais](#funcionalidades-principais)  
4. [Pré‑requisitos](#pré-requisitos)  
5. [Instalação e execução](#instalação-e-execução)  
6. [Configurações](#configurações)  
7. [Estrutura de banco de dados](#estrutura-de-banco-de-dados)  
8. [Endpoints REST](#endpoints-rest)  
9. [Fluxo MQTT](#fluxo-mqtt)  
10. [Build offline / NuGet local](#build-offline--nuget-local)  
11. [Teste rápido](#teste-rápido)  
12. [Roadmap](#roadmap)  
13. [Contribuindo](#contribuindo)  
14. [Licença](#licença)  

---

## Visão geral

O **VirtualBorder** resolve um problema comum em linhas de produção — saber, em tempo real, _onde_ cada dispositivo móvel (AGV, robô, empilhadeira, etc.) está e se ele ultrapassou zonas restritas (“fronteiras virtuais”).

Ele:

* **Consome** dados de posição via **MQTT** (`virtualborder/posto`) enviados por ESP32, PLC ou Gateway.  
* **Persiste** leituras em **SQLite** para auditoria e analytics.  
* **Renderiza** um **mapa interativo** (Leaflet) mostrando:  
  * Última posição & timestamp por MAC.  
  * Histórico de trilha (setas de deslocamento).  
  * Cores verde / laranja / vermelho conforme tempo desde a última leitura.  
* **Expõe** uma **Web API RESTful** para MES/WMS:  
  * Cadastro de mapas (`/api/maps`)  
  * Importação de ordens & produtos  
  * Consulta de dispositivos e eventos  

---

## Arquitetura

```text
┌─────────────┐     MQTT       ┌─────────────┐
│  Disposit.  │───(broker)───▶│ VirtualBorder│
│  (ESP/PLC)  │               │  Web API     │
└─────────────┘               │  .NET 8      │
                              └─────┬────────┘
                                    │EF Core
                              ┌─────▼──────┐
                              │  SQLite    │
                              └────────────┘
```

* **Camada Web/API**: ASP.NET Core Minimal API + Razor Pages (painel).  
* **Camada de dados**: Entity Framework Core (code‑first).  
* **Serviço MQTT**: [MQTTnet](https://github.com/dotnet/MQTTnet), rodando em background.  
* **Frontend**: Razor + Leaflet + Fetch/AJAX.

---

## Funcionalidades principais

| Categoria              | Descrição resumida |
|------------------------|--------------------|
| **Rastreamento**       | Consome tópicos MQTT, grava última posição; atualiza SSE/WebSocket na UI. |
| **Mapa dinâmico**      | Zoom, camadas OSM, filtros por MAC/linha, legenda de latência (cores). |
| **Importação CSV/JSON**| Endpoints para ordens (`/api/orders`) e produtos (`/api/items`), validador de schema. |
| **Alertas**            | Webhook/Telegram opcional quando dispositivo cruza fronteira. |
| **Modo Offline**       | NuGet local + Docker Compose (Mosquitto + app) |

---

## Pré‑requisitos

| Software        | Versão mínima |
|-----------------|---------------|
| **.NET SDK**    | 8.0.x (ou 7.0.x) |
| **SQLite**      | Nenhum client necessário – binário embutido |
| **Node/NPM**    | _Opcional_ – construir assets front‑end |
| **Broker MQTT** | Eclipse **Mosquitto 2.x** |

---

## Instalação e execução

```bash
# 1. Clone
git clone https://github.com/<seu-usuario>/VirtualBorder.git
cd VirtualBorder

# 2. Restore & build
dotnet restore          # usa nuget.config (offline ou online)
dotnet build -c Release

# 3. Migrar BD
dotnet ef database update

# 4. Run!
dotnet run --project src/VirtualBorder
```

A aplicação sobe em `http://localhost:5050`.  
O painel está em `http://localhost:5050/dashboard`.

### Docker rápido

```bash
docker compose up -d    # inclui Mosquitto + app .NET
```

---

## Configurações

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

Variáveis de ambiente sobrepõem os valores acima.

---

## Estrutura de banco de dados

| Tabela      | Campos principais                     | Descrição                        |
|-------------|---------------------------------------|----------------------------------|
| `devices`   | `id, mac, nome, created_at`           | Cadastro dos dispositivos        |
| `maps`      | `id, nome, png_blob`                  | Mapas para sobreposição          |
| `readings`  | `id, mac, lat, lon, dataHora`         | Leituras MQTT                    |
| `orders`    | `order, client, type, launch, …`      | Importações de ordens            |

Para recriar: `dotnet ef migrations add Init`.

---

## Endpoints REST

| Método | Rota                       | Descrição                                 |
|--------|----------------------------|-------------------------------------------|
| GET    | `/api/devices`             | Lista dispositivos                        |
| POST   | `/api/devices`             | Cria/edita dispositivo                    |
| GET    | `/api/maps/{id}`           | Retorna mapa PNG/base64                   |
| POST   | `/api/maps`                | Upload de novo mapa                       |
| POST   | `/api/orders/import`       | Importa CSV de ordens                     |
| GET    | `/api/readings/latest`     | Última posição de cada MAC                |

_Documentação Swagger em `/swagger`._

---

## Fluxo MQTT

```text
Tópico: virtualborder/posto
Payload (JSON):
{
  "mac": "AA:BB:CC:DD:EE:FF",
  "lat": -3.12345,
  "lon": -60.98765,
  "dataHora": "2025-04-24T21:00:00-03:00"
}
```

* **QoS** recomendado: `1`.  
* A leitura é gravada e o painel é atualizado via Server‑Sent Events.

---

## Build offline / NuGet local

1. Na máquina online:  
   ```bash
   dotnet restore --runtime win-x64 --packages ./nupkgs
   ```
2. Copie `nupkgs` para a máquina offline.  
3. Crie `nuget.config`:

   ```xml
   <configuration>
     <packageSources>
       <add key="OfflineFeed" value="C:\nuget\nupkgs" />
     </packageSources>
   </configuration>
   ```

4. Rode `dotnet restore --source OfflineFeed`.

---

## Teste rápido

```bash
mosquitto_pub -h localhost -t virtualborder/posto -m '{
  "mac":"DE:AD:BE:EF:01",
  "lat":-3.1201,
  "lon":-60.0123,
  "dataHora":"'"$(date -Iseconds)"'"
}'
```

Abra o dashboard: o marcador deve piscar em verde.

---

## Roadmap

- [ ] Autenticação JWT  
- [ ] Logs de fronteiras ultrapassadas  
- [ ] Relatórios PDF (tempo em cada zona)  
- [ ] Exportar GPX/KML  
- [ ] Docker multi‑arch (arm64/amd64)  

---

## Contribuindo

1. Fork, crie branch (`feature/…`)  
2. `dotnet test`  
3. Abra Pull Request seguindo o template  

---

## Licença

Distribuído sob a licença **MIT**.  
Consulte `LICENSE` para detalhes.
