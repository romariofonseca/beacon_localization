using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json; // Necess�rio para serializar o JSON

[ApiController]
[Route("api/[controller]")]
public class GatewaysController : ControllerBase
{
    private readonly GatewayService _gatewayService;

    public GatewaysController(GatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Gateways>>> GetGateways()
    {
        return Ok(await _gatewayService.GetGatewaysAsync());
    }

    [HttpGet("{mac}")]
    public async Task<ActionResult<Gateways>> GetGateway(string mac)
    {
        var gateway = await _gatewayService.GetGatewayByMacAsync(mac);
        if (gateway == null)
        {
            return NotFound();
        }
        return Ok(gateway);
    }

    [HttpPost]
    public async Task<ActionResult> AddGateway([FromBody] Gateways gateway)
    {
        // Log do JSON recebido no console
        Console.WriteLine("JSON Recebido: " + JsonSerializer.Serialize(gateway));

        // Verificar se o ModelState � v�lido
        if (!ModelState.IsValid)
        {
            // Captura e exibe os erros de valida��o do ModelState
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
            Console.WriteLine("Erros de Valida��o: " + string.Join("; ", errors));
            return BadRequest(ModelState);
        }

        try
        {
            // Tenta adicionar o gateway no banco de dados
            await _gatewayService.AddGatewayAsync(gateway);
            return CreatedAtAction(nameof(GetGateway), new { mac = gateway.Mac }, gateway);
        }
        catch (Exception ex)
        {
            // Log detalhado do erro ocorrido durante a adi��o
            Console.WriteLine("Erro ao adicionar gateway: " + ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{mac}")]
    public async Task<ActionResult> UpdateGateway(string mac, [FromBody] Gateways gateway)
    {
        // Log do JSON recebido no console
        Console.WriteLine("JSON Recebido para Atualiza��o: " + JsonSerializer.Serialize(gateway));

        // Verificar se o MAC no URL corresponde ao MAC do objeto
        if (mac != gateway.Mac)
        {
            Console.WriteLine("Erro: MAC do URL n�o corresponde ao MAC do objeto.");
            return BadRequest("MAC do gateway n�o corresponde.");
        }

        // Verificar se o ModelState � v�lido
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
            Console.WriteLine("Erros de Valida��o: " + string.Join("; ", errors));
            return BadRequest(ModelState);
        }

        try
        {
            // Tenta atualizar o gateway no banco de dados
            await _gatewayService.UpdateGatewayAsync(gateway);
            return NoContent();
        }
        catch (Exception ex)
        {
            // Log detalhado do erro ocorrido durante a atualiza��o
            Console.WriteLine("Erro ao atualizar gateway: " + ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{mac}")]
    public async Task<ActionResult> DeleteGateway(string mac)
    {
        // Log do MAC do gateway que ser� exclu�do
        Console.WriteLine($"Solicita��o para deletar o Gateway com MAC: {mac}");

        try
        {
            // Tenta deletar o gateway do banco de dados
            await _gatewayService.DeleteGatewayAsync(mac);
            return NoContent();
        }
        catch (Exception ex)
        {
            // Log detalhado do erro ocorrido durante a exclus�o
            Console.WriteLine("Erro ao deletar gateway: " + ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
