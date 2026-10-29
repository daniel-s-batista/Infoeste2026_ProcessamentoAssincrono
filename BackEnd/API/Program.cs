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

        var configuracaoServiceBus = builder.Configuration
            .GetSection("ConfiguracaoServiceBus")
            .Get<ConfiguracaoServiceBus>() ?? new ConfiguracaoServiceBus();

        builder.Services.AddSingleton(configuracaoProcessamento);
        builder.Services.AddSingleton(configuracaoServiceBus);
        builder.Services.AddSingleton<INotificacaoRepository, NotificacaoRepository>();
        builder.Services.AddSingleton<INotificacaoPublisher, NotificacaoPublisher>();
        builder.Services.AddScoped<INotificacaoService>(provider =>
            new NotificacaoService(
                provider.GetRequiredService<INotificacaoRepository>(),
                provider.GetRequiredService<INotificacaoPublisher>(),
                provider.GetRequiredService<ConfiguracaoProcessamento>()
            )
        );
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
