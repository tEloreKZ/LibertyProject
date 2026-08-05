using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace ProjectLiberty.LibertyScript.IPC;

/// <summary>
/// NamedPipe client for IPC communication with .NET 8.0 system.
/// </summary>
public class NamedPipeClient
{
    private readonly string _pipeName;
    private NamedPipeClientStream? _pipeClient;
    private bool _isConnected;
    private Thread? _listenerThread;
    private bool _isRunning;

    public event Action<string>? MessageReceived;

    public bool IsConnected => _isConnected;

    public NamedPipeClient(string pipeName = "ProjectLibertyIPC")
    {
        _pipeName = pipeName;
    }

    public bool Connect()
    {
        try
        {
            _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
            _pipeClient.Connect(5000); // 5 second timeout

            if (_pipeClient.IsConnected)
            {
                _isConnected = true;
                _isRunning = true;
                _listenerThread = new Thread(ListenThread);
                _listenerThread.IsBackground = true;
                _listenerThread.Start();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to connect to IPC server: {ex.Message}");
            return false;
        }
    }

    public void Disconnect()
    {
        _isRunning = false;
        _isConnected = false;
        
        _listenerThread?.Join(1000);
        _pipeClient?.Dispose();
        _pipeClient = null;
    }

    public void SendMessage(string message)
    {
        if (!_isConnected || _pipeClient == null)
        {
            return;
        }

        try
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            _pipeClient.Write(buffer, 0, buffer.Length);
            _pipeClient.Flush();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send message: {ex.Message}");
        }
    }

    private void ListenThread()
    {
        var buffer = new byte[4096];

        while (_isRunning && _pipeClient != null && _pipeClient.IsConnected)
        {
            try
            {
                var bytesRead = _pipeClient.Read(buffer, 0, buffer.Length);
                
                if (bytesRead == 0)
                {
                    break; // Server disconnected
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading from pipe: {ex.Message}");
                break;
            }
        }

        _isConnected = false;
    }
}
