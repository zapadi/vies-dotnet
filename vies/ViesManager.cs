using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Zapadi.Vies
{
	public class ViesManager : IDisposable
	{
		private const string VIES_URI = "http://ec.europa.eu/taxation_customs/vies/services/checkVatService";
		private const string VIES_TEST_URI = "http://ec.europa.eu/taxation_customs/vies/services/checkVatTestService";
		private const string SOAP_VALIDATE_VAT_MESSAGE_FORMAT = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:v1=\"http://schemas.conversesolutions.com/xsd/dmticta/v1\"><soapenv:Header/><soapenv:Body><checkVat xmlns=\"urn:ec.europa.eu:taxud:vies:services:checkVat:types\" ><countryCode>{0}</countryCode><vatNumber>{1}</vatNumber></checkVat></soapenv:Body></soapenv:Envelope>";
		private const string TEXT_XML_MEDIA_TYPE = "text/xml";

		private static readonly Regex countryCodeRegex = new Regex("[a-zA-Z]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		private static readonly Dictionary<string, Regex> vatPatterns = new Dictionary<string, Regex>
		{
			 {"AT", new Regex("U[A-Z\\d]{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"BE", new Regex("(0\\d{9}|\\d{10})", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"BG", new Regex("\\d{9,10}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"CY", new Regex("\\d{8}[A-Z]", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"CZ", new Regex("\\d{8,10}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"DE", new Regex("\\d{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"DK", new Regex("(\\d{2} ?){3}\\d{2}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"EE", new Regex("\\d{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"GR", new Regex("\\d{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)}, //GR = EL
			 {"EL", new Regex("\\d{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"ES", new Regex("[A-Z]\\d{7}[A-Z]|\\d{8}[A-Z]|[A-Z]\\d{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant) },
			 {"FI", new Regex("\\d{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"FR", new Regex("([A-Z]{2}|\\d{2})\\d{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"GB", new Regex("\\d{9}|\\d{12}|(GD|HA)\\d{3}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"HR", new Regex("\\d{11}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"HU", new Regex("\\d{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"IE", new Regex("[A-Z\\d]{8}|[A-Z\\d]{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"IT", new Regex("\\d{11}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"LT", new Regex("(\\d{9}|\\d{12})", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"LU", new Regex("\\d{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"LV", new Regex("\\d{11}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"MT", new Regex("\\d{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"NL", new Regex("\\d{9}B\\d{2}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"PL", new Regex("\\d{10}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"PT", new Regex("\\d{9}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"RO", new Regex("\\d{2,10}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"SE", new Regex("\\d{12}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"SI", new Regex("\\d{8}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
			 {"SK", new Regex("\\d{10}", RegexOptions.Compiled | RegexOptions.CultureInvariant)},
		};

		private readonly HttpClient httpClient;

		public ViesManager(int timeout = 10) : this(new HttpClient()
		{
			Timeout = TimeSpan.FromSeconds(timeout)
		})
		{
		}

		public ViesManager(HttpClient httpClient)
		{
			this.httpClient = httpClient;
		}

		public async Task<CheckVatResponse> CheckVat(string vatNumber, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(vatNumber))
				throw new ValidationException("VAT number cannot be null");

			vatNumber = vatNumber.SanitizeVatNumber();

			if (vatNumber.Length < 3)
				throw new ValidationException($"Vat number '{vatNumber}' is too short.");

			var countryCode = vatNumber.Substring(0, 2);
			var number = vatNumber.Substring(2);

			EnsureCountryCodeIsValid(countryCode);
			EnsureVatNumberIsValid(countryCode, number);

			return await CheckVat(countryCode, number, cancellationToken).ConfigureAwait(false);
		}

		private async Task<CheckVatResponse> CheckVat(string countryCode, string vatNumber, CancellationToken cancellationToken)
		{
			var stringContent = new StringContent(string.Format(SOAP_VALIDATE_VAT_MESSAGE_FORMAT, countryCode, vatNumber), Encoding.UTF8, TEXT_XML_MEDIA_TYPE);

			using (var postResult = await httpClient.PostAsync(VIES_TEST_URI, stringContent, cancellationToken).ConfigureAwait(false))
			{
				if (postResult.IsSuccessStatusCode)
				{
					string content = await postResult.Content.ReadAsStringAsync().ConfigureAwait(false);
					return ParseContentResponse(content);
				}
				else
				{
					throw new Exception($"VIES service error: {postResult.StatusCode} - {postResult.ReasonPhrase}");
				}
			}
		}

		public void Dispose()
		{
			httpClient?.Dispose();
		}

		private CheckVatResponse ParseContentResponse(string content)
		{
			if (content.Contains("INVALID_INPUT") || content.Contains("SERVICE_UNAVAILABLE")) return default(CheckVatResponse);

			var cc = content.GetValue( "<countryCode>(.*?)</countryCode>");
			var vn = content.GetValue( "<vatNumber>(.*?)</vatNumber>");
			var n = content.GetValue("<name>(.*?)</name>");
			var adr = content.GetValue("<address>(?s)(.*?)(?-s)</address>", s => s.Replace("\\\n", "\n"));
			var isValid = content.GetValue( "<valid>(true|false)</valid>");
			var dt = content.GetValue("<requestDate>(.*?)</requestDate>");
			//^(?<month>(0?[1-9]|1[0-2eGetValue(day>d\d)(/(?<year>\d\d\d\d)?)?)[ \t](?<hour>\d\d):(?<min>\d\d)(:(?<sec>\d\d))?$
			return new CheckVatResponse(cc, vn, dt == null ? default(DateTimeOffset) : DateTimeOffset.Parse(dt), n, adr, isValid == null ? false : bool.Parse(isValid));
		}

		private void EnsureCountryCodeIsValid(string countryCode)
		{
			if (!countryCodeRegex.IsMatch(countryCode))
			{
				throw new ValidationException($"{countryCode} is not a valid ISO_3166-1 country code.");
			}
			if (!vatPatterns.ContainsKey(countryCode))
			{
				throw new ValidationException($"{countryCode} is not a european member state.");
			}
		}

		private void EnsureVatNumberIsValid(string countryCode, string vatNumber)
		{
			if (!vatPatterns[countryCode].IsMatch(vatNumber))
				throw new ValidationException($"'{countryCode}' does not match the countries VAT ID specifications.'");
		}
	}
}