using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarkersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MarkersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/markers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetMarkers()
        {
            try
            {
                // Consulta os últimos 500 dispositivos ordenados por 'ultima_leitura' em ordem decrescente
                var devices = await _context.Devices
                    .OrderByDescending(d => d.Ultima_Leitura)
                    .Take(500)
                    .ToListAsync();

                if (devices == null || !devices.Any())
                {
                    return NotFound("Nenhum dispositivo encontrado.");
                }

                // Log dos dispositivos encontrados (para diagnóstico)
                foreach (var device in devices)
                {
                    Console.WriteLine($"Dispositivo: {device.Nome}, Latitude: {device.Latitude}, Longitude: {device.Longitude}");
                }

                return Ok(devices);
            }
            catch (Exception ex)
            {
                // Log do erro para diagnóstico
                Console.WriteLine($"Erro ao ler os dispositivos: {ex.Message}");
                return StatusCode(500, "Erro ao ler os dispositivos.");
            }
        }
    }
}
