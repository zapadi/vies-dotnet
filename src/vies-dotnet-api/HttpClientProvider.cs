/*
   Copyright 2017-2024 Adrian Popescu.
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

namespace Padi.Vies;

internal static class HttpClientProvider
{
    /// <summary>
    /// Retrieves an instance of HttpClient.
    /// </summary>
    /// <returns>An instance of HttpClient.</returns>
    #pragma warning disable CA2000 // Dispose objects before losing scope
    public static HttpClient GetHttpClient(HttpClient httpClient = null) =>  httpClient ?? new HttpClient(new ViesHttpClientHandler
    {
        CheckCertificateRevocationList = true,
    })
    {
        Timeout = TimeSpan.FromSeconds(30),
    };
}
