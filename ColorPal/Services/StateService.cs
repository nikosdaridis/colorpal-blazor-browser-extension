using ColorPal.Common;
using ColorPal.Common.Models;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.JSInterop;

namespace ColorPal.Services
{
    public sealed class StateService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private Dictionary<uint, string> _colorNamesMap = [];
        private readonly int _colorNamesStep = 4;

        public StateService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;

            _ = _jsRuntime.InvokeVoidAsync(JsFuncs.InitializeStateService.Value(), DotNetObjectReference.Create(this));
        }

        /// <summary>
        /// Checks if color names have been initialized
        /// </summary>
        public bool ColorNamesInitialized() =>
            _colorNamesMap.Count > 0;

        /// <summary>
        /// Decompresses, parses, and caches color names
        /// </summary>
        public async Task DecompressParseAndCacheColorNamesAsync()
        {
            if (ColorNamesInitialized())
                return;

            try
            {
                byte[] colorNamesData = await _httpClient.GetByteArrayAsync(@$"Data/colorNamesStep{_colorNamesStep}.dat");

                _colorNamesMap = MessagePackSerializer.Deserialize<Dictionary<uint, string>>(colorNamesData,
                    ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block));
            }
            catch
            {
                _colorNamesMap = new() { { 0, string.Empty } };
            }
        }

        /// <summary>
        /// Finds the closest rounded color name
        /// </summary>
        [JSInvokable]
        public string FindClosestRoundedColorName(ColorRGB colorRGB)
        {
            ColorRGB roundedColor = RoundColor(colorRGB, _colorNamesStep);
            uint roundedColorKey = GetColorKey(roundedColor);
            if (_colorNamesMap.TryGetValue(roundedColorKey, out string? closestColorName) && closestColorName is not null)
                return closestColorName;

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
}
