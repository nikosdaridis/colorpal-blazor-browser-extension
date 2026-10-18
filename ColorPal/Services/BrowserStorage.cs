using ColorPal.Common;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ColorPal.Services;

public interface IBrowserStorage
{
    Task<T?> GetItemAsync<T>(string key);

    Task SetItemAsync<T>(string key, T value);
}

public sealed class BrowserStorage(IJSRuntime jsRuntime) : IBrowserStorage
{
    public async Task<T?> GetItemAsync<T>(string key)
    {
        string? storedValue = await jsRuntime.InvokeAsync<string?>(JsFuncs.GetLocalStorageItem.Value(), key);

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
        await jsRuntime.InvokeVoidAsync(JsFuncs.SetLocalStorageItem.Value(), key, JsonSerializer.Serialize(value));
}
