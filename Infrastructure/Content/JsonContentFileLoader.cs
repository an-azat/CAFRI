using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace CAFRI.Infrastructure.Content;

public sealed class JsonContentFileLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHostEnvironment _hostEnvironment;

    public JsonContentFileLoader(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public T Load<T>(string relativePath)
    {
        var absolutePath = Path.Combine(_hostEnvironment.ContentRootPath, relativePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Content file '{relativePath}' was not found.", absolutePath);
        }

        using var stream = File.OpenRead(absolutePath);
        var model = JsonSerializer.Deserialize<T>(stream, SerializerOptions);
        if (model is null)
        {
            throw new InvalidOperationException($"Content file '{relativePath}' could not be deserialized into {typeof(T).Name}.");
        }

        return model;
    }
}
