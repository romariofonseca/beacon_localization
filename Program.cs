using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies; // Para autenticação





var builder = WebApplication.CreateBuilder(args);

// Registro do MqttService e GatewayService como Scoped (ciclo de vida por requisição)
builder.Services.AddScoped<MqttService>();
builder.Services.AddScoped<GatewayService>();

// Obtém a string de conexão do arquivo appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"String de Conexão Utilizada: {connectionString}"); // Exibe a string de conexão usada no console

// Configura o DbContext para usar SQLite com a string de conexão correta
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); // Usa SQLite com a string de conexão

// Registra os controladores e Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// **Configuração dos serviços de autenticação**
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";   // Rota para a página de login
        options.LogoutPath = "/Logout"; // Rota para a página de logout
    });

// Configura o Kestrel para escutar em todos os IPs na porta especificada
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5043);  // Porta 5043 para HTTP
});

// Configura o logging para exibir logs no console e no debug
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information); // Define o nível de log mínimo para Information
});

var app = builder.Build();

// Configurações de ambiente e tratamento de erros
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Exibe página de erro detalhado no ambiente de desenvolvimento
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // Adiciona cabeçalhos HSTS para maior segurança em produção
}

// Middlewares essenciais da aplicação
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// **Middleware de autenticação e autorização**
app.UseAuthentication(); // Deve vir antes do UseAuthorization
app.UseAuthorization();

// Mapeia os endpoints para Razor Pages e Controladores
app.MapRazorPages();
app.MapControllers(); // Mapeia os controladores diretamente

app.Run();
