using System;
using System.Threading.Tasks;
using System.Xml;
using Padi.Vies.Extensions;

namespace Padi.Vies.Parsers;

internal static class XmlReaderExtensions
{
    public static T GetValue<T>(this XmlReader xmlReader, IFormatProvider formatProvider = null)
    {
        var val = xmlReader.ReadElementContentAsString();
        return (T) Convert.ChangeType(val, typeof(T), provider: formatProvider);
    }

    public static string GetValueAsString(this XmlReader xmlReader)
    {
        return xmlReader.ReadElementContentAsString();
    }

    public static bool GetValueAsBool(this XmlReader xmlReader)
    {
        var value = xmlReader.ReadElementContentAsString();

        if (value.AsSpan().TryConvertToBool(out var result))
        {
            return result;
        }

        throw new InvalidCastException($"Unable to convert '{value}' to bool");
    }

    public static DateTimeOffset GetValueAsDateTimeOffset(this XmlReader xmlReader)
    {
        var value = xmlReader.ReadElementContentAsString();

        if (value.AsSpan().TryConvertToDateTimeOffset(out DateTimeOffset result))
        {
            return result;
        }

        throw new InvalidCastException($"Unable to convert '{value}' to DateTimeOffset");
    }

    public static bool? GetValueAsNullableBool(this XmlReader xmlReader)
    {
        var val = xmlReader.ReadElementContentAsString();

        return GetNullableBool(val.AsSpan());
    }

    public static async Task<T> GetValueAsync<T>(this XmlReader xmlReader, IFormatProvider formatProvider = null)
    {
        var val = await xmlReader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        return (T) Convert.ChangeType(val, typeof(T), provider: formatProvider);
    }

    public static async Task<bool> GetValueAsBoolAsync(this XmlReader xmlReader)
    {
        var value = await xmlReader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (value.AsSpan().TryConvertToBool(out var result))
        {
            return result;
        }

        throw new InvalidCastException($"Unable to convert '{value}' to bool");
    }

    public static async Task<DateTimeOffset> GetValueAsDateTimeOffsetAsync(this XmlReader xmlReader)
    {
        var value = await xmlReader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (value.AsSpan().TryConvertToDateTimeOffset(out DateTimeOffset result))
        {
            return result;
        }

        throw new InvalidCastException($"Unable to convert '{value}' to DateTimeOffset");
    }

    public static async Task<bool?> GetValueAsNullableBoolAsync(this XmlReader xmlReader)
    {
        var val = await xmlReader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        return GetNullableBool(val.AsSpan());
    }

    public static async Task<string> GetValueAsStringAsync(this XmlReader xmlReader)
    {
        return await xmlReader.ReadElementContentAsStringAsync().ConfigureAwait(false);
    }

    private static bool? GetNullableBool(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return null;
        }

        if (value.TryConvertToBool(out var result))
        {
            return result;
        }

        throw new InvalidCastException($"Unable to convert '{value.ToString()}' to bool");
    }
}
