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
using System.Runtime.CompilerServices;
using Padi.Vies.Internal.Extensions;

namespace Padi.Vies;

internal static class ViesExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this char c) => (int)(uint)(c - '0');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        var count = ComputeSanitizedLength(vatNumberSpan);
        if (count == 0)
        {
            return string.Empty;
        }

#if NETSTANDARD2_0
        char[]? rented = count <= 128 ? null : new char[count];
        Span<char> buffer = rented ?? stackalloc char[count];
        WriteSanitized(buffer, vatNumberSpan);
        return buffer.ToString();
#else
        return string.Create(count, vatNumber, static (span, source) => WriteSanitized(span, source.AsSpan()));
#endif
    }

    private static int ComputeSanitizedLength(ReadOnlySpan<char> source)
    {
        var count = 0;
        var startPos = 0;

        if ((source[0] & ~0x20) == 'G' && (source[1] & ~0x20) == 'R')
        {
            count = 2;
            startPos = 2;
        }

        for (var index = startPos; index < source.Length; index++)
        {
            var ch = source[index];
            if (char.IsLetter(ch) || char.IsDigit(ch))
            {
                count++;
            }
        }

        return count;
    }

    private static void WriteSanitized(Span<char> destination, ReadOnlySpan<char> source)
    {
        var pos = 0;
        var startPos = 0;

        if ((source[0] & ~0x20) == 'G' && (source[1] & ~0x20) == 'R')
        {
            destination[0] = 'E';
            destination[1] = 'L';
            pos = 2;
            startPos = 2;
        }

        for (var index = startPos; index < source.Length; index++)
        {
            var ch = source[index];
            if (char.IsLetter(ch))
            {
                destination[pos++] = (char)(ch & ~0x20);
            }
            else if (char.IsDigit(ch))
            {
                destination[pos++] = ch;
            }
        }
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

    public static int Sum(this string input, int[] multipliers, int start = 0)
    {
        return input.AsSpan().Sum(multipliers, start);
    }
}
