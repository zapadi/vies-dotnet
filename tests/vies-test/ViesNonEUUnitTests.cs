/*
   Copyright 2017-2022 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

using Xunit;

namespace Padi.Vies.Test
{
    public class ViesNonEUUnitTests
    {
        [Theory]
        [InlineData("GB434031494", true)]
        [InlineData("GBGD001", true)]
        [InlineData("GBHA500", true)]
        [InlineData("GB434031493", false)]
        [InlineData("GB12345", false)]
        [InlineData("GBGD500", false)]
        [InlineData("GBHA100", false)]
        [InlineData("GB12345678", false)]
        public void Should_Validate_GB_Vat(string vatNumber, bool isValid)
        {
            Assert.True(ViesManager.IsValid(vatNumber).IsValid == isValid);
        }
    }
}