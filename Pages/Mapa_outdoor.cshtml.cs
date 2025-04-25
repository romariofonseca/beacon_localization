using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace VirtualBorder.Pages
{
    public class MapaOutdoorModel : PageModel
    {
        private readonly ILogger<MapaOutdoorModel> _logger;

        public MapaOutdoorModel(ILogger<MapaOutdoorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Mapa-outdoor carregado com sucesso.");
        }
    }
}
