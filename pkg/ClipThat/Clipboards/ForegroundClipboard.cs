using System.Runtime.InteropServices;

namespace ClipThat;

/// <summary>
/// Creates a new monitor clipboard that sends and receives text to and from the clipboard.
/// </summary>
internal sealed class ForegroundClipboard : IClipboard
{
    private Action<string>? _clipboardTextReceived;
    private bool _started;

    private readonly object _startLocker = new object();
    private readonly TimeSpan _interval;
    private readonly Action<string, Exception?>? _exceptionProcessor;

    private string? _clippedText;
    private IntPtr _polledStringPointer;

    private ForegroundClipboard()
    {

    }

    /// <summary>
    /// Creates a new clipboard with a callback method used to receive the changes in text.
    /// </summary>
    internal ForegroundClipboard(TimeSpan interval, Action<string, Exception?>? exceptionProcessor = null)
    {
        _interval = interval;
        _exceptionProcessor = exceptionProcessor;
    }

    /// <inheritdoc />
    public void StartMonitoringClipboard(Action<string> clipboardTextReceived)
    {
        lock (_startLocker)
        {
            if (_started)
            {
                return;
            }
            _clipboardTextReceived = clipboardTextReceived;
            _started = true;
            try
            {
                ClipboardBindings.init_clipboard();

                var pollingThread = new Thread(PollingExecutionMethod)
                {
                    IsBackground = true,
                    Name = "clipboard_polling_thread",
                    Priority = ThreadPriority.Lowest
                };

                pollingThread.Start();
            }
            catch (Exception e)
            {
                PublishError(nameof(StartMonitoringClipboard), e);
            }
        }
    }

    private void PollingExecutionMethod()
    {
        while(_started)
        {
            Thread.Sleep(_interval);

            var clipboardText = PollClipboard();

            if (string.IsNullOrEmpty(clipboardText) || _clippedText == clipboardText)
            {
                continue;
            }

            _clippedText = clipboardText;
            _clipboardTextReceived?.Invoke(_clippedText);
        }
    }

    /// <inheritdoc />
    public void SetClipboardText(string text)
    {
        try
        {
            ClipboardBindings.set_clipboard_text(text);
        }
        catch (Exception e)
        {
            PublishError(nameof(SetClipboardText), e);
        }
    }

    /// <inheritdoc />
    public string PollClipboard()
    {
        lock (_startLocker)
        {
            try
            {
                _polledStringPointer = ClipboardBindings.poll_clipboard_text();
                if (_polledStringPointer == IntPtr.Zero)
                {
                    return string.Empty;
                }

                var clipboardText = Marshal.PtrToStringAnsi(_polledStringPointer) ?? string.Empty;


                return clipboardText;
            }
            catch (Exception e)
            {
                PublishError(nameof(PollClipboard), e);
                return string.Empty;
            }
            finally
            {
                if (_polledStringPointer != IntPtr.Zero)
                {
                    ClipboardBindings.free_rust_string(_polledStringPointer);
                    _polledStringPointer = IntPtr.Zero;
                }
            }
        }
    }

    private void PublishError(string message, Exception? error = null) => _exceptionProcessor?.Invoke(message, error);

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_startLocker)
        {
            if (!_started)
            {
                return;
            }

            _started = false;

            if (_polledStringPointer != IntPtr.Zero)
            {
                ClipboardBindings.free_rust_string(_polledStringPointer);
            }
        }
    }
}
