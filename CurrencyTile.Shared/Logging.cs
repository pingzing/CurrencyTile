using System.Diagnostics;
using Serilog;
using Windows.Storage;

namespace CurrencyTile.Shared;

public class Logging
{
    private ILogger? _logger = null;

    public async Task<ILogger> GetLogger()
    {
        if (_logger != null)
        {
            return _logger;
        }

        // Note: This won't work if the application every gets created in an unpackaged way.
        // Will have to use real paths then.
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync("log.txt", CreationCollisionOption.OpenIfExists);

        Debug.WriteLine($"Opened log file at: {file.Path}");

        _logger = new LoggerConfiguration()
            .WriteTo.File(file.Path, shared: true, fileSizeLimitBytes: 1024 * 1024 * 10)
            .CreateLogger();

        return _logger;
    }
}
