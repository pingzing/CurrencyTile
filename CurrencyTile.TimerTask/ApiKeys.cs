namespace CurrencyTile.TimerTask;

internal class ApiKeys
{
    /// <summary>
    /// Loads the given file by name (not full file path) of the embedded resource text file,
    /// reads its contents, and returns them.
    /// Throws exceptions if it can't find the file, or the file is empty or whitespace.
    /// </summary>
    internal static string LoadKey(string fileName)
    {
        Stream? apiKeyStream = typeof(ApiKeys).Assembly.GetManifestResourceStream(
            $"CurrencyTile.TimerTask.{fileName}"
        );

        if (apiKeyStream == null)
        {
            throw new Exception(
                $"Missing the CurrencyTile.TimerTask.{fileName} file that's supposed to contain an API key in the background task project. Go create it!\n"
                    + "(It needs to be an embedded resource, btw!)"
            );
        }

        using TextReader reader = new StreamReader(apiKeyStream);
        string apiKey = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception(
                $"API key read from the file '{fileName}' is null, empty, or whitespace. Fix it!"
            );
        }
        return apiKey;
    }
}
