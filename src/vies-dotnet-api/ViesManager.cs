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
using Padi.Vies.Parsers;
using Padi.Vies.Validators;

namespace Padi.Vies;

/// <summary>
/// ViesManager offers a way to check if an European Union VAT is valid and/or active.
/// </summary>
/// <remarks>
/// https://en.wikipedia.org/wiki/VAT_identification_number#cite_note-10
/// http://sima.cat/nif.php
/// </remarks>
public sealed class ViesManager : IDisposable
{
    private static readonly Dictionary<string, IVatValidator> VatValidators = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, ExcludedCountryInfo> ExcludedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        {"GB", new ExcludedCountryInfo("GB", "Great Britain", "Brexit", "2021-01-01")},
    };

    private static IVatValidator GetValidator(string countryCode)
    {
        if (VatValidators.TryGetValue(countryCode, out IVatValidator validator))
        {
            return validator;
        }

        validator = countryCode.AsSpan() switch
        {
            "AT" => new AtVatValidator(countryCode),
            "BE" => new BeVatValidator(countryCode),
            "BG" => new BgVatValidator(countryCode),
            "CY" => new CyVatValidator(countryCode),
            "CZ" => new CzVatValidator(countryCode),
            "DE" => new DeVatValidator(countryCode),
            "DK" => new DkVatValidator(countryCode),
            "EE" => new EeVatValidator(countryCode),
            "EL" => new ElVatValidator(countryCode),
            "ES" => new EsVatValidator(countryCode),
            "FI" => new FiVatValidator(countryCode),
            "FR" => new FrVatValidator(countryCode),
            "HR" => new HrVatValidator(countryCode),
            "HU" => new HuVatValidator(countryCode),
            "IE" => new IeVatValidator(countryCode),
            "IT" => new ItVatValidator(countryCode),
            "LT" => new LtVatValidator(countryCode),
            "LU" => new LuVatValidator(countryCode),
            "LV" => new LvVatValidator(countryCode),
            "MT" => new MtVatValidator(countryCode),
            "NL" => new NlVatValidator(countryCode),
            "PL" => new PlVatValidator(countryCode),
            "PT" => new PtVatValidator(countryCode),
            "RO" => new RoVatValidator(countryCode),
            "SE" => new SeVatValidator(countryCode),
            "SI" => new SiVatValidator(countryCode),
            "SK" => new SkVatValidator(countryCode),
            "XI" => new XiVatValidator(countryCode),
            _ => null,
        };

        if (validator == null)
        {
            return null;
        }

        VatValidators.Add(countryCode, validator);

        return validator;
    }

    private readonly bool _disposeClient;
    private readonly HttpClient _httpClient;
    private readonly ViesService _viesService;

    /// <summary>
    ///
    /// </summary>
    public ViesManager() : this(HttpClientProvider.GetHttpClient(), disposeClient: true)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(ViesConstants.MediaTypeHeaderTextXml);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="disposeClient"></param>
    #pragma warning disable
    public ViesManager(HttpClient httpClient, bool disposeClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeClient = disposeClient;
        _viesService = new ViesService(httpClient, new XmlResponseParser());
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
        if (ExcludedCountries.TryGetValue(countryCode, out ExcludedCountryInfo excludedCountryInfo))
        {
            return VatValidationResult.Failed(countryCode, VatValidationErrorCode.InvalidEUVat, excludedCountryInfo.ToString());
        }

        IVatValidator validator = GetValidator(countryCode);
        return validator == null
            ? VatValidationResult.Failed(countryCode, VatValidationErrorCode.InvalidEUVat,"Not a valid ISO_3166-1 European member state.")
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
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [Obsolete("Use IsActiveAsync(string countryCode, string vatNumber, CancellationToken cancellationToken) instead")]
    public Task<ViesCheckVatResponse> IsActive(string countryCode, string vatNumber, CancellationToken cancellationToken = default)
    {
        return IsActiveAsync(countryCode, vatNumber, cancellationToken);
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
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public async Task<ViesCheckVatResponse> IsActiveAsync(string countryCode, string vatNumber, CancellationToken cancellationToken = default)
    {
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
    [Obsolete("Use IsActiveAsync(string countryCode, string vatNumber, CancellationToken cancellationToken) instead.")]
    public Task<ViesCheckVatResponse> IsActive(string vatNumber, CancellationToken cancellationToken = default)
    {
        return IsActiveAsync(vatNumber, cancellationToken);
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
            throw new ViesValidationException("VAT number cannot be null or empty.");
        }

        if (vat.Length < 3)
        {
            throw new ViesValidationException($"VAT number '{vat}' is too short.");
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
