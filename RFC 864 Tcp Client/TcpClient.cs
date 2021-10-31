using Microsoft.Extensions.Options;

namespace RFC_864_Tcp_Client
{
    public class TcpClient : BackgroundService
    {
        private readonly ILogger<TcpClient> _logger;
        private readonly ClientOptions _options;

        public TcpClient(ILogger<TcpClient> logger, IOptions<ClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Tcp client starting at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = new System.Net.Sockets.TcpClient();
                try
                {
                    _logger.LogInformation($"Attempting to connect to {_options.Endpoint.Address}:{_options.Endpoint.Port}");
                    await client.ConnectAsync(_options.Endpoint, stoppingToken);

                    _logger.LogInformation("Began receiving data");
                    await Task.Delay(500, stoppingToken);

                    var stream = client.GetStream();
                    var buffer = new byte[10];
                    try
                    {
                        _logger.LogInformation("Began reading from stream");
                        await stream.ReadAsync(buffer, 0, 10, stoppingToken);
                        var msg = string.Join(",", buffer);
                        _logger.LogInformation($"Read {msg} from stream");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to read from stream");
                    }

                    _logger.LogInformation("Terminating connection");
                    await Task.Delay(250, stoppingToken);
                    client.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect");
                }

                _logger.LogInformation("Waiting 2 seconds");
                await Task.Delay(2000, stoppingToken);
            }
            _logger.LogInformation("Tcp client stopping at: {time}", DateTimeOffset.Now);
        }
    }
}