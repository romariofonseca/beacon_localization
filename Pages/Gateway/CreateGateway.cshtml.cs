using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace VirtualBorder.Pages.Gateway
{
    public class GatewaysModel : PageModel
    {
        private readonly GatewayService _gatewayService;

        public GatewaysModel(GatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }

        [BindProperty]
        public Gateways Gateway { get; set; } // Modelo para vinculação dos dados do formulário

        public List<Gateways> Gateways { get; set; }

        // Método GET para carregar a página e exibir a lista de gateways
        public async Task OnGetAsync()
        {
            Gateways = await _gatewayService.GetGatewaysAsync();
        }

        // Método POST para lidar com o envio do formulário
        public async Task<IActionResult> OnPostCreateGatewayAsync()
        {
            if (!ModelState.IsValid)
            {
                // Validação dos dados do formulário
                return Page();
            }

            try
            {
                await _gatewayService.AddGatewayAsync(Gateway); // Adiciona o novo gateway ao banco de dados
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message); // Exibe erros de validação
                return Page();
            }

            // Redireciona para a página atual para evitar reenvio do formulário ao recarregar a página
            return RedirectToPage();
        }

        // Método POST para deletar um gateway
        public async Task<IActionResult> OnPostDeleteGatewayAsync(string mac)
        {
            await _gatewayService.DeleteGatewayAsync(mac);
            return RedirectToPage();
        }
    }
}