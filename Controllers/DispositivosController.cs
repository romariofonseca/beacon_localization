using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DevicesController(AppDbContext context)
    {
        _context = context;
    }

    // API para trazer todos os dispositivos cadastrados
    [HttpGet("get_all_devices")]
    public async Task<IActionResult> GetAllDevices()
    {
        // Busca todos os dispositivos na tabela 'dispositivos'
        List<RegisteredDevice> devices = await _context.RegisteredDevice.ToListAsync();
        return Ok(devices);
    }

    // API para trazer todos os gateways
    [HttpGet("get_all_gateways")]
    public async Task<IActionResult> GetAllGateways()
    {
        // Busca todos os gateways na tabela 'gateways'
        List<Gateways> gateways = await _context.Gateways.ToListAsync();
        return Ok(gateways);
    }
}
