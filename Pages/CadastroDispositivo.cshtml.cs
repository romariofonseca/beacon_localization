using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace VirtualBorder.Pages
{

    [Authorize]
    public class CadastroDispositivoModel : PageModel
{
    private readonly AppDbContext _context;

    public List<RegisteredDevice> RegisteredDevice { get; set; }

    [BindProperty]
    public RegisteredDevice NewDevice { get; set; }

    public CadastroDispositivoModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        RegisteredDevice = await _context.RegisteredDevice.ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.RegisteredDevice.Add(NewDevice);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}

}




