using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualBorder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MapsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/maps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map>>> GetMaps()
        {
            try
            {
                var maps = await _context.Map
                    .Select(m => new Map
                    {
                        Id = m.Id,
                        Nome = m.Nome,
                        Imagem = m.Imagem,
                        // Definindo valores constantes aqui, sem enviar para o banco

                    })
                    .ToListAsync();

                if (!maps.Any())
                {
                    return NotFound("Nenhum mapa encontrado.");
                }
                return Ok(maps);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar os mapas: {ex.Message}");
                return StatusCode(500, "Erro interno ao buscar os mapas.");
            }
        }

        // GET: api/maps/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Map>> GetMap(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido fornecido.");
            }

            try
            {
                var map = await _context.Map
                    .Select(m => new Map
                    {
                        Id = m.Id,
                        Nome = m.Nome,
                        Imagem = m.Imagem,
                        // Definindo valores constantes diretamente no objeto retornado

                    })
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (map == null)
                {
                    return NotFound("Mapa não encontrado.");
                }

                return Ok(map);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar o mapa: {ex.Message}");
                return StatusCode(500, "Erro interno ao buscar o mapa.");
            }
        }

        // POST: api/maps
        [HttpPost]
        public async Task<ActionResult<Map>> PostMap([FromForm] string nome, [FromForm] IFormFile imagem)
        {
            if (string.IsNullOrEmpty(nome))
            {
                return BadRequest("O nome do mapa é obrigatório.");
            }

            if (imagem == null || imagem.Length == 0)
            {
                return BadRequest("Imagem é obrigatória.");
            }

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Path.GetRandomFileName() + Path.GetExtension(imagem.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }

                var map = new Map
                {
                    Nome = nome,
                    Imagem = fileName
                    // Não adiciona as propriedades de coordenadas e escalas no banco de dados
                };

                _context.Map.Add(map);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMap), new { id = map.Id }, map);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o mapa: {ex.Message}");
                return StatusCode(500, "Erro interno ao processar o mapa.");
            }
        }

        // PUT: api/maps/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMap(int id, [FromForm] string nome, [FromForm] IFormFile imagem)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido fornecido.");
            }

            var map = await _context.Map.FindAsync(id);
            if (map == null)
            {
                return NotFound("Mapa não encontrado.");
            }

            if (string.IsNullOrEmpty(nome))
            {
                return BadRequest("O nome do mapa é obrigatório.");
            }

            map.Nome = nome;

            if (imagem != null && imagem.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    var oldFilePath = Path.Combine(uploadsFolder, map.Imagem);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    var newFileName = Path.GetRandomFileName() + Path.GetExtension(imagem.FileName);
                    var newFilePath = Path.Combine(uploadsFolder, newFileName);

                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await imagem.CopyToAsync(stream);
                    }

                    map.Imagem = newFileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao salvar a imagem: {ex.Message}");
                    return StatusCode(500, "Erro ao salvar a imagem.");
                }
            }

            _context.Entry(map).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MapExists(id))
                {
                    return NotFound("Mapa não encontrado.");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar o mapa: {ex.Message}");
                return StatusCode(500, "Erro interno ao atualizar o mapa.");
            }

            return NoContent();
        }

        // DELETE: api/maps/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMap(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido fornecido.");
            }

            var map = await _context.Map.FindAsync(id);
            if (map == null)
            {
                return NotFound("Mapa não encontrado.");
            }

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                var filePath = Path.Combine(uploadsFolder, map.Imagem);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Map.Remove(map);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deletar o mapa: {ex.Message}");
                return StatusCode(500, "Erro interno ao deletar o mapa.");
            }
        }

        private bool MapExists(int id)
        {
            return _context.Map.Any(e => e.Id == id);
        }
    }
}
