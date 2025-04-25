using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
//using VirtualBorder.Models; // Ajuste conforme o namespace do seu modelo Map
using Microsoft.AspNetCore.Authorization;

namespace VirtualBorder.Pages
{
    [Authorize]
    public class CadastroMapaModel : PageModel
    {
        private readonly AppDbContext _context;

        public CadastroMapaModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Map> Mapas { get; set; }

        public async Task OnGetAsync()
        {
            // Carrega os mapas do banco de dados sem tentar acessar colunas inexistentes
            Mapas = await _context.Map
                .Select(m => new Map
                {
                    Id = m.Id,
                    Nome = m.Nome,
                    Imagem = m.Imagem,

                })
                .ToListAsync();
        }
    }
}
