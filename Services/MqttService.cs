using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MqttService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;
    private readonly HttpClient _httpClient;
    private Timer _statusCheckTimer;
    private bool _isConnected;

    // Armazena os dispositivos lidos em memória (para virtualborder/pdt)
    private readonly ConcurrentDictionary<string, Device> _devicesRead = new ConcurrentDictionary<string, Device>();

    // Construtor que inicializa o HttpClient e configura o MQTT
    public MqttService()
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        // Inicializando HttpClient para enviar requisições à API
        _httpClient = new HttpClient { BaseAddress = new Uri("http://10.241.210.95:5043") }; // Ajuste a URL base e porta conforme necessário

        // Assinando os eventos de conexão, desconexão e mensagem recebida
        _mqttClient.ConnectedAsync += OnConnectedAsync;
        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        // Configurando as opções do cliente MQTT
        _options = new MqttClientOptionsBuilder()
            .WithClientId("VirtualBorderClient")
            .WithTcpServer("10.241.210.95", 1883)
            .WithCleanSession()
            .Build();

        // Inicializa o timer para checar o status do serviço a cada 30 segundos
        _statusCheckTimer = new Timer(async _ => await CheckServiceStatusAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
    {
        Console.WriteLine("Conectado ao Broker MQTT!");
        _isConnected = true;

        // Inscreve-se nos tópicos 'virtualborder/pdt' e 'virtualborder/gps'
        _mqttClient.SubscribeAsync("virtualborder/pdt");
        _mqttClient.SubscribeAsync("virtualborder/gps");
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        Console.WriteLine("Desconectado do Broker MQTT!");
        _isConnected = false;
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        Console.WriteLine($"Mensagem recebida no tópico {e.ApplicationMessage.Topic}: {payload}");

        if (e.ApplicationMessage.Topic == "virtualborder/pdt")
        {
            var deviceData = JsonConvert.DeserializeObject<Device>(payload);

            if (deviceData != null)
            {
                deviceData.Ultima_Leitura = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                if (!string.IsNullOrEmpty(deviceData.Mac) && !string.IsNullOrEmpty(deviceData.Nome))
                {
                    _devicesRead[deviceData.Mac] = deviceData; // Armazena em memória

                    await AddDeviceToDatabaseAsync(deviceData);
                    Console.WriteLine($"Dispositivo com MAC {deviceData.Mac} foi lido e enviado para a API.");
                }
            }
            else
            {
                Console.WriteLine("Falha ao desserializar o payload JSON de 'virtualborder/pdt'.");
            }
        }
        else if (e.ApplicationMessage.Topic == "virtualborder/gps")
        {
            Console.WriteLine("Processando dados do tópico 'virtualborder/gps'...");

            var data = payload.Split(',');
            if (data.Length == 5)
            {
                try
                {
                    // Limpar o valor do MAC removendo o '{'
                    string macAddress = data[0].Trim().Replace("{", ""); // Remove o '{' no início do MAC

                    // Limpar o valor da data, removendo qualquer caractere inesperado
                    string dateString = data[4].Trim().Replace("}", ""); // Remove o '}' no final da string de data

                    // Converte a string para DateTime
                    var gpsData = new GpsData
                    {
                        MAC = macAddress, // Usa o MAC sem o '{'
                        Latitude = double.Parse(data[1].Trim()),
                        Longitude = double.Parse(data[2].Trim()),
                        Altitude = double.Parse(data[3].Trim()),
                        DataHora = DateTime.Parse(dateString) // Conversão da string de data
                    };

                    // Log para verificar os dados
                    Console.WriteLine($"Dados processados do GPS: MAC={gpsData.MAC}, Latitude={gpsData.Latitude}, Longitude={gpsData.Longitude}, Altitude={gpsData.Altitude}, DataHora={gpsData.DataHora}");

                    // Formatar a mensagem JSON conforme especificado
                    var formattedGpsData = new
                    {
                        MAC = gpsData.MAC,
                        latitude = gpsData.Latitude,
                        longitude = gpsData.Longitude,
                        altitude = gpsData.Altitude,
                        dataHora = gpsData.DataHora.ToString("yyyy-MM-ddTHH:mm:ss") // Formata para o formato desejado
                    };

                    // Envia os dados formatados para a API via POST
                    await SendGpsDataToApiAsync(formattedGpsData);
                    Console.WriteLine($"Dados de GPS para MAC {gpsData.MAC} enviados com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar dados do tópico 'virtualborder/gps': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Formato de dados inválido no tópico 'virtualborder/gps'.");
            }
        }
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _mqttClient.ConnectAsync(_options);
            Console.WriteLine("Conexão realizada com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
        }
    }

    private async Task CheckServiceStatusAsync()
    {
        if (!_mqttClient.IsConnected)
        {
            Console.WriteLine("MQTT não está conectado. Tentando reconectar...");
            try
            {
                await _mqttClient.ConnectAsync(_options);
                Console.WriteLine("Reconexão ao MQTT realizada com sucesso!");
                _isConnected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao reconectar ao MQTT: {ex.Message}");
                _isConnected = false;
            }
        }
        else
        {
            Console.WriteLine("Serviço MQTT está funcionando corretamente.");
            _isConnected = true;
        }
    }

    // Método para enviar os dispositivos do tópico 'virtualborder/pdt' para a API
    private async Task AddDeviceToDatabaseAsync(Device device)
    {
        try
        {
            var jsonContent = JsonConvert.SerializeObject(new
            {
                Nome = device.Nome,
                Mac = device.Mac,
                Latitude = device.Latitude,
                Longitude = device.Longitude,
                Ultima_Leitura = device.Ultima_Leitura
            });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/devices/add_device_to_dispositivos", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Dispositivo com MAC {device.Mac} enviado para a API com sucesso.");
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao enviar dispositivo para a API: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar dispositivo para a API: {ex.Message}");
        }
    }

    // Método para enviar os dados de GPS formatados do tópico 'virtualborder/gps' para a API
    private async Task SendGpsDataToApiAsync(object formattedGpsData)
    {
        try
        {
            var jsonContent = JsonConvert.SerializeObject(formattedGpsData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/gps/add", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Dados de GPS enviados com sucesso.");
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao enviar dados de GPS: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar dados de GPS: {ex.Message}");
        }
    }

    // Método para obter o status da conexão MQTT
    public bool IsConnected()
    {
        return _isConnected;
    }

    // Método para obter todos os dispositivos lidos do tópico 'virtualborder/pdt'
    public IEnumerable<Device> GetReadDevices()
    {
        return _devicesRead.Values;
    }
}
