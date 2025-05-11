using System.Runtime.InteropServices;

namespace ClipThat;

/// <summary>
/// Creates a new monitor clipboard that sends and receives text to and from the clipboard.
/// </summary>
internal sealed class BackgroundClipboard : IClipboard
{
    private Action<string>? _clipboardTextReceived;
    private readonly Action<string, Exception?>? _exceptionProcessor;

    private BackgroundClipboard()
    {

    }

    /// <summary>
    /// Creates a new clipboard with a callback method used to receive the changes in text.
    /// </summary>
    internal BackgroundClipboard(Action<string, Exception?>? exceptionProcessor = null)
    {
        _exceptionProcessor = exceptionProcessor;
    }

    /// <summary>
    /// Starts monitoring the clipboard.
    /// Once started the provided callback will receive text.
    /// <param name="clipboardTextReceived">The text received from the clipboard.</param>
    /// </summary>
    public void StartMonitoringClipboard(Action<string> clipboardTextReceived)
    {
        _clipboardTextReceived = clipboardTextReceived;

        try
        {
            ClipboardBindings.init_clipboard();
            ClipboardBindings.start_clipboard_monitor(ClipboardChanged, ErrorChanged);
        }
        catch (Exception e)
        {
            PublishError(nameof(StartMonitoringClipboard), e);
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
        try
        {
            var strPtr = ClipboardBindings.poll_clipboard_text();
            if (strPtr == IntPtr.Zero)
            {
                return string.Empty;
            }

            var clipboardText = Marshal.PtrToStringAnsi(strPtr) ?? string.Empty;
            ClipboardBindings.free_rust_string(strPtr);

            return clipboardText;
        }
        catch (Exception e)
        {
            PublishError(nameof(PollClipboard), e);
            return string.Empty;
        }
    }

    private void ClipboardChanged(IntPtr textPtr)
    {
        try
        {
            var text = Marshal.PtrToStringAnsi(textPtr);

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _clipboardTextReceived?.Invoke(text);
        }
        catch (Exception e)
        {
            PublishError(nameof(ClipboardChanged), e);
        }
    }

    private void ErrorChanged(IntPtr textPtr)
    {
        try
        {
            var text = Marshal.PtrToStringAnsi(textPtr);

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            PublishError(text);
        }
        catch (Exception e)
        {
            PublishError(nameof(ErrorChanged), e);
        }
    }

    private void PublishError(string message, Exception? error = null) => _exceptionProcessor?.Invoke(message, error);

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            ClipboardBindings.stop_clipboard_monitor();
        }
        catch (Exception e)
        {
            PublishError(nameof(Dispose), e);
        }
    }
}