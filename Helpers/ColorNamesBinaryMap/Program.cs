using MessagePack;
using MessagePack.Resolvers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColorNamesBinaryMap
{
    class Program
    {
        /*
         * This application creates a compressed binary map linking RGB values to their nearest color names
         * RGB values are rounded to a configurable precision step to improve performance
         * The map, built from online JSON data, stores each 32-bit encoded RGB key with its closest color name for O(1) lookup
         */

        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            int step = GetPrecisionStep();
            string inputUri = "https://unpkg.com/color-name-list/dist/colornames.json";
            string outputFilePath = @$"C:\Users\nikos\Desktop\colorNamesStep{step}.dat";

            Console.WriteLine($"⏳ Generating colors map with step = {step}...");
            Console.WriteLine($"📂 Output Path: {outputFilePath}");
            Console.WriteLine(step switch
            {
                <= 2 => "⚠️ Brace yourself, your CPU is about to go turbo",
                <= 5 => "💻 Things are heating up, CPU is getting a workout",
                <= 10 => "🙂 Just a light workout for the CPU",
                _ => "😎 Relaxed mode"
            });

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                ColorNamesBinaryMap colorNamesBinaryMap = new();

                string jsonData = await colorNamesBinaryMap.FetchJsonFromUriAsync(inputUri);
                List<ColorName> colorNames = ColorNamesBinaryMap.ParseColorData(jsonData);
                Dictionary<uint, string> colorsMap = ColorNamesBinaryMap.GenerateRoundedColorMap(colorNames, step);
                int serializedBytes = await ColorNamesBinaryMap.CompressAndSaveDataAsync(colorsMap, outputFilePath);

                stopwatch.Stop();
                Console.WriteLine($"\n👍 Done in {stopwatch.Elapsed.TotalSeconds:F2} seconds!");
                Console.WriteLine($"📦 Total size of saved file: {serializedBytes:N0} bytes ({serializedBytes / 1_048_576.0:F2} MB)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"👎 Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Prompts user to enter RGB precision step value
        /// </summary>
        private static int GetPrecisionStep()
        {
            Console.WriteLine("Enter RGB precision step value (recommended 3-5)");
            Console.WriteLine("Lower values will increase CPU load and file size significantly: ");

            if (!int.TryParse(Console.ReadLine(), out int step) || step < 1 || step > int.MaxValue)
            {
                step = 5;
                Console.WriteLine("⚠️ Invalid input, defaulting to step = 5");
            }

            return step;
        }
    }

    public class ColorName
    {
        public string? Name { get; set; }

        public ColorRGB? RGB { get; set; }
    }

    public class ColorRGB
    {
        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }
    }

    public class ColorNamesBinaryMap()
    {
        private readonly HttpClient _httpClient = new();

        private class InputModel
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("hex")]
            public string? Hex { get; set; }
        }

        /// <summary>
        /// Fetches JSON data from URI
        /// </summary>
        public async Task<string> FetchJsonFromUriAsync(string uri)
        {
            Console.WriteLine($"🌐 HTTP GET to: {uri}");
            string jsonString = await _httpClient.GetStringAsync(uri);
            int bytes = Encoding.UTF8.GetByteCount(jsonString);
            Console.WriteLine($"✅ Successfully retrieved {bytes:N0} bytes ({bytes / 1_048_576.0:F2} MB)");

            return jsonString;
        }

        /// <summary>
        /// Parses JSON data to list of Color Names and RGB values
        /// </summary>
        public static List<ColorName> ParseColorData(string json)
        {
            List<InputModel> inputColors = JsonSerializer.Deserialize<List<InputModel>>(json) ?? [];

            return inputColors.Select(inputColor => new ColorName
            {
                Name = inputColor.Name,
                RGB = HexToRgb(inputColor.Hex)
            }).ToList();

            // Converts hexadecimal color string to RGB
            static ColorRGB HexToRgb(string? hex)
            {
                if (string.IsNullOrWhiteSpace(hex))
                    return new ColorRGB { R = 0, G = 0, B = 0 };

                hex = hex.TrimStart('#');
                return new ColorRGB
                {
                    R = Convert.ToByte(hex[..2], 16),
                    G = Convert.ToByte(hex.Substring(2, 2), 16),
                    B = Convert.ToByte(hex.Substring(4, 2), 16)
                };
            }
        }

        /// <summary>
        /// Generates map of unique rounded colors and their closest named color
        /// </summary>
        public static Dictionary<uint, string> GenerateRoundedColorMap(List<ColorName> colorNames, int step)
        {
            ConcurrentDictionary<uint, string> colorsMap = new();
            ConcurrentDictionary<uint, byte> uniqueColors = GenerateUniqueColors(step);
            PopulateColorsMap(uniqueColors, colorNames, colorsMap);

            return new Dictionary<uint, string>(colorsMap);

            // Generates dictionary of unique rounded colors
            ConcurrentDictionary<uint, byte> GenerateUniqueColors(int step)
            {
                ConcurrentDictionary<uint, byte> uniqueColors = new();
                Parallel.For(0, 256, r =>
                {
                    for (int g = 0; g < 256; g++)
                    {
                        for (int b = 0; b < 256; b++)
                        {
                            ColorRGB roundedColor = RoundColor(new ColorRGB { R = (byte)r, G = (byte)g, B = (byte)b }, step);
                            uint colorKey = EncodeColorKey(roundedColor);
                            uniqueColors.TryAdd(colorKey, 0);
                        }
                    }
                });

                return uniqueColors;
            }

            // Populates the colors map with the closest named color for each unique color
            void PopulateColorsMap(ConcurrentDictionary<uint, byte> uniqueColors, List<ColorName> colorNames, ConcurrentDictionary<uint, string> colorsMap)
            {
                int currentIteration = 0;
                int totalIterations = uniqueColors.Count;
                object consoleLock = new();

                Parallel.ForEach(uniqueColors.Keys, roundedColorKey =>
                {
                    ColorRGB roundedColor = DecodeColorKey(roundedColorKey);
                    ColorName? closestColor = FindClosestColorEuclideanDistance(roundedColor, colorNames);

                    if (closestColor is not null)
                    {
                        colorsMap.TryAdd(roundedColorKey, closestColor.Name!);
                        int iteration = Interlocked.Increment(ref currentIteration);

                        if (iteration % 1000 == 0)
                        {
                            lock (consoleLock)
                            {
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write($"Progress: {iteration:N0}/{totalIterations:N0} ({Math.Min((iteration / (double)totalIterations) * 100, 100):F2}%)");
                            }
                        }
                    }
                });

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine($"Progress: {totalIterations:N0}/{totalIterations:N0} (100.00%)");
            }

            // Finds the closest named color to the given RGB using Euclidean distance
            ColorName? FindClosestColorEuclideanDistance(ColorRGB colorRGB, List<ColorName> colorNames)
            {
                ColorName? closestNamedColor = null;
                double highestSimilarity = double.MinValue;

                foreach (ColorName colorName in colorNames)
                {
                    if (colorName.RGB is null)
                        continue;

                    double similarity = CalculateSimilarity(colorRGB, colorName.RGB);
                    if (similarity > highestSimilarity)
                    {
                        closestNamedColor = colorName;
                        highestSimilarity = similarity;

                        if (highestSimilarity == 100)
                            return closestNamedColor;
                    }
                }

                return closestNamedColor;

                // Calculates similarity between two colors using Euclidean distance
                static double CalculateSimilarity(ColorRGB color1, ColorRGB color2)
                {
                    double rDiff = color1.R - color2.R;
                    double gDiff = color1.G - color2.G;
                    double bDiff = color1.B - color2.B;
                    double squaredDistance = rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;

                    return ((441.67 - Math.Sqrt(squaredDistance)) / 441.67) * 100;
                }
            }

            // Rounds RGB values to the nearest multiple of step min 0, max 255
            ColorRGB RoundColor(ColorRGB color, int step) =>
                new()
                {
                    R = color.R >= 255 ? (byte)255 : (byte)((color.R / step) * step),
                    G = color.G >= 255 ? (byte)255 : (byte)((color.G / step) * step),
                    B = color.B >= 255 ? (byte)255 : (byte)((color.B / step) * step)
                };

            // Encodes RGB to color key
            uint EncodeColorKey(ColorRGB color) =>
               (uint)(color.R << 16 | color.G << 8 | color.B);

            // Decodes color key to RGB
            ColorRGB DecodeColorKey(uint colorKey) =>
                new()
                {
                    R = (byte)(colorKey >> 16 & 0xFF),
                    G = (byte)(colorKey >> 8 & 0xFF),
                    B = (byte)(colorKey & 0xFF)
                };
        }

        /// <summary>
        /// Compresses and saves the data to a file
        /// </summary>
        public static async Task<int> CompressAndSaveDataAsync(Dictionary<uint, string> colorsMap, string filePath)
        {
            byte[] messagePackBytes = MessagePackSerializer.Serialize(colorsMap, ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block));
            await SaveToFileAsync(filePath, messagePackBytes);

            return messagePackBytes.Length;

            // Saves byte array to file
            static async Task SaveToFileAsync(string filePath, byte[] data)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllBytesAsync(filePath, data);
            }
        }
    }
}
