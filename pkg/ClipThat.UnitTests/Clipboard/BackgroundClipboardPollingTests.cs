using FluentAssertions;

namespace ClipThat.Tests;

public class BackgroundClipboardPollingTests
{
    [Theory]
    [InlineData("hi my name is")]
    [InlineData("this is some other string")]
    [InlineData("and yet I don't know what else to type")]
    [InlineData("maybe I should be using bogus?")]
    public void PollClipboard_Should_HaveText_WhenTextWasSet(string text)
    {
        using var clipboard = new ClipboardFactory()
            .WithBackgroundProcessing()
            .Build();

        clipboard.SetClipboardText(text);

        clipboard.PollClipboard().Should().Be(text);
    }
    
    [Theory]
    [InlineData("hi my name is")]
    [InlineData("this is some other string")]
    [InlineData("and yet I don't know what else to type")]
    [InlineData("maybe I should be using bogus?")]
    public void MonitorClipboard_Should_HaveText_WhenTextWasSet(string text)
    {
        using var clipboard = new ClipboardFactory()
            .WithBackgroundProcessing()
            .Build();

        var textReceived = string.Empty;
        using var waitHandle = new ManualResetEventSlim(false);

        clipboard.StartMonitoringClipboard(clip =>
        {
            textReceived = clip;
            if (clip == text)
            {
                // ReSharper disable once AccessToDisposedClosure
                waitHandle.Set();
            }
        });

        // Act
        clipboard.SetClipboardText(text);

        waitHandle.Wait(1000).Should().BeTrue();
        textReceived.Should().Be(text);
    }
}