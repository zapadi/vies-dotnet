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
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;

namespace Padi.Vies.Parsers.Json;

internal sealed class JsonResponseParser : IResponseParserAsync
{
    public ViesCheckVatResponse Parse(Stream response)
    {
        try
        {
            CheckVatRestResult result = JsonSerializer.Deserialize(response, ViesJsonSerializerContext.Default.CheckVatRestResult);
            return MapResponse(result);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new ViesDeserializationException("The response could not be parsed.", ex);
        }
    }

    public async Task<ViesCheckVatResponse> ParseAsync(Stream response, CancellationToken cancellationToken)
    {
        try
        {
            CheckVatRestResult result = await JsonSerializer
                .DeserializeAsync(response, ViesJsonSerializerContext.Default.CheckVatRestResult, cancellationToken)
                .ConfigureAwait(false);
            return MapResponse(result);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new ViesDeserializationException("The response could not be parsed.", ex);
        }
    }

    private static ViesCheckVatResponse MapResponse(CheckVatRestResult result)
    {
        if (result == null)
        {
            throw new ViesDeserializationException("The response could not be parsed.");
        }

        if (result.ErrorWrappers is { Length: > 0 } || result.ActionSucceed == false)
        {
            throw CreateErrorException(result);
        }

        if (result.Valid == null
            && result.ActionSucceed == null
            && result.ErrorWrappers == null
            && result.CountryCode == null
            && result.VatNumber == null)
        {
            throw new ViesDeserializationException("The response payload does not match the VIES REST contract.");
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
                "VIES service reported failure without providing error details.");
        }

        var fault = wrappers[0].Error;
        if (string.IsNullOrEmpty(fault))
        {
            fault = wrappers[0].Message;
        }

        var (code, _, _) = ViesErrorCodeMapper.Map(fault);

        var details = new string[wrappers.Length];
        for (var i = 0; i < wrappers.Length; i++)
        {
            var wrapper = wrappers[i];
            details[i] = string.IsNullOrEmpty(wrapper.Error) ? wrapper.Message : wrapper.Error;
        }

        var joined = string.Join(", ", details);
        return new ViesServiceException(code, FormattableString.Invariant($"VIES service returned error: {joined}"));
    }

    private static RestErrorWrapper[] FilterWrappers(RestErrorWrapper[] wrappers)
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

        if (count == wrappers.Length)
        {
            return wrappers;
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
}
