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

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;
using Padi.Vies.Parsers.Json;
using Xunit;

namespace Padi.Vies.Test;

public sealed class ViesJsonDeserializationTests
{
    private readonly IResponseParserAsync _parseResponseAsync = new JsonResponseParser();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private const string ValidJson = TestPayloads.ValidJson;

    private const string InvalidVatJson =
        """
        {
          "countryCode": "RO", "vatNumber": "123456789",
          "requestDate": "2026-07-16T10:23:03.183Z",
          "valid": false, "requestIdentifier": "", "name": "", "address": "",
          "traderName": "---", "traderStreet": "---"
        }
        """;

    private const string ErrorJson = TestPayloads.ErrorJson;

    [Fact]
    public async Task Should_Deserialize_Valid_Async()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidJson)))
        {
            ViesCheckVatResponse response = await _parseResponseAsync.ParseAsync(stream, Token);

            Assert.Equal("RO", response.CountryCode);
            Assert.Equal("123456789", response.VatNumber);
            Assert.True(response.IsValid);
            Assert.Equal("ACME SRL", response.Name);
            Assert.Equal("STR. EXAMPLE 1", response.Address);
            Assert.Equal("WAPIAAAAY123", response.RequestIdentifier);
            Assert.Equal("ACME SRL", response.TraderName);
            Assert.Equal("BUCURESTI", response.TraderCity);
            Assert.Equal(2026, response.RequestDate.Year);
        }
    }

    [Fact]
    public void Should_Deserialize_Valid_Sync()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidJson)))
        {
            ViesCheckVatResponse response = _parseResponseAsync.Parse(stream);

            Assert.Equal("RO", response.CountryCode);
            Assert.True(response.IsValid);
            Assert.Equal("WAPIAAAAY123", response.RequestIdentifier);
            Assert.Equal("BUCURESTI", response.TraderCity);
            Assert.Equal(2026, response.RequestDate.Year);
        }
    }

    [Fact]
    public async Task Should_Deserialize_Invalid_Vat_Async()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(InvalidVatJson)))
        {
            ViesCheckVatResponse response = await _parseResponseAsync.ParseAsync(stream, Token);

            Assert.False(response.IsValid);
        }
    }

    [Fact]
    public async Task Should_Throw_ViesServiceException_On_ErrorWrappers()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ErrorJson)))
        {
            ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
                () => _parseResponseAsync.ParseAsync(stream, Token));

            Assert.Equal(ViesErrorCodes.InputError.InvalidInput.Code, exception.ErrorCode);
        }
    }

    [Fact]
    public async Task Should_Throw_ViesDeserializationException_On_Garbage()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is not json")))
        {
            await Assert.ThrowsAsync<ViesDeserializationException>(() => _parseResponseAsync.ParseAsync(stream, Token));
        }
    }

    [Fact]
    public async Task Should_Throw_ViesDeserializationException_On_Empty_Object()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}")))
        {
            await Assert.ThrowsAsync<ViesDeserializationException>(() => _parseResponseAsync.ParseAsync(stream, Token));
        }
    }

    [Fact]
    public async Task Should_Not_Throw_Nre_On_Null_ErrorWrapper_Element()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[null]}""";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
                () => _parseResponseAsync.ParseAsync(stream, Token));

            Assert.Equal(ViesErrorCodes.ServiceError.InvalidResponse.Code, exception.ErrorCode);
        }
    }

    [Fact]
    public async Task Should_Use_Message_When_Error_Is_Null()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[{"message":"Blocked"}]}""";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
                () => _parseResponseAsync.ParseAsync(stream, Token));

            Assert.Contains("Blocked", exception.Message, System.StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Should_Map_ActionSucceed_False_With_Empty_Wrappers_To_InvalidResponse()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[]}""";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
                () => _parseResponseAsync.ParseAsync(stream, Token));

            Assert.Equal(ViesErrorCodes.ServiceError.InvalidResponse.Code, exception.ErrorCode);
        }
    }

    [Fact]
    public async Task Should_Map_RateLimit_Wrapper()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[{"error":"MS_MAX_CONCURRENT_REQ"}]}""";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
                () => _parseResponseAsync.ParseAsync(stream, Token));

            Assert.Equal(ViesErrorCodes.ServiceError.RateLimitExceeded.Code, exception.ErrorCode);
        }
    }
}
