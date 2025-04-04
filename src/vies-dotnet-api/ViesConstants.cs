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

using System.Net.Http.Headers;

namespace Padi.Vies;

public static class ViesConstants
{
    public const string RESPONSE_DATE_FORMAT = "yyyy-MM-dd+hh:mm";

    internal const string MediaTypeTextXml = "text/xml";

    internal const string ViesUri = "https://ec.europa.eu/taxation_customs/vies/services/checkVatService";

    internal static readonly MediaTypeWithQualityHeaderValue MediaTypeHeaderTextXml = new(MediaTypeTextXml);
}
