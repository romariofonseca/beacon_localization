using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies; // Para autentica��o





var builder = WebApplication.CreateBuilder(args);

// Registro do MqttService e GatewayService como Scoped (ciclo de vida por requisi��o)
builder.Services.AddScoped<MqttService>();
builder.Services.AddScoped<GatewayService>();

// Obt�m a string de conex�o do arquivo appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"String de Conex�o Utilizada: {connectionString}"); // Exibe a string de conex�o usada no console

// Configura o DbContext para usar SQLite com a string de conex�o correta
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); // Usa SQLite com a string de conex�o

// Registra os controladores e Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// **Configura��o dos servi�os de autentica��o**
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";   // Rota para a p�gina de login
        options.LogoutPath = "/Logout"; // Rota para a p�gina de logout
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
    logging.SetMinimumLevel(LogLevel.Information); // Define o n�vel de log m�nimo para Information
});

var app = builder.Build();

// Configura��es de ambiente e tratamento de erros
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Exibe p�gina de erro detalhado no ambiente de desenvolvimento
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // Adiciona cabe�alhos HSTS para maior seguran�a em produ��o
}

// Middlewares essenciais da aplica��o
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// **Middleware de autentica��o e autoriza��o**
app.UseAuthentication(); // Deve vir antes do UseAuthorization
app.UseAuthorization();

// Mapeia os endpoints para Razor Pages e Controladores
app.MapRazorPages();
app.MapControllers(); // Mapeia os controladores diretamente

app.Run();
