using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualBorder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegisteredDevice>>> GetDevices()
        {
            return await _context.RegisteredDevice.ToListAsync();
        }

        // GET: api/devices/{mac}
        [HttpGet("{mac}")]
        public async Task<ActionResult<RegisteredDevice>> GetDevice(string mac)
        {
            var device = await _context.RegisteredDevice.FindAsync(mac);
            if (device == null)
            {
                return NotFound();
            }
            return device;
        }

        // POST: api/devices
        [HttpPost]
        public async Task<ActionResult<RegisteredDevice>> PostDevice(RegisteredDevice device)
        {
            // Verifica se o dispositivo já existe pelo MAC
            var existingDevice = await _context.RegisteredDevice.FindAsync(device.Mac);
            if (existingDevice != null)
            {
                return Conflict("Um dispositivo com este MAC já existe.");
            }

            _context.RegisteredDevice.Add(device);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDevice), new { mac = device.Mac }, device);
        }

        // PUT: api/devices/{mac}
        [HttpPut("{mac}")]
        public async Task<IActionResult> PutDevice(string mac, RegisteredDevice device)
        {
            if (mac != device.Mac)
            {
                return BadRequest("MAC do dispositivo não corresponde.");
            }

            var existingDevice = await _context.RegisteredDevice.FindAsync(mac);
            if (existingDevice == null)
            {
                return NotFound();
            }

            // Atualiza os campos do dispositivo
            existingDevice.Nome = device.Nome;
            existingDevice.Latitude = device.Latitude;
            existingDevice.Longitude = device.Longitude;
            existingDevice.Sn = device.Sn;

            _context.Entry(existingDevice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(mac))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/devices/{mac}
        [HttpDelete("{mac}")]
        public async Task<IActionResult> DeleteDevice(string mac)
        {
            var device = await _context.RegisteredDevice.FindAsync(mac);
            if (device == null)
            {
                return NotFound();
            }

            _context.RegisteredDevice.Remove(device);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Método para adicionar um dispositivo na tabela 'dispositivos'
        [HttpPost("add_device_to_dispositivos")]
        public async Task<ActionResult<Device>> AddDeviceToDispositivos([FromBody] Device device)
        {
            // Log para verificar se o método está sendo acessado
            Console.WriteLine("Método AddDeviceToDispositivos chamado.");

            if (device == null)
            {
                Console.WriteLine("Dispositivo é nulo.");
                return BadRequest("Dispositivo inválido.");
            }

            if (string.IsNullOrEmpty(device.Mac))
            {
                Console.WriteLine("MAC do dispositivo não fornecido.");
                return BadRequest("MAC não fornecido.");
            }

            try
            {
                Console.WriteLine($"Tentando salvar dispositivo com MAC: {device.Mac}");
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Dispositivo com MAC {device.Mac} foi inserido com sucesso na tabela 'dispositivos'.");
                return Ok($"Dispositivo com MAC {device.Mac} foi inserido com sucesso na tabela 'dispositivos'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar o dispositivo: {ex.Message}");
                return StatusCode(500, $"Erro ao salvar o dispositivo: {ex.Message}");
            }
        }

        // Verifica se o dispositivo existe
        private bool DeviceExists(string mac)
        {
            return _context.RegisteredDevice.Any(e => e.Mac == mac);
        }
    }
}
