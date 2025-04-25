using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MqttStatusController : ControllerBase
{
    private readonly MqttService _mqttService;

    public MqttStatusController(MqttService mqttService)
    {
        _mqttService = mqttService;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        // Verifica o status do serviço MQTT e retorna a resposta
        bool isConnected = _mqttService.IsConnected();
        if (isConnected)
        {
            return Ok(new { status = "connected" });
        }
        else
        {
            return Ok(new { status = "disconnected" });
        }
    }

    [HttpGet("devices")]
    public IActionResult GetReadDevices()
    {
        // Retorna a lista de dispositivos lidos do tópico MQTT
        var devices = _mqttService.GetReadDevices();
        return Ok(devices);
    }
}
