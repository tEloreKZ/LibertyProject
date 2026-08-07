namespace ProjectLiberty.UI;

public sealed class BrowserNativeException : InvalidOperationException
{
    internal BrowserNativeException(string operation, NativeResult result)
        : base($"Native browser operation '{operation}' failed: {result}.")
    {
        ResultCode = (int)result;
    }

    public int ResultCode { get; }
}

