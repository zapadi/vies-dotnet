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
using System.Diagnostics.CodeAnalysis;
using Padi.Vies.Internal;

namespace Padi.Vies.Errors;

internal static class ExceptionDispatcher
{
    [DoesNotReturn]
    public static void ThrowInvalidVatNumber(string param = null, string userMessage = null)
    {
        throw new ViesValidationException(
            errorCode: ViesErrorCodes.ValidationError.InvalidVatFormat.Code,
            message: ViesErrorCodes.ValidationError.InvalidVatFormat.Message,
            param: param,
            userMessage: userMessage ?? ViesErrorCodes.ValidationError.InvalidVatFormat.UserMessage
        );
    }

    [DoesNotReturn]
    public static void ThrowDeserialization(Exception innerException = null, string message = null)
    {
        throw new ViesDeserializationException(message, innerException);
    }

    [DoesNotReturn]
    public static T ThrowDeserialization<T>(Exception innerException = null, string message = null)
    {
        throw new ViesDeserializationException(message, innerException);
    }

    [DoesNotReturn]
    public static T ThrowInvalidCast<T>(string value)
    {
        var typeName = typeof(T) == typeof(bool) ? "bool" : typeof(T).Name;
        throw new ViesDeserializationException(InvariantString.Format($"Unable to convert '{value}' to {typeName}"));
    }

    [DoesNotReturn]
    public static void ThrowUnsupportedRegion(string countryCode, string userMessage)
    {
        throw new ViesUnsupportedRegionException(
            errorCode: ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Code,
            message: $"{ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Message} (Country: {countryCode}).",
            param: ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Param,
            userMessage: userMessage
        );
    }
}
