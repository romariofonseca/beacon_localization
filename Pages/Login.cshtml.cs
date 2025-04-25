using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        // Verifica se o ModelState é válido
        if (!ModelState.IsValid)
        {
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Erro de validação: {error.ErrorMessage}");
                }
            }
            ErrorMessage = "Problema com o formulário.";
            Console.WriteLine("Modelo inválido.");
            return Page();
        }

        // Log para depuração
        Console.WriteLine($"Tentativa de login com usuário: {Username}, senha: {Password}");

        // Validação simples de credenciais
        if (Username == "admin" && Password == "senha123")
        {
            Console.WriteLine("Credenciais corretas");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Define se o cookie de autenticação é persistente
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Define a expiração do cookie
            };

            // Faz o login do usuário e cria o cookie de autenticação
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToPage("/SelecionarMapa");
        }

        // Se a autenticação falhar
        ErrorMessage = "Usuário ou senha inválidos.";
        Console.WriteLine("Usuário ou senha inválidos");
        return Page();
    }
}
