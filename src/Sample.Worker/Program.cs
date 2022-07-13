using MassTransit;
using Sample.Worker.Consumers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ValidationConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost");

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

await host.RunAsync();