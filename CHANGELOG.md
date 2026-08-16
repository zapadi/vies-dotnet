
## [4.0.0] - 2026-08-16

## New ✨

* Support for the EC VIES REST (JSON) API, now the default endpoint. `ViesApiEndpoint` selects between REST and SOAP.
* `ViesCheckVatResponse` gains `RequestIdentifier` and `Trader*` properties (populated on the REST endpoint only).
* Country-code overloads accept GR as an alias for EL, consistent with sanitized input.

## Breaking 💥

* Default endpoint changed from SOAP to REST.
* `ViesManager.IsValid` for excluded countries (GB) now returns a failed `VatValidationResult` instead of throwing `ViesUnsupportedRegionException`.
* Parser interfaces (`IResponseParser`, `IResponseParserAsync`) and the helper extension classes (`ViesExtensions`, `ViesConstants`) are now internal.
* Removed the obsolete `ReplaceString` extension and the unused `RESPONSE_DATE_FORMAT` constant.

## Improvements 🙌

* Unified the SOAP and REST transport pipeline behind a single template-method base.
* Client-side timeouts and network errors now surface as `ViesServiceException` with `timeout` / `network-error` codes; genuine caller cancellation still propagates as `OperationCanceledException`.
* SOAP faults returned on non-200 responses now surface their real fault code instead of a generic service error.
* VIES fault strings are mapped to stable library error codes on both the SOAP and REST endpoints.
* `IsActiveAsync` now throws `ViesUnsupportedRegionException` for excluded countries (GB), consistent with `IsValid`.
* VIES HTTP errors now propagate the status code and response body with specific error codes (rate-limit-exceeded, timeout).
* Validator lookup is thread-safe and validators are stateless.
* Package metadata fixes.

## [3.1.0] - 2025-09-03

## Bug Fixes 🐛

* Fixed IEValidator wrong checksum for 2013+ format
## Improvements 🙌

* Replace exceptions magic strings error codes with constants

### [3.0.1] - 2025-08-23

## Bug Fixes 🐛

* Fixed `CountryCode` in ___VatValidatorAbstract___ to be an instance property, ensuring each validator maintains its own independent country code

### [3.0.0] - 2025-08-06

## New ✨

* Introduced `ExceptionDispatcher` & `VatValidationDispatcher` to centralize error handling across all validators, returning `VatValidationResult` objects with standardized error codes and messages.

## Improvements 🙌

* Refactored validation handling in all validator classes (e.g., `AtVatValidator`, `BEVatValidator`, `DEVatValidator`, etc.) to use `VatValidationDispatcher` methods instead of direct `VatValidationResult.Failed` calls.

* Updated error codes to use kebab-case format from ViesErrorCodes (e.g., `invalid-vat-format`) with user-facing messages.

## Breaking Changes ⚠️

* Changed error codes in `VatValidationResult.ErrorCode` from VatValidationErrorCode values (e.g., InvalidLength, InvalidFormat) to kebab-case ViesErrorCodes (e.g., vat-number-too-long, invalid-vat-format). ___Consumers relying on specific error codes must update their logic.___


* Updated `VatValidationResult.Error` to use user-facing messages from `ViesErrorCodes.UserMessage` (e.g., “The VAT number is too long.”). Consumers parsing specific error messages may need adjustments.
