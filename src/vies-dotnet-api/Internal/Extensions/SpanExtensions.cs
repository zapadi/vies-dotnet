/*
   Copyright 2017-2025 Adrian Popescu.
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
#if !(NETCOREAPP2_0 || NETSTANDARD2_0)
        input
#else
        input.ToString()
#endif
        , NumberStyles.Number, CultureInfo.InvariantCulture, out no);
    }

    public static bool TryConvertToLong(this Span<char> input, out long no)
    {
        return long.TryParse(
#if !(NETCOREAPP2_0 || NETSTANDARD2_0)
        input
#else
        input.ToString()
#endif
        , NumberStyles.Number, CultureInfo.InvariantCulture, out no);
    }

    public static bool TryConvertToDateTimeOffset(this ReadOnlySpan<char> input, out DateTimeOffset dateTimeOffset)
    {
        return DateTimeOffset.TryParse(
#if !(NETCOREAPP2_0 || NETSTANDARD2_0)
        input
#else
        input.ToString()
#endif
        , CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset);
    }

    public static bool TryConvertToBool(this ReadOnlySpan<char> input, out bool value)
    {
        return bool.TryParse(
#if !(NETCOREAPP2_0 || NETSTANDARD2_0)
        input
#else
        input.ToString()
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

        for (var index = start; index < multipliers.Length; index++)
        {
            var digit = multipliers[index];
            sum += input[index].ToInt() * digit;
        }

        return sum;
    }

    public static int Sum(this Span<char> input, ReadOnlySpan<int> multipliers, int start = 0)
    {
        if (input.IsEmpty)
        {
            return 0;
        }

        if (multipliers.IsEmpty)
        {
            return 0;
        }

        var sum = 0;

        for (var index = start; index < multipliers.Length; index++)
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
}
