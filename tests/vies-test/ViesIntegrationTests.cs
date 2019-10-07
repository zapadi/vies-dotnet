using System.Threading.Tasks;
using Xunit;

namespace Padi.Vies.Test
{
    [Collection("ViesCollection")]
    public class ViesIntegrationTests
    {
        private readonly ViesManagerFixture _fixture;

        public ViesIntegrationTests(ViesManagerFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("LU26375245")]
        [InlineData("SE 556656688001")]
        [InlineData("FI 09073468")]
        [InlineData("NL 107452613B01")]
        [InlineData("FR 66322120916")]
        [InlineData("IT 01640320360")]
        [InlineData("RO26129093")]
        [InlineData("SK2120046819")]
        public async Task Should_Return_Vat_Active(string vat)
        {
            var actual = await _fixture.ViesManager.IsActive(vat );

            Assert.True(actual.IsValid, "Inactive vat number");
        }
        
        [Theory]
        [InlineData("RO123456789")]
        [InlineData("ATU12345675")]
        [InlineData("CZ612345670")]
        [InlineData("ESK1234567L")]
        [InlineData("IE1234567T")]
        [InlineData("NL123456782B90")]
        public async Task Should_Return_Vat_Inactive(string vat)
        {
            var actual = await _fixture.ViesManager.IsActive(vat);

            Assert.False(actual.IsValid, "Inactive vat number");
        }
    }
}