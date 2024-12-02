using Microsoft.EntityFrameworkCore;
using ServerCentral.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados em memória
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MonitoringDataDB"));

// Adiciona o serviço de controllers para a API
builder.Services.AddControllers();

// Adiciona suporte a Razor Pages (se necessário) e a exibição de arquivos estáticos (HTML, JS, CSS)
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuração do Swagger para testar a API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuração para servir arquivos estáticos (index.html, CSS, JS)
app.UseStaticFiles();  // Serve arquivos dentro da pasta 'wwwroot'

// Configuração de redirecionamento de https
app.UseHttpsRedirection();

// Roteamento para a API
app.MapControllers();

// Roteamento para as Razor Pages ou página estática
app.MapRazorPages();

// Roteamento fallback para servir a página estática 'index.html'
app.MapFallbackToFile("index.html");  // Isso serve o 'index.html' na raiz da aplicação

app.Run();
