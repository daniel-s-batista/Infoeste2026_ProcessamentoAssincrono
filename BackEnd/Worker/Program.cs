using Core.Interfaces;
using Core.Models;
using Core.Repositories;
using Core.Services;

namespace Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var configuracaoProcessamento = builder.Configuration
                .GetSection("ProcessamentoNotificacoes")
                .Get<ConfiguracaoProcessamento>() ?? new ConfiguracaoProcessamento();

            var configuracaoServiceBus = builder.Configuration
                .GetSection("ConfiguracaoServiceBus")
                .Get<ConfiguracaoServiceBus>() ?? new ConfiguracaoServiceBus();

            builder.Services.AddSingleton(configuracaoProcessamento);
            builder.Services.AddSingleton(configuracaoServiceBus);
            builder.Services.AddSingleton<INotificacaoRepository, NotificacaoRepository>();
            builder.Services.AddScoped<INotificacaoService>(provider =>
                new NotificacaoService(
                    provider.GetRequiredService<INotificacaoRepository>(),
                    provider.GetRequiredService<ConfiguracaoProcessamento>()
                )
            );

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
