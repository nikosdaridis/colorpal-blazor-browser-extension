using ColorPal.Common;
using ColorPal.Common.Models;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.JSInterop;

namespace ColorPal.Services;

public sealed class StateService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<StateService> _logger;
    private const int COLOR_NAMES_STEP = 4;
    private Dictionary<uint, string> _colorNamesMap = [];
    private Task? _colorNamesLoadTask;

    public StateService(HttpClient httpClient, IJSRuntime jsRuntime, ILogger<StateService> logger)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _logger = logger;

        _ = _jsRuntime.InvokeVoidAsync(JsFuncs.InitializeStateService.Value(), DotNetObjectReference.Create(this));
    }

    public Task DecompressParseAndCacheColorNamesAsync() =>
        _colorNamesLoadTask ??= LoadColorNamesAsync();

    private async Task LoadColorNamesAsync()
    {
        try
        {
            byte[] colorNamesData = await _httpClient.GetByteArrayAsync(@$"Data/colorNamesStep{COLOR_NAMES_STEP}.dat");

            _colorNamesMap = MessagePackSerializer.Deserialize<Dictionary<uint, string>>(colorNamesData,
                ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block));
        }
        catch (Exception exception) when (exception is HttpRequestException or MessagePackSerializationException)
        {
            _logger.LogError(exception, "Loading the color names failed, color names stay empty.");
        }
    }

    /// <summary>
    /// Finds the closest rounded color name.
    /// </summary>
    [JSInvokable]
    public string FindClosestRoundedColorName(ColorRGB colorRGB)
    {
        ColorRGB roundedColor = RoundColor(colorRGB, COLOR_NAMES_STEP);
        uint roundedColorKey = GetColorKey(roundedColor);
        if (_colorNamesMap.TryGetValue(roundedColorKey, out string? closestColorName) && closestColorName is not null)
        {
            return closestColorName;
        }

        return string.Empty;

        static ColorRGB RoundColor(ColorRGB color, int step) =>
            new()
            {
                R = color.R >= 255 ? (byte)255 : (byte)((color.R / step) * step),
                G = color.G >= 255 ? (byte)255 : (byte)((color.G / step) * step),
                B = color.B >= 255 ? (byte)255 : (byte)((color.B / step) * step)
            };

        static uint GetColorKey(ColorRGB color) =>
            (uint)(color.R << 16 | color.G << 8 | color.B);
    }
}
