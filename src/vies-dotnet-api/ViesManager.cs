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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Internal;
using Padi.Vies.Validators;

namespace Padi.Vies;

/// <summary>
/// ViesManager offers a way to check if an European Union VAT is valid and/or active.
/// </summary>
/// <remarks>
/// https://en.wikipedia.org/wiki/VAT_identification_number#cite_note-10
/// </remarks>
public sealed class ViesManager : IDisposable
{
    private static readonly Dictionary<string, IVatValidator> VatValidators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AT"] = new AtVatValidator("AT"),
        ["BE"] = new BeVatValidator("BE"),
        ["BG"] = new BgVatValidator("BG"),
        ["CY"] = new CyVatValidator("CY"),
        ["CZ"] = new CzVatValidator("CZ"),
        ["DE"] = new DeVatValidator("DE"),
        ["DK"] = new DkVatValidator("DK"),
        ["EE"] = new EeVatValidator("EE"),
        ["EL"] = new ElVatValidator("EL"),
        ["ES"] = new EsVatValidator("ES"),
        ["FI"] = new FiVatValidator("FI"),
        ["FR"] = new FrVatValidator("FR"),
        ["HR"] = new HrVatValidator("HR"),
        ["HU"] = new HuVatValidator("HU"),
        ["IE"] = new IeVatValidator("IE"),
        ["IT"] = new ItVatValidator("IT"),
        ["LT"] = new LtVatValidator("LT"),
        ["LU"] = new LuVatValidator("LU"),
        ["LV"] = new LvVatValidator("LV"),
        ["MT"] = new MtVatValidator("MT"),
        ["NL"] = new NlVatValidator("NL"),
        ["PL"] = new PlVatValidator("PL"),
        ["PT"] = new PtVatValidator("PT"),
        ["RO"] = new RoVatValidator("RO"),
        ["SE"] = new SeVatValidator("SE"),
        ["SI"] = new SiVatValidator("SI"),
        ["SK"] = new SkVatValidator("SK"),
        ["XI"] = new XiVatValidator("XI"),
    };

    private static readonly Dictionary<string, ExcludedCountryInfo> ExcludedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        {"GB", new ExcludedCountryInfo("GB", "Great Britain", "Brexit", "2021-01-01")},
    };

    private static IVatValidator GetValidator(string countryCode)
    {
        return VatValidators.GetValueOrDefault(countryCode);
    }

    private static string NormalizeCountryCode(string countryCode)
    {
        return string.Equals(countryCode, "GR", StringComparison.OrdinalIgnoreCase) ? "EL" : countryCode;
    }

    private readonly bool _disposeClient;
    private readonly HttpClient _httpClient;
    private readonly IViesService _viesService;

    /// <summary>
    /// Initializes a new instance that calls the VIES REST (JSON) endpoint.
    /// </summary>
    public ViesManager() : this(ViesApiEndpoint.Rest)
    {
    }

    /// <summary>
    /// Initializes a new instance that calls the specified VIES endpoint.
    /// </summary>
    /// <param name="apiEndpoint">The VIES endpoint to call.</param>
    public ViesManager(ViesApiEndpoint apiEndpoint) : this(HttpClientProvider.GetHttpClient(), disposeClient: true, apiEndpoint)
    {
    }

    /// <summary>
    /// Initializes a new instance using the supplied <see cref="HttpClient"/> and the default REST endpoint.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="disposeClient"></param>
    public ViesManager(HttpClient httpClient, bool disposeClient = false) : this(httpClient, disposeClient, ViesApiEndpoint.Rest)
    {
    }

    /// <summary>
    /// Initializes a new instance using the supplied <see cref="HttpClient"/> and the specified VIES endpoint.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="disposeClient"></param>
    /// <param name="apiEndpoint">The VIES endpoint to call.</param>
    public ViesManager(HttpClient httpClient, bool disposeClient, ViesApiEndpoint apiEndpoint)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeClient = disposeClient;
        _viesService = apiEndpoint == ViesApiEndpoint.Soap
            ? new ViesService(httpClient)
            : (IViesService)new ViesRestService(httpClient);
    }

    /// <summary>
    /// Validates a VAT number
    /// </summary>
    /// <param name="vat">The VAT (with country identification) of a registered company</param>
    /// <returns>VatValidationResult</returns>
    public static VatValidationResult IsValid(string vat)
    {
        var (code, number) = SplitInput(vat);

        return IsValid(code, number);
    }

    /// <summary>
    /// Validates a given country code and VAT number
    /// </summary>
    /// <param name="countryCode">The two-character country code of a European member country</param>
    /// <param name="vatNumber">The VAT number (without the country identification) of a registered company</param>
    /// <returns>VatValidationResult</returns>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static VatValidationResult IsValid(string countryCode, string vatNumber)
    {
        countryCode = NormalizeCountryCode(countryCode);

        if (ExcludedCountries.TryGetValue(countryCode, out ExcludedCountryInfo excludedCountryInfo))
        {
            return VatValidationDispatcher.RegionUnsupported(countryCode, excludedCountryInfo.ToString());
        }

        IVatValidator validator = GetValidator(countryCode);
        return validator == null
            ? VatValidationDispatcher.InvalidCountryCode(countryCode, "Not a valid ISO_3166-1 European member state.")
            : validator.Validate(vatNumber);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="countryCode">The two-character country code of a European member country</param>
    /// <param name="vatNumber">The VAT number (without the country identification) of a registered company</param>
    /// <param name="cancellationToken"></param>
    /// <returns>ViesCheckVatResponse</returns>
    /// <exception cref="ViesValidationException"></exception>
    /// <exception cref="ViesServiceException"></exception>
    /// <exception cref="ViesUnsupportedRegionException">Thrown when the country is no longer covered by VIES (for example GB after Brexit).</exception>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public async Task<ViesCheckVatResponse> IsActiveAsync(string countryCode, string vatNumber, CancellationToken cancellationToken = default)
    {
        countryCode = NormalizeCountryCode(countryCode);

        if (ExcludedCountries.TryGetValue(countryCode, out ExcludedCountryInfo excludedCountryInfo))
        {
            ExceptionDispatcher.ThrowUnsupportedRegion(countryCode, excludedCountryInfo.ToString());
        }

        return await _viesService.SendRequestAsync(countryCode, vatNumber, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="vatNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>ViesCheckVatResponse</returns>
    /// <exception cref="ViesValidationException"></exception>
    /// <exception cref="ViesServiceException"></exception>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public async Task<ViesCheckVatResponse> IsActiveAsync(string vatNumber, CancellationToken cancellationToken = default)
    {
        var (code, number) = SplitInput(vatNumber);

        return await IsActiveAsync(code, number, cancellationToken).ConfigureAwait(false);
    }

    private static (string code, string number) SplitInput(string vat)
    {
        vat = vat.Sanitize();

        if (string.IsNullOrWhiteSpace(vat))
        {
            ExceptionDispatcher.ThrowInvalidVatNumber(nameof(vat),"VAT number cannot be null or empty.");
        }

        if (vat.Length < 3)
        {
            ExceptionDispatcher.ThrowInvalidVatNumber(nameof(vat), $"VAT number '{vat}' is too short.");
        }

        var countryCode = vat.Slice(0, 2);
        var vatNumber = vat.Slice(2);

        return (countryCode, vatNumber);
    }

    public void Dispose()
    {
        if (_disposeClient)
        {
            _httpClient?.Dispose();
        }
    }
}
