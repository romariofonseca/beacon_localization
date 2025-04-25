using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;





namespace VirtualBorder.Pages
{

    [Authorize]
    public class MapaModel : PageModel
    {
        public int Id { get; set; }

        public void OnGet(int id)
        {
            Id = id; // Captura o ID do mapa a partir da URL
        }
    }
}


