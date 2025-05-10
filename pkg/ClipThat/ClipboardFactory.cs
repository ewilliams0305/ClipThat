namespace ClipThat;

/// <summary>
/// The clipboard factory creates a new instance of a clipboard with the configured options.
/// There are two clipboards supported.  One runs entirely on a RUST background thread while the other runs on a C# Thread.
/// </summary>
public sealed class ClipboardFactory
{
    private bool _foreground = true;
    private TimeSpan _foregroundInterval;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public ClipboardFactory WithForegroundProcessing(TimeSpan interval)
    {
        _foreground = true;
        _foregroundInterval = interval;
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ClipboardFactory WithBackgroundProcessing()
    {
        _foreground = false;
        return this;
    }

    /// <summary>
    /// Creates the new clipboard
    /// </summary>
    /// <param name="exceptionProcessor">Optional processor for exception outlet</param>
    /// <returns>The instance of the configured clipboard</returns>
    public IClipboard Build(Action<string, Exception?>? exceptionProcessor = null) =>
        _foreground
            ? new ForegroundClipboard(_foregroundInterval, exceptionProcessor)
            : new BackgroundClipboard(exceptionProcessor);
}