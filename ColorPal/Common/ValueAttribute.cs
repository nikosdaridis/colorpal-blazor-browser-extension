using System.Collections.Concurrent;
using System.Reflection;

namespace ColorPal.Common;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ValueAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> _enumValueCache = new();

    /// <summary>
    /// Gets the value of enum with ValueAttribute
    /// </summary>
    public static string Value(this Enum field) =>
        _enumValueCache.GetOrAdd(field, key =>
            key.GetType().GetField(key.ToString())?
            .GetCustomAttribute<ValueAttribute>()?.Value ?? string.Empty);

    /// <summary>
    /// Tries to parse string to an enum value based on the ValueAttribute
    /// </summary>
    public static bool TryParse<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        result = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().FirstOrDefault(enumValue => enumValue.Value() == value);

        return !EqualityComparer<TEnum>.Default.Equals(result, default) || value == result.Value();
    }
}
