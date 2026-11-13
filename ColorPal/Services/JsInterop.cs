using ColorPal.Common;
using Microsoft.JSInterop;

namespace ColorPal.Services;

public sealed class JsInterop(IJSRuntime jsRuntime, ILogger<JsInterop> logger)
{
    public async Task<T> InvokeAsync<T>(JsFunctions function, params object?[]? arguments)
    {
        try
        {
            return await jsRuntime.InvokeAsync<T>(function.Value(), arguments);
        }
        catch (JSException exception)
        {
            LogFailure(exception, function);
            throw;
        }
    }

    public async Task InvokeVoidAsync(JsFunctions function, params object?[]? arguments)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(function.Value(), arguments);
        }
        catch (JSException exception)
        {
            LogFailure(exception, function);
            throw;
        }
    }

    private void LogFailure(JSException exception, JsFunctions function) =>
        logger.LogError(exception, "JavaScript function {Function} failed.", function.Value());
}
