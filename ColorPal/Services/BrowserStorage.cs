using ColorPal.Common;
using System.Text.Json;

namespace ColorPal.Services;

public interface IBrowserStorage
{
    Task<T?> GetItemAsync<T>(string key);

    Task SetItemAsync<T>(string key, T value);
}

public sealed class BrowserStorage(JsInterop jsInterop) : IBrowserStorage
{
    public async Task<T?> GetItemAsync<T>(string key)
    {
        string? storedValue = await jsInterop.InvokeAsync<string?>(JsFunctions.GetLocalStorageItem, key);

        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(storedValue);
        }
        catch (JsonException exception) when (exception.Path == "$" && typeof(T) == typeof(string))
        {
            return (T)(object)storedValue;
        }
    }

    public async Task SetItemAsync<T>(string key, T value) =>
        await jsInterop.InvokeVoidAsync(JsFunctions.SetLocalStorageItem, key, JsonSerializer.Serialize(value));
}
