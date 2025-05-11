using FluentAssertions;

namespace ClipThat.Factory;

public class FactoryTests
{
    [Fact]
    public void Build_WithForeground_Should_ReturnForegroundClipboard()
    {
        using var clipboard = new ClipboardFactory()
            .WithForegroundProcessing(TimeSpan.FromMilliseconds(200))
            .Build();

        clipboard.Should().BeOfType<ForegroundClipboard>();
    }

    [Fact]
    public void Build_WithBackground_Should_ReturnBackgroundClipboard()
    {
        using var clipboard = new ClipboardFactory()
            .WithBackgroundProcessing()
            .Build();

        clipboard.Should().BeOfType<BackgroundClipboard>();
    }
}