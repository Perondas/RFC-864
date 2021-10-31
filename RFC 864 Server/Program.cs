using RFC_864_Server;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var port = context.Configuration.GetValue<int>("ServerSettings:Port");
        services.Configure<ServerOptions>(option => option.Port = port);
        services.AddHostedService<UdpWorker>();
        services.AddHostedService<TcpWorker>();
    })
    .Build();

await host.RunAsync();
