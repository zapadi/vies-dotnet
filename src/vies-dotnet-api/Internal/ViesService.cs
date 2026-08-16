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
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;

namespace Padi.Vies.Internal;

internal sealed class ViesService(HttpClient httpClient) : ViesServiceBase(httpClient, new XmlResponseParser())
{
    private const string SOAP_VALIDATE_VAT_MESSAGE_FORMAT =
        """
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
            <soapenv:Header/>
            <soapenv:Body>
                <ns2:checkVat xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
                    <ns2:countryCode>{0}</ns2:countryCode>
                    <ns2:vatNumber>{1}</ns2:vatNumber>
                </ns2:checkVat>
            </soapenv:Body>
        </soapenv:Envelope>
        """;
    #if NET8_0_OR_GREATER
    private static readonly CompositeFormat validateVatMessageCompositeFormat = CompositeFormat.Parse(SOAP_VALIDATE_VAT_MESSAGE_FORMAT);
    #endif

    private const int MaxFaultBodyReadBytes = 16 * 1024;

    private static readonly Uri viesUri = new(ViesConstants.ViesUri);

    protected override HttpRequestMessage CreateHttpRequestMessage(string countryCode, string vatNumber)
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = viesUri,
            Headers = {{"SOAPAction", string.Empty}},
            Content = CreateContent(countryCode, vatNumber),
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ViesConstants.MediaTypeTextXml));
        return requestMessage;
    }

    protected override async Task<ViesCheckVatResponse> HandleNonSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        // VIES SOAP faults can arrive with HTTP 500 per SOAP 1.1, so buffer a bounded slice of the body and try to parse it.
        var (buffer, length) = await ReadBoundedBodyAsync(response, cancellationToken).ConfigureAwait(false);
        var bufferedText = Encoding.UTF8.GetString(buffer, 0, length).Trim();

        if (!bufferedText.StartsWith('<'))
        {
            throw ViesHttpErrorHandler.CreateException(response.StatusCode, response.ReasonPhrase, bufferedText);
        }

        try
        {
            using (var stream = new MemoryStream(buffer, 0, length, writable: false))
            {
                return await ResponseParser.ParseAsync(stream, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (ViesDeserializationException)
        {
            throw ViesHttpErrorHandler.CreateException(response.StatusCode, response.ReasonPhrase, bufferedText);
        }
    }

    private static async Task<(byte[] Buffer, int Length)> ReadBoundedBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using (Stream stream =
#if (NETSTANDARD2_0 || NETSTANDARD2_1)
            await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
#else
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
#endif
        {
            var buffer = new byte[MaxFaultBodyReadBytes];
            var total = 0;

            while (total < MaxFaultBodyReadBytes)
            {
                var read = await stream.ReadAsync(
#if NETSTANDARD2_0
                    buffer, total, MaxFaultBodyReadBytes - total, cancellationToken).ConfigureAwait(false);
#else
                    buffer.AsMemory(total, MaxFaultBodyReadBytes - total), cancellationToken).ConfigureAwait(false);
#endif
                if (read == 0)
                {
                    break;
                }

                total += read;
            }

            return (buffer, total);
        }
    }

    private static StringContent CreateContent(string countryCode, string vatNumber)
    {
        var content =
        #if(NET8_0_OR_GREATER)
        string.Format(CultureInfo.InvariantCulture, validateVatMessageCompositeFormat, countryCode, vatNumber);
        #else
        string.Format(CultureInfo.InvariantCulture, SOAP_VALIDATE_VAT_MESSAGE_FORMAT, countryCode, vatNumber);
        #endif

        return new StringContent(content, Encoding.UTF8, ViesConstants.MediaTypeTextXml);
    }
}
