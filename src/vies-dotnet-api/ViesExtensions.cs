/*
   Copyright 2017-2024 Adrian Popescu.
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

namespace Padi.Vies;

public static class ViesExtensions
{
    public static int ToInt(this char c) => (int)(uint)(c - '0');

    public static bool IsAsciiDigit(this char c) => (uint)(c - '0') <= 9;

    public static string Sanitize(this string vatNumber)
    {
        if (string.IsNullOrWhiteSpace(vatNumber))
        {
            return vatNumber;
        }

        ReadOnlySpan<char> vatNumberSpan = vatNumber.AsSpan();

        if (vatNumberSpan.Length <= 1)
        {
            return vatNumber;
        }

        Span<char> result = stackalloc char[vatNumberSpan.Length];
        var startPos = 0;
        if ((vatNumberSpan[0] & ~0x20) == 'G' && (vatNumberSpan[1] & ~0x20)== 'R')
        {
            result[0] = 'E';
            result[1] = 'L';
            startPos = 2;
        }

        for (var index = startPos; index < vatNumberSpan.Length; index++)
        {
            if (char.IsLetter(vatNumberSpan[index]))
            {
                result[startPos++] = (char)(vatNumberSpan[index] & ~0x20);
            }
            else
            {
                if (char.IsDigit(vatNumberSpan[index]))
                {
                    result[startPos++] = vatNumberSpan[index];
                }
            }
        }

        return result[..startPos].ToString();
    }

    public static string Slice(this string input, int startIndex)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        #if (NET5_0_OR_GREATER || NETSTANDARD2_1)
        return input.AsSpan()[startIndex..].ToString();
        #else
        return input[startIndex..];
        #endif
    }

    public static string Slice(this string input, int startIndex, int length)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        #if (NET5_0_OR_GREATER || NETSTANDARD2_1)
        return input.AsSpan().Slice(startIndex, length).ToString();
        #else
        return input.Substring(startIndex, length);
        #endif
    }

    [Obsolete("This method is obsolete and will be removed in a future version.")]
    public static string ReplaceString(this string input, string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        #if (NET5_0_OR_GREATER || NETSTANDARD2_1)
        return input.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
        #else
        return input.ToUpperInvariant().Replace(oldValue, newValue);
        #endif
    }

    public static int Sum(this string input, int[] multipliers, int start = 0)
    {
        return input.AsSpan().Sum(multipliers, start);
    }
}
