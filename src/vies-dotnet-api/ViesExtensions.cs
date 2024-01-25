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

        var arr = vatNumber.ToCharArray();

        arr = Array.FindAll(arr, char.IsLetterOrDigit);

        vatNumber = new string(arr);

        return vatNumber.ReplaceString("GR", "EL");
    }

    public static string Slice(this string input, int startIndex)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        #if !(NET5_0_OR_GREATER || NETSTANDARD2_1)
        return input.Substring(startIndex);
        #else
            return input.AsSpan()[startIndex..].ToString();
        #endif
    }

    public static string Slice(this string input, int startIndex, int length)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        #if !(NET5_0_OR_GREATER || NETSTANDARD2_1)
        return input.Substring(startIndex, length);
        #else
            return input.AsSpan().Slice(startIndex, length).ToString();
        #endif
    }

    public static string ReplaceString(this string input, string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        #if !(NET5_0_OR_GREATER || NETSTANDARD2_1)
        return input.ToUpperInvariant().Replace(oldValue, newValue);
        #else
            return input.ToUpperInvariant().Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
        #endif
    }

    public static int Sum(this string input, int[] multipliers, int start = 0)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return 0;
        }

        if (multipliers == null)
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
}
