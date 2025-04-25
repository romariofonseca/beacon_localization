using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VirtualBorder.Pages
{
    public class MapaOutdoorHistoricoModel : PageModel
    {
        public List<Device> Devices { get; set; }

        public async Task OnGetAsync()
        {
            Devices = await FetchDevicesAsync();
        }

        private async Task<List<Device>> FetchDevicesAsync()
        {
            string apiUrl = "http://10.241.210.95:5043/api/gps/get_all"; // Substitua pela URL correta da sua API
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(apiUrl);
                var devices = JsonConvert.DeserializeObject<List<Device>>(response);
                return devices;
            }
        }
    }

    public class Device
    {
        public string Mac { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DataHora { get; set; }
        public double Altitude { get; set; }
    }
}
