using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace RFC_864_Server
{
    public class TcpWorker : BackgroundService
    {
        private readonly ILogger<TcpWorker> _logger;
        private readonly ServerOptions _options;

        public TcpWorker(ILogger<TcpWorker> logger, IOptions<ServerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TcpWorker starting at: {time}", DateTimeOffset.Now);
            TcpListener listener = new TcpListener(IPAddress.Any, _options.Port);
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Tcp listener");
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Tcp waiting for client");
                TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);
                _logger.LogInformation("Tcp client accepted");
                await Task.Run(() => HandleClient(client, stoppingToken), stoppingToken);
            }

            _logger.LogCritical("TcpWorker terminating");
        }

        private void HandleClient(TcpClient client, CancellationToken stoppingToken)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                while (client.Connected && stream.CanWrite)
                {
                    stream.Write(new byte[] { 1, 2, 3 });
                }
                _logger.LogInformation("Tcp client disconnected gracefully");
            }
            catch
            {
                _logger.LogWarning("The Tcp client ended the connection abruptly");
            }
        }
    }
}