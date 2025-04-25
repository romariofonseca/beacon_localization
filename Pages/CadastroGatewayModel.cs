using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace VirtualBorder.Pages
{

    [Authorize]
    public class CadastroGatewayModel : PageModel
    {
        private readonly GatewayService _gatewayService;

        public List<Gateways> Gateways { get; set; }

        public CadastroGatewayModel(GatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }

        public async Task OnGetAsync()
        {
            Gateways = await _gatewayService.GetGatewaysAsync();
        }
    }
}