/*
   Copyright 2017 Adrian Popescu.
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
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;

namespace Zapadi.Vies
{
    public static class ViesExtensions
    {
        public static string GetValue(this string content, string pattern, Func<string, string> func = null)
        {
            var match = Regex.Match(content, pattern);
            if (match.Success)
            {
                var result = match.Groups[1].Value;

                return func != null ? func.Invoke(result) : result;
            }
            return default(string);
        }

        public static int ToInt(this char c)
        {
            return Convert.ToInt32(c) - Convert.ToInt32('0');
        }
    }
}