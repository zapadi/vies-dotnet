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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Padi.Vies.Parsers.Json;

namespace Padi.Vies.Internal;

internal sealed class ViesRestService(HttpClient httpClient) : ViesServiceBase(httpClient, new JsonResponseParser())
{
    private static readonly Uri viesRestUri = new(ViesConstants.ViesRestUri);

    protected override HttpRequestMessage CreateHttpRequestMessage(string countryCode, string vatNumber)
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = viesRestUri,
            Content = CreateContent(countryCode, vatNumber),
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ViesConstants.MediaTypeApplicationJson));
        return requestMessage;
    }

    private static ByteArrayContent CreateContent(string countryCode, string vatNumber)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            new CheckVatRestRequest { CountryCode = countryCode, VatNumber = vatNumber },
            ViesJsonSerializerContext.Default.CheckVatRestRequest);

        return new ByteArrayContent(bytes)
        {
            Headers = { ContentType = new MediaTypeHeaderValue(ViesConstants.MediaTypeApplicationJson) },
        };
    }
}
