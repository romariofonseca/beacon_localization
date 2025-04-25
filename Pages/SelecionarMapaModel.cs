using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
//using VirtualBorder.Models; // Certifique-se de ajustar o namespace corretamente
using Microsoft.AspNetCore.Authorization;

namespace VirtualBorder.Pages
{
    [Authorize]
    public class SelecionarMapaModel : PageModel
    {
        private readonly AppDbContext _context;

        public SelecionarMapaModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Map> Mapas { get; set; }

        // Constantes para coordenadas e escalas
        public const double DefaultCenterX = 0;
        public const double DefaultCenterY = 0;
        public const double DefaultScaleLat = 100;
        public const double DefaultScaleLon = 100;

        public async Task OnGetAsync()
        {
            // Carrega os mapas do banco de dados sem tentar acessar colunas inexistentes
            Mapas = await _context.Map
                .Select(m => new Map
                {
                    Id = m.Id,
                    Nome = m.Nome,
                    Imagem = m.Imagem
                })
                .ToListAsync();
        }
    }
}
