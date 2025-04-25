using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace VirtualBorder.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class GpsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Injeção de dependência do GpsContext
        public GpsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/gps/add
        [HttpPost("add")]
        public async Task<IActionResult> AddGpsData([FromBody] GpsData gpsData)
        {
            if (gpsData == null)
            {
                return BadRequest("Dados de GPS inválidos.");
            }

            _context.GpsData.Add(gpsData);
            await _context.SaveChangesAsync();
            return Ok("Dados de GPS adicionados com sucesso.");
        }

        // GET: api/gps/get_all
        [HttpGet("get_all")]
        public async Task<IActionResult> GetAllGpsData()
        {
            var gpsData = await _context.GpsData.ToListAsync();
            return Ok(gpsData);
        }

        // GET: api/gps/get_by_mac/{mac}
        [HttpGet("get_by_mac/{mac}")]
        public async Task<IActionResult> GetGpsDataByMac(string mac)
        {
            var gpsData = await _context.GpsData.Where(g => g.MAC == mac).ToListAsync();
            if (!gpsData.Any())
            {
                return NotFound($"Nenhum dado de GPS encontrado para o MAC: {mac}");
            }

            return Ok(gpsData);
        }

        // GET: api/gps/get_by_date_range?start={start}&end={end}
        [HttpGet("get_by_date_range")]
        public async Task<IActionResult> GetGpsDataByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var gpsData = await _context.GpsData
                                        .Where(g => g.DataHora >= start && g.DataHora <= end)
                                        .ToListAsync();

            if (!gpsData.Any())
            {
                return NotFound("Nenhum dado de GPS encontrado para o intervalo de datas fornecido.");
            }

            return Ok(gpsData);
        }
    }

}



