/*
   Copyright 2017-2019 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Padi.Vies
{
    public static class ViesExtensions
    {
        public static string GetValue(this string content, string pattern, Func<string, string> func = null)
        {
            var match = Regex.Match(content, pattern);
            if (!match.Success)
            {
                return default(string);
            }
            
            var result = match.Groups[1].Value;

            return func != null 
                ? func.Invoke(result) 
                : result;
        }

        public static int ToInt(this char c)
        {
            return Convert.ToInt32(c) - Convert.ToInt32('0');
        }

        public static string Sanitize(this string vatNumber)
        {
            if (string.IsNullOrWhiteSpace(vatNumber))
            {
                return vatNumber;
            }
            
            var arr = vatNumber.ToCharArray();

            arr = Array.FindAll(arr, (c => (char.IsLetterOrDigit(c))));
            vatNumber = new string(arr);
            
            return vatNumber
                .ToUpperInvariant()
                .Replace("GR", "EL");
        }

        public static string Slice(this string input, int startIndex)
        {
            return input.Substring(startIndex);
        }
        
        public static string Slice(this string input, int startIndex, int length)
        {
            return input.Substring(startIndex, length);
        }

        public static int Sum(this string input, int[] multipliers, int start = 0)
        {
            var sum = 0;
           
            for (var index = start; index < multipliers.Length; index++)
            {
                var digit = multipliers[index];
                sum += input[index].ToInt() * digit;
            }

            return sum;
        }
    }
}