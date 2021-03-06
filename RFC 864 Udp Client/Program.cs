using RFC_864_Udp_Client;
using System.Net;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string? address = context.Configuration.GetValue<string>("ClientSettings:IpAddress");
        int port = context.Configuration.GetValue<int>("ClientSettings:Port");
        services.Configure<ClientOptions>(options =>
            options.Endpoint = new IPEndPoint(IPAddress.Parse(address), port));
        services.AddHostedService<UdpClient>();
    })
    .Build();

await host.RunAsync();
