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

using System.Net;
using System.Net.Http;

namespace Padi.Vies.Internal;

internal sealed class ViesHttpClientHandler : HttpClientHandler
{
    public ViesHttpClientHandler()
    {
        CheckCertificateRevocationList = true;
#if NETSTANDARD2_0 || NETSTANDARD2_1
        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;
        #else
        AutomaticDecompression = DecompressionMethods.All;
        #endif
    }
}
