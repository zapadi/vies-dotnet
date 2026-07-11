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

using System.Threading.Tasks;
using Xunit;

namespace Padi.Vies.Test;

public sealed class ViesManagerConcurrencyTests
{
    private static readonly string[] SupportedCountries =
    [
        "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL", "ES",
        "FI", "FR", "HR", "HU", "IE", "IT", "LT", "LU", "LV", "MT",
        "NL", "PL", "PT", "RO", "SE", "SI", "SK", "XI"
    ];

    [Fact]
    public void IsValid_IsThreadSafe_UnderConcurrentFirstAccess()
    {
        // Each iteration hits one country code from multiple threads. The cache
        // is process-static so only the *first* access per code can race; running
        // every code through Parallel.For exercises that exact window.
        Parallel.ForEach(SupportedCountries, code =>
        {
            Parallel.For(0, 8, _ =>
            {
                // Return value intentionally ignored — this test only checks that
                // the validator-cache lookup doesn't throw under concurrent first
                // access.
                ViesManager.IsValid(code, "0");
            });
        });
    }
}
