using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace RFC_864_Udp_Client
{
    public class UdpClient : BackgroundService
    {
        private readonly ILogger<UdpClient> _logger;
        private readonly ClientOptions _options;

        public UdpClient(ILogger<UdpClient> logger, IOptions<ClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Udp client starting at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    System.Net.Sockets.UdpClient? client = new System.Net.Sockets.UdpClient(0);
                    try
                    {
                        _logger.LogInformation("Sending request");
                        client.Send(new byte[] { 1, 2 }, _options.Endpoint);
                        _logger.LogInformation("Awaiting reply");
                        try
                        {
                            UdpReceiveResult reply = await client.ReceiveAsync(stoppingToken);
                            string? msg = string.Join(",", reply.Buffer);
                            _logger.LogInformation($"Received: {msg}");
                        }
                        catch
                        {
                            _logger.LogWarning(
                                "Failed to receive message. It is likely that the remote sent back a ICMP Port Unreachable");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send message to remote");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Udp listener");
                }

                await Task.Delay(2000, stoppingToken);
            }
            _logger.LogInformation("Udp client stopping at: {time}", DateTimeOffset.Now);
        }
    }
}