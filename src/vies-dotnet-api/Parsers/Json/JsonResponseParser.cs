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
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Padi.Vies.Errors;
using Padi.Vies.Internal;

namespace Padi.Vies.Parsers.Json;

internal sealed class JsonResponseParser : IResponseParser
{
    public ViesCheckVatResponse Parse(ReadOnlyMemory<byte> response)
    {
        CheckVatRestResult result;
        try
        {
            var reader = new Utf8JsonReader(TrimBom(response.Span));
            result = ReadResult(ref reader);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return ExceptionDispatcher.ThrowDeserialization<ViesCheckVatResponse>(ex);
        }

        return MapResponse(result);
    }

    private static CheckVatRestResult ReadResult(ref Utf8JsonReader reader)
    {
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            ExceptionDispatcher.ThrowDeserialization(message: "The VIES REST response is not a JSON object.");
        }

        var result = new CheckVatRestResult();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            if (reader.ValueTextEquals(ViesJsonKeys.CountryCode))
            {
                result.CountryCode = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.VatNumber))
            {
                result.VatNumber = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.RequestDate))
            {
                result.RequestDate = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.Valid))
            {
                result.Valid = ReadNullableBool(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.RequestIdentifier))
            {
                result.RequestIdentifier = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.Name))
            {
                result.Name = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.Address))
            {
                result.Address = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.TraderName))
            {
                result.TraderName = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.TraderStreet))
            {
                result.TraderStreet = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.TraderPostalCode))
            {
                result.TraderPostalCode = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.TraderCity))
            {
                result.TraderCity = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.TraderCompanyType))
            {
                result.TraderCompanyType = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.ActionSucceed))
            {
                result.ActionSucceed = ReadNullableBool(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.ErrorWrappers))
            {
                result.ErrorWrappers = ReadErrorWrappers(ref reader);
            }
            else
            {
                reader.Skip();
            }
        }

        return result;
    }

    private static RestErrorWrapper?[]? ReadErrorWrappers(ref Utf8JsonReader reader)
    {
        reader.Read();

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            reader.Skip();
            return null;
        }

        List<RestErrorWrapper?> wrappers = [];

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    wrappers.Add(null);
                    break;
                case JsonTokenType.StartObject:
                    wrappers.Add(ReadErrorWrapper(ref reader));
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return wrappers.ToArray();
    }

    private static RestErrorWrapper ReadErrorWrapper(ref Utf8JsonReader reader)
    {
        var wrapper = new RestErrorWrapper();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            if (reader.ValueTextEquals(ViesJsonKeys.Error))
            {
                wrapper.Error = ReadString(ref reader);
            }
            else if (reader.ValueTextEquals(ViesJsonKeys.Message))
            {
                wrapper.Message = ReadString(ref reader);
            }
            else
            {
                reader.Skip();
            }
        }

        return wrapper;
    }

    private static string? ReadString(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetString();
    }

    private static bool? ReadNullableBool(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            _ => reader.GetBoolean(),
        };
    }

    private static ViesCheckVatResponse MapResponse(CheckVatRestResult? result)
    {
        if (result == null)
        {
            ExceptionDispatcher.ThrowDeserialization();
        }

        if (result.ErrorWrappers is { Length: > 0 } || result.ActionSucceed == false)
        {
            throw CreateErrorException(result);
        }

        if (result.Valid == null && result.ActionSucceed == null && result.ErrorWrappers == null && result.CountryCode == null && result.VatNumber == null)
        {
            ExceptionDispatcher.ThrowDeserialization(message: "The response payload does not match the VIES REST contract.");
        }

        var response = new ViesCheckVatResponse
        {
            CountryCode = result.CountryCode,
            VatNumber = result.VatNumber,
            IsValid = result.Valid ?? false,
            Name = result.Name,
            Address = result.Address,
            RequestIdentifier = result.RequestIdentifier,
            TraderName = result.TraderName,
            TraderStreet = result.TraderStreet,
            TraderPostalCode = result.TraderPostalCode,
            TraderCity = result.TraderCity,
            TraderCompanyType = result.TraderCompanyType,
        };

        if (DateTimeOffset.TryParse(result.RequestDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var requestDate))
        {
            response.RequestDate = requestDate;
        }

        return response;
    }

    private static ViesServiceException CreateErrorException(CheckVatRestResult result)
    {
        RestErrorWrapper[] wrappers = FilterWrappers(result.ErrorWrappers);

        if (wrappers.Length == 0)
        {
            return new ViesServiceException(
                ViesErrorCodes.ServiceError.InvalidResponse.Code,
                InvariantString.Format($"{ViesErrorCodes.ServiceError.InvalidResponse.Message} (VIES reported failure without error details)."));
        }

        var fault = wrappers[0].Error;
        if (string.IsNullOrEmpty(fault))
        {
            fault = wrappers[0].Message;
        }

        var (code, message, userMessage) = ViesErrorCodeMapper.Map(fault);

        var details = new string?[wrappers.Length];
        for (var i = 0; i < wrappers.Length; i++)
        {
            var wrapper = wrappers[i];
            details[i] = string.IsNullOrEmpty(wrapper.Error) ? wrapper.Message : wrapper.Error;
        }

        var joined = string.Join(", ", details);
        return new ViesServiceException(code, InvariantString.Format($"{message} (VIES error: {joined})."), userMessage: userMessage);
    }

    private static RestErrorWrapper[] FilterWrappers(RestErrorWrapper?[]? wrappers)
    {
        if (wrappers == null || wrappers.Length == 0)
        {
            return [];
        }

        var count = 0;
        foreach (var wrapper in wrappers)
        {
            if (wrapper != null)
            {
                count++;
            }
        }

        if (count == 0)
        {
            return [];
        }

        var filtered = new RestErrorWrapper[count];
        var index = 0;
        foreach (var wrapper in wrappers)
        {
            if (wrapper != null)
            {
                filtered[index++] = wrapper;
            }
        }

        return filtered;
    }

    private static ReadOnlySpan<byte> TrimBom(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
        {
            return data[3..];
        }

        return data;
    }
}
