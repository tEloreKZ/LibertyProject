namespace ProjectLiberty.Abstractions.IPC;

/// <summary>
/// IPC service interface for inter-process communication.
/// </summary>
public interface IIPCService
{
    /// <summary>
    /// Starts the IPC server.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the IPC server.
    /// </summary>
    void Stop();

    /// <summary>
    /// Sends a message to the connected client.
    /// </summary>
    /// <param name="message">Message to send.</param>
    void SendMessage(string message);

    /// <summary>
    /// Event raised when a message is received.
    /// </summary>
    event EventHandler<IPCMessageEventArgs> MessageReceived;
}

/// <summary>
/// IPC message event arguments.
/// </summary>
public class IPCMessageEventArgs : EventArgs
{
    public string Message { get; set; }

    public IPCMessageEventArgs(string message)
    {
        Message = message;
    }
}
