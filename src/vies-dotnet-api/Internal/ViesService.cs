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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;

namespace Padi.Vies.Internal;

internal sealed class ViesService(HttpClient httpClient, IResponseParserAsync parseResponse) : IViesService
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

    private static readonly Uri viesUri = new(ViesConstants.ViesUri);

    public async Task<ViesCheckVatResponse> SendRequestAsync(string countryCode, string vatNumber, CancellationToken cancellationToken)
    {
        try
        {
            using (HttpRequestMessage requestMessage = CreateHttpRequestMessage(viesUri, countryCode, vatNumber))
            {
                using (HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                                           .ConfigureAwait(false))
                {
                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        await HandleExceptionAsync(httpResponseMessage).ConfigureAwait(false);
                    }

                    return await GetViesCheckVatResponseAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            throw new ViesServiceException(
                errorCode: ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
                message: ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
                userMessage: httpRequestException.GetBaseException().Message,
                innerException: httpRequestException
            );
        }
    }

    private async Task<ViesCheckVatResponse> GetViesCheckVatResponseAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
    {
        using(Stream stream =
#if !(NETCOREAPP || NET5_0_OR_GREATER)
         await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
#else
        await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
#endif
        {
            return await parseResponse.ParseAsync(stream).ConfigureAwait(false);
        }
    }

    private static HttpRequestMessage CreateHttpRequestMessage(Uri uri, string countryCode, string vatNumber)
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = uri,
            Headers = {{"SOAPAction", string.Empty}},
            Content = CreateContent(countryCode, vatNumber),
        };
        return requestMessage;
    }

    private static StringContent CreateContent(string countryCode, string vatNumber)
    {
        var content =
        #if(NET8_0_OR_GREATER)
        string.Format(CultureInfo.InvariantCulture, validateVatMessageCompositeFormat, countryCode, vatNumber);
        #else
        string.Format(CultureInfo.InvariantCulture, SOAP_VALIDATE_VAT_MESSAGE_FORMAT, countryCode, vatNumber);
        #endif

        return new StringContent(content, Encoding.UTF8, ViesConstants.MediaTypeTextXml)
        {
            Headers = { ContentType = ViesConstants.MediaTypeHeaderTextXml },
        };
    }

    /// <summary>
    /// 200 = Valid request with an Invalid VAT Number
    /// 201 = Error : INVALID_INPUT
    /// 202 = Error : INVALID_REQUESTER_INFO
    /// 300 = Error : SERVICE_UNAVAILABLE
    /// 301 = Error : MS_UNAVAILABLE
    /// 302 = Error : TIMEOUT
    /// 400 = Error : VAT_BLOCKED
    /// 401 = Error : IP_BLOCKED
    /// 500 = Error : GLOBAL_MAX_CONCURRENT_REQ
    /// 501 = Error : GLOBAL_MAX_CONCURRENT_REQ_TIME
    /// 600 = Error : MS_MAX_CONCURRENT_REQ
    /// 601 = Error : MS_MAX_CONCURRENT_REQ_TIME
    /// For all the other cases, The web service will respond with a "SERVICE_UNAVAILABLE" error.
    /// </summary>
    private static async Task HandleExceptionAsync(HttpResponseMessage response)
    {
        var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        throw new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
            message: ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
            userMessage: ViesErrorCodes.ServiceError.ServiceUnavailable.UserMessage
        );
       // throw new ViesServiceException($"VIES service error: {response.StatusCode:D} - {response.ReasonPhrase}{(!string.IsNullOrWhiteSpace(errorBody) ? $": {errorBody}" : string.Empty)}");
    }
}
