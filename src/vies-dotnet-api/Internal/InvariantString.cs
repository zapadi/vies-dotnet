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
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Padi.Vies.Internal;

/// <summary>
/// Culture-invariant string interpolation.
/// </summary>
internal static class InvariantString
{
#if NET6_0_OR_GREATER
    public static string Format(ref InvariantStringHandler handler)
    {
        return handler.ToStringAndClear();
    }

    [InterpolatedStringHandler]
    internal ref struct InvariantStringHandler(int literalLength, int formattedCount)
    {
        private DefaultInterpolatedStringHandler _inner = new(literalLength, formattedCount, CultureInfo.InvariantCulture);

        public void AppendLiteral(string value) => _inner.AppendLiteral(value);

        public void AppendFormatted(string value) => _inner.AppendFormatted(value);

        public void AppendFormatted(ReadOnlySpan<char> value) => _inner.AppendFormatted(value);

        public void AppendFormatted<T>(T value) => _inner.AppendFormatted(value);

        public void AppendFormatted<T>(T value, string format) => _inner.AppendFormatted(value, format);

        public void AppendFormatted<T>(T value, int alignment) => _inner.AppendFormatted(value, alignment);

        public void AppendFormatted<T>(T value, int alignment, string format) => _inner.AppendFormatted(value, alignment, format);

        public string ToStringAndClear() => _inner.ToStringAndClear();
    }
#else
    public static string Format(FormattableString formattableString)
    {
        return FormattableString.Invariant(formattableString);
    }
#endif
}
