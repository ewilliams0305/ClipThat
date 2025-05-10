namespace ClipThat;

/// <summary>
/// Creates a new clipboard monitor that sends and receives text to and from the clipboard.
/// </summary>
public interface IClipboard : IDisposable
{
    /// <summary>
    /// Starts monitoring the clipboard.
    /// Once started the provided callback will receive text.
    /// <param name="clipboardTextReceived">The text received from the clipboard.</param>
    /// </summary>
    void StartMonitoringClipboard(Action<string> clipboardTextReceived);

    /// <summary>
    /// Sets the text in the clipboard.
    /// <remarks>BE CAREFUL, this will also involve the callback as the clipboard change will be detected by the monitor.</remarks>
    /// </summary>
    /// <param name="text">The text to assign to the clipboard.</param>
    void SetClipboardText(string text);

    /// <summary>
    /// Polls the clipboard for current text.
    /// </summary>
    /// <returns>A string containing the current text in the clipboard</returns>
    string PollClipboard();
}