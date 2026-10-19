using Core.Interfaces;
using Core.Models;
using Core.Repositories;
using Core.Services;
using System.Text.Json.Serialization;

namespace API;

public class Program
{
    private const string PoliticaCorsFrontend = "FrontendLocal";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuracaoProcessamento = builder.Configuration
            .GetSection("ProcessamentoNotificacoes")
            .Get<ConfiguracaoProcessamento>() ?? new ConfiguracaoProcessamento();

        builder.Services.AddSingleton(configuracaoProcessamento);
        builder.Services.AddSingleton<INotificacaoRepository, NotificacaoRepository>();
        builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
        builder.Services.AddControllers().AddJsonOptions(opcoes =>
        {
            opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddCors(opcoes =>
        {
            opcoes.AddPolicy(PoliticaCorsFrontend, politica =>
            {
                politica
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        app.UseCors(PoliticaCorsFrontend);
        app.MapControllers();

        app.Run();
    }
}
