using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace RFC_864_Server
{
    public class UdpWorker : BackgroundService
    {
        private readonly ILogger<UdpWorker> _logger;
        private readonly ServerOptions _options;

        public UdpWorker(ILogger<UdpWorker> logger, IOptions<ServerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UdpWorker starting at: {time}", DateTimeOffset.Now);
            try
            {
                UdpClient listener = new UdpClient(new IPEndPoint(IPAddress.Any, _options.Port));
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Udp waiting for message");
                        UdpReceiveResult result = await listener.ReceiveAsync(stoppingToken);

                        _logger.LogInformation("Received message");
                        Task.Run(() => ReturnMessage(result.RemoteEndPoint), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failure in receiving message. Possibly got a ICMP reply");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open Udp server on port {_options.Port}");
            }
            _logger.LogCritical("UdpWorker terminating");
        }

        private void ReturnMessage(IPEndPoint remoteEndPoint)
        {
            try
            {
                UdpClient sender = new UdpClient();
                _logger.LogInformation($"Sending response to {remoteEndPoint.Address}");
                sender.Send(new byte[] { 1, 2, 3 }, remoteEndPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send response to {remoteEndPoint.Address}");
            }
        }
    }
}