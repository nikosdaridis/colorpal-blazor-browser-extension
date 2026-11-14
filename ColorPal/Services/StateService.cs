using ColorPal.Common;
using ColorPal.Common.Models;
using Microsoft.JSInterop;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace ColorPal.Services;

public sealed class StateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StateService> _logger;
    private const int COLOR_NAMES_STEP = 4;
    private const int MAX_CHANNEL_VALUE = 255;
    private const int LEVELS_BELOW_MAX_CHANNEL_VALUE = (MAX_CHANNEL_VALUE - 1) / COLOR_NAMES_STEP + 1;
    private const int LEVEL_COUNT = LEVELS_BELOW_MAX_CHANNEL_VALUE + 1;
    private const int CELL_COUNT = LEVEL_COUNT * LEVEL_COUNT * LEVEL_COUNT;
    private const int MAX_NAME_COUNT = ushort.MaxValue + 1;
    private const int HEADER_BYTES = 8;
    private const int LEVEL_COUNT_OFFSET = 2;
    private const int NAME_COUNT_OFFSET = 4;
    private const char NAME_SEPARATOR = '\n';
    private string[] _colorNames = [];
    private ushort[] _colorNameIndexes = [];
    private Task? _colorNamesLoadTask;

    public StateService(HttpClient httpClient, JsInterop jsInterop, ILogger<StateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _ = jsInterop.InvokeVoidAsync(JsFunctions.InitializeStateService, DotNetObjectReference.Create(this));
    }

    public Task LoadColorNamesAsync() =>
        _colorNamesLoadTask ??= FetchColorNamesAsync();

    private async Task FetchColorNamesAsync()
    {
        try
        {
            byte[] colorNamesData = await _httpClient.GetByteArrayAsync(@$"Data/colorNamesStep{COLOR_NAMES_STEP}.dat");

            (_colorNames, _colorNameIndexes) = ParseColorNames(colorNamesData);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidDataException)
        {
            _logger.LogError(exception, "Loading the color names failed, color names stay empty.");
        }
    }

    private static (string[] ColorNames, ushort[] ColorNameIndexes) ParseColorNames(ReadOnlySpan<byte> colorNamesData)
    {
        int namesEnd = colorNamesData.Length - CELL_COUNT * sizeof(ushort);
        if (namesEnd < HEADER_BYTES)
        {
            throw new InvalidDataException($"The color names file has {colorNamesData.Length} bytes, too few for {CELL_COUNT} colors.");
        }

        int step = BinaryPrimitives.ReadUInt16LittleEndian(colorNamesData);
        int levelCount = BinaryPrimitives.ReadUInt16LittleEndian(colorNamesData[LEVEL_COUNT_OFFSET..]);
        int nameCount = BinaryPrimitives.ReadInt32LittleEndian(colorNamesData[NAME_COUNT_OFFSET..]);
        string[] colorNames = Encoding.UTF8.GetString(colorNamesData[HEADER_BYTES..namesEnd]).Split(NAME_SEPARATOR);
        if (step != COLOR_NAMES_STEP || levelCount != LEVEL_COUNT || nameCount > MAX_NAME_COUNT || colorNames.Length != nameCount)
        {
            throw new InvalidDataException($"The color names file has step {step}, {levelCount} levels and {colorNames.Length} of {nameCount} names.");
        }

        ushort[] colorNameIndexes = new ushort[CELL_COUNT];
        MemoryMarshal.Cast<byte, ushort>(colorNamesData[namesEnd..]).CopyTo(colorNameIndexes);
        if (colorNameIndexes.AsSpan().ContainsAnyExceptInRange((ushort)0, (ushort)(nameCount - 1)))
        {
            throw new InvalidDataException($"The color names file has a name index outside its {nameCount} names.");
        }

        return (colorNames, colorNameIndexes);
    }

    /// <summary>
    /// Finds the closest rounded color name.
    /// </summary>
    [JSInvokable]
    public string FindClosestRoundedColorName(ColorRGB colorRGB)
    {
        if (_colorNameIndexes.Length == 0)
        {
            return string.Empty;
        }

        int cell = (GetLevel(colorRGB.R) * LEVEL_COUNT + GetLevel(colorRGB.G)) * LEVEL_COUNT + GetLevel(colorRGB.B);

        return _colorNames[_colorNameIndexes[cell]];

        static int GetLevel(int channel) =>
            channel >= MAX_CHANNEL_VALUE ? LEVEL_COUNT - 1 : channel / COLOR_NAMES_STEP;
    }
}
