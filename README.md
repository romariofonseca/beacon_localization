# beacon_localization
Beacon localization+

VirtualBorder <!-- omit in toc -->
Um sistema .NET para monitorar “fronteiras virtuais” em fábricas e armazéns— rastreando dispositivos via MQTT, exibindo-os num mapa em tempo real e expondo uma Web API para integração com MES/WMS.

Estado: Proof-of-Concept - funcional, porém ainda em evolução
Framework alvo: .NET 7 → 8 (compatível com LTS)
Banco de dados: SQLite embutido
Mapa: Leaflet.js + OpenStreetMap

Índice <!-- omit in toc -->
Visão geral

Arquitetura

Funcionalidades principais

Pré-requisitos

Instalação e execução

Configurações

Estrutura de banco de dados

Endpoints REST

Fluxo MQTT

Build offline / NuGet local

Teste rápido

Roadmap

Contribuindo

Licença

Visão geral
O VirtualBorder resolve um problema comum em linhas de produção — saber, em tempo real, onde cada dispositivo móvel (AGV, robô, empilhadeira, etc.) está e se ele ultrapassou zonas restritas (“fronteiras virtuais”).
Ele:

Consome dados de posição via MQTT (virtualborder/posto) enviados por ESP32, PLC ou Gateway.

Persiste leituras em SQLite para auditoria e analytics.

Renderiza um mapa interativo (Leaflet) mostrando:

Última posição & timestamp por MAC.

Histórico de trilha (setas de deslocamento).

Cores verde / laranja / vermelho conforme tempo desde a última leitura.

Expõe uma Web API RESTful para MES/WMS:

Cadastro de mapas (/api/maps)

Importação de ordens & produtos

Consulta de dispositivos e eventos

Arquitetura
text
Copiar
Editar
┌─────────────┐     MQTT       ┌─────────────┐
│  Disposit.  │───(broker)───▶│ VirtualBorder│
│  (ESP/PLC)  │               │  Web API     │
└─────────────┘               │  .NET 8      │
                              └─────┬────────┘
                                    │EF Core
                              ┌─────▼──────┐
                              │  SQLite    │
                              └────────────┘
Camada Web/API: ASP.NET Core Minimal API + Razor Pages (painel).

Camada de dados: Entity Framework Core (code-first).

Serviço MQTT: MQTTnet, rodando em background.

Frontend: Razor + Leaflet + AJAX (fetch API).

Funcionalidades principais

Categoria	Descrição resumida
Rastreamento	Consome tópicos MQTT, grava última posição; atualiza WebSocket/Server-Sent Events para UI.
Mapa dinâmico	Zoom, camadas OSM, filtros por MAC/linha, legenda de latência (cores).
Importação CSV/JSON	Endpoints para ordens (/api/orders) e produtos (/api/items), validador de schema.
Alertas	Trigger opcional para enviar webhook/Telegram quando dispositivo cruza fronteira.
Modo Offline	NuGet local + Docker Compose para broker Mosquitto e banco SQLite gravado em volume.
Pré-requisitos

Software	Versão mínima
.NET SDK	8.0.x (ou 7.0.x)
SQLite	Nenhum client necessário – binário embutido
Node/NPM	Opcional — só para gerar assets front-end
Broker MQTT	Testado com Eclipse Mosquitto 2.x
Instalação e execução
bash
Copiar
Editar
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
A API sobe em http://localhost:5050 (por padrão). A UI pode ser acessada em http://localhost:5050/dashboard.

Docker rápido
bash
Copiar
Editar
docker compose up -d          # inclui Mosquitto + app .NET
Configurações
Arquivo appsettings.json (ou variáveis de ambiente):

jsonc
Copiar
Editar
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
Estrutura de banco de dados

Tabela	Campos principais	Descrição
devices	id, mac, nome, created_at	Cadastro dos dispositivos
maps	id, nome, png_blob	Mapas em PNG para sobrepor
readings	id, mac, lat, lon, dataHora	Últimas leituras (MQTT)
orders	order, client, type, launch, ...	Importações CSV
Rodar dotnet ef migrations add Init caso queira recriar do zero.

Endpoints REST

Método	Rota	Descrição
GET	/api/devices	Lista dispositivos
POST	/api/devices	Cria/edita dispositivo
GET	/api/maps/{id}	Retorna mapa PNG/base64
POST	/api/maps	Faz upload de novo mapa
POST	/api/orders/import	Importa CSV de ordens
GET	/api/readings/latest	Última posição de cada MAC
Swagger UI disponível em /swagger.

Fluxo MQTT
text
Copiar
Editar
Tópico: virtualborder/posto
Payload (JSON):
{
  "mac": "AA:BB:CC:DD:EE:FF",
  "lat": -3.12345,
  "lon": -60.98765,
  "dataHora": "2025-04-24T21:00:00-03:00"
}
QoS recomendado: 1 (at-least-once).

O serviço grava em readings e dispara atualização SSE para o painel.

Build offline / NuGet local
Se a máquina alvo não tem acesso à internet:

Baixe os pacotes na máquina online
dotnet restore --runtime win-x64 --no-dependencies --packages ./nupkgs

Copie a pasta nupkgs para a máquina offline.

Adicione/edite nuget.config:

xml
Copiar
Editar
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="OfflineFeed" value="C:\nuget\nupkgs" />
  </packageSources>
</configuration>
Rode dotnet restore --source OfflineFeed.

Teste rápido
bash
Copiar
Editar
# Publicar fake via MQTT
mosquitto_pub -h 127.0.0.1 -t virtualborder/posto -m '{
  "mac":"DE:AD:BE:EF:01",
  "lat":-3.1201,
  "lon":-60.0123,
  "dataHora":"'"$(date -Iseconds)"'"
}'
Abra o dashboard: o marcador deve piscar em verde.

Roadmap
 Auth/JWT para API externa

 Logs de fronteiras ultrapassadas

 Relatórios PDF (tempo em cada zona)

 Exportar GPX/KML

 Docker multi-arch (arm64/amd64)

Contribuições são bem-vindas!

Contribuindo
Fork & branch (feature/nova-funcionalidade)

dotnet test (garanta que tudo passa)

Abra um Pull Request seguindo o template.

Licença
Distribuído sob a licença MIT. Veja LICENSE para detalhes.
