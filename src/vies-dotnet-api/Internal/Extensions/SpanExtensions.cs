/*
   Copyright 2017-2026 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Globalization;
#if DEBUG
using System.Text;
#endif

namespace Padi.Vies.Internal.Extensions;

internal static class SpanExtensions
{
    public static bool ContainsOnlyDigits(this ReadOnlySpan<char> span)
    {
        foreach (var ch in span)
        {
            if (!char.IsDigit(ch))
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryConvertToInt(this ReadOnlySpan<char> input, out int no)
    {
        return int.TryParse(
#if NETSTANDARD2_0
        input.ToString()
#else
        input
#endif
        , NumberStyles.Number, CultureInfo.InvariantCulture, out no);
    }

    public static bool TryConvertToLong(this ReadOnlySpan<char> input, out long no)
    {
        return long.TryParse(
#if NETSTANDARD2_0
        input.ToString()
#else
        input
#endif
        , NumberStyles.Number, CultureInfo.InvariantCulture, out no);
    }

    public static bool TryConvertToDateTimeOffset(this ReadOnlySpan<char> input, out DateTimeOffset dateTimeOffset)
    {
        return DateTimeOffset.TryParse(
#if NETSTANDARD2_0
        input.ToString()
#else
        input
#endif
        , CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset);
    }

    public static bool TryConvertToBool(this ReadOnlySpan<char> input, out bool value)
    {
        return bool.TryParse(
#if NETSTANDARD2_0
        input.ToString()
#else
        input
#endif
        , out value);
    }

    public static int Sum(this ReadOnlySpan<char> input, ReadOnlySpan<int> multipliers, int start = 0)
    {
        if (input.IsEmpty || input.IsWhiteSpace())
        {
            return 0;
        }

        if (multipliers.IsEmpty)
        {
            return 0;
        }

        var sum = 0;
        var count = Math.Min(input.Length, multipliers.Length);

        for (var index = start; index < count; index++)
        {
            var digit = multipliers[index];
            sum += input[index].ToInt() * digit;
        }

        return sum;
    }

    public static bool ValidateAllDigits(this ReadOnlySpan<char> span, int start = 0, int? length = null)
    {
        var end = length.HasValue ? start + length.Value : span.Length;

        for (var i = start; i < end; i++)
        {
            if (!char.IsDigit(span[i]))
            {
                return false;
            }
        }

        return true;
    }

    #if DEBUG
    public static string AsText(this ReadOnlySpan<byte> bytes) => Encoding.UTF8.GetString(bytes.ToArray());

    public static string AsText(this ReadOnlyMemory<byte> bytes) => Encoding.UTF8.GetString(bytes.ToArray());

    public static string AsText(this byte[] bytes) => Encoding.UTF8.GetString(bytes);

    public static string AsHex(this ReadOnlySpan<byte> bytes) => BitConverter.ToString(bytes.ToArray());
    #endif
}
