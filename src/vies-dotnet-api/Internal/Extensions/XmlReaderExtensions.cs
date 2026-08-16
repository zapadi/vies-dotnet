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
using System.Xml;
using Padi.Vies.Errors;

namespace Padi.Vies.Internal.Extensions;

internal static class XmlReaderExtensions
{
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

        return ExceptionDispatcher.ThrowInvalidCast<bool>(value);
    }

    public static DateTimeOffset GetValueAsDateTimeOffset(this XmlReader xmlReader)
    {
        var value = xmlReader.ReadElementContentAsString();

        if (value.AsSpan().TryConvertToDateTimeOffset(out DateTimeOffset result))
        {
            return result;
        }

        return ExceptionDispatcher.ThrowInvalidCast<DateTimeOffset>(value);
    }
}
