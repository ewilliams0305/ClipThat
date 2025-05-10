using ClipThat;

var run = true;

var clipboard = new ClipboardFactory()
    .WithForegroundProcessing(TimeSpan.FromMilliseconds(200))
    .Build((method, error) =>
{
    Console.WriteLine($"Error From {method}: {error?.Message}");
});

var polledText = clipboard.PollClipboard();

if (!string.IsNullOrEmpty(polledText))
{
    Console.WriteLine($"Polled Keyboard Text: \n", polledText);
}

clipboard.StartMonitoringClipboard(text =>
{
    Console.WriteLine($"CLIPBOARD TEXT RX: \n{text}");
});

while (run)
{
    Console.WriteLine("ENTER TEXT TO CLIPBOARD:");

    var data = Console.ReadLine();
    if (data is not null)
    {
        if(data == "end")
        {
            run = false;
            clipboard.Dispose();
            continue;
        }

        clipboard.SetClipboardText(data);
    }
}

clipboard = new ClipboardFactory()
    .WithForegroundProcessing(TimeSpan.FromMilliseconds(200))
    .Build((method, error) =>
    {
        Console.WriteLine($"Error From {method}: {error?.Message}");
    });

Console.Clear();
Console.WriteLine("RESTARTED NEW SESSION");
run = true;

clipboard.StartMonitoringClipboard(text =>
{
    Console.WriteLine($"CLIPBOARD TEXT RX: \n{text}");
});

while (run)
{
    Console.WriteLine("ENTER TEXT TO CLIPBOARD:");

    var data = Console.ReadLine();
    if (data is not null)
    {
        if (data == "end")
        {
            run = false;
            clipboard.Dispose();
            continue;
        }

        clipboard.SetClipboardText(data);
    }
}

Console.WriteLine("APPLICATION ENDED");
