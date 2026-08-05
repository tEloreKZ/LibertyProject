using ProjectLiberty.Abstractions.IPC;
using ProjectLiberty.Abstractions.Logging;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLiberty.Core.IPC;

/// <summary>
/// NamedPipe server for IPC communication.
/// </summary>
public class NamedPipeServer : IIPCService
{
    private readonly ILogger _logger;
    private readonly string _pipeName;
    private NamedPipeServerStream? _pipeServer;
    private bool _isRunning;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listenerTask;

    public event EventHandler<IPCMessageEventArgs>? MessageReceived;

    public NamedPipeServer(ILogger logger, string pipeName = "ProjectLibertyIPC")
    {
        _logger = logger;
        _pipeName = pipeName;
    }

    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("IPC server is already running");
            return;
        }

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _listenerTask = Task.Run(() => ListenAsync(_cancellationTokenSource.Token));
        _logger.LogInformation($"IPC server started on pipe: {_pipeName}");
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        _pipeServer?.Dispose();
        _listenerTask?.Wait(TimeSpan.FromSeconds(5));
        _logger.LogInformation("IPC server stopped");
    }

    public void SendMessage(string message)
    {
        if (_pipeServer == null || !_pipeServer.IsConnected)
        {
            _logger.LogWarning("Cannot send message - no client connected");
            return;
        }

        try
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            _pipeServer.Write(buffer, 0, buffer.Length);
            _pipeServer.Flush();
            _logger.LogDebug($"Sent message: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send IPC message");
        }
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                
                _logger.LogInformation("Waiting for client connection...");
                await _pipeServer.WaitForConnectionAsync(cancellationToken);
                _logger.LogInformation("Client connected");

                await ReadMessagesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IPC server error");
                await Task.Delay(1000, cancellationToken); // Wait before retrying
            }
            finally
            {
                _pipeServer?.Dispose();
            }
        }
    }

    private async Task ReadMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        while (_pipeServer != null && _pipeServer.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var bytesRead = await _pipeServer.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                
                if (bytesRead == 0)
                {
                    break; // Client disconnected
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _logger.LogDebug($"Received message: {message}");
                
                MessageReceived?.Invoke(this, new IPCMessageEventArgs(message));
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading IPC message");
                break;
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
    }
}
