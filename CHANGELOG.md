### [3.0.0] - 2025-08-06

## ➕ Added

Introduced `ExceptionDispatcher` & `VatValidationDispatcher` to centralize error handling across all validators, returning `VatValidationResult` objects with standardized error codes and messages.

## 🔄 Changed

Refactored validation handling in all validator classes (e.g., `AtVatValidator`, `BEVatValidator`, `DEVatValidator`, etc.) to use `VatValidationDispatcher` methods instead of direct `VatValidationResult.Failed` calls.

Updated error codes to use kebab-case format from ViesErrorCodes (e.g., `invalid-vat-format`) with user-facing messages.

## ⚠️ Breaking Changes

Changed error codes in `VatValidationResult.ErrorCode` from VatValidationErrorCode values (e.g., InvalidLength, InvalidFormat) to kebab-case ViesErrorCodes (e.g., vat-number-too-long, invalid-vat-format). ___Consumers relying on specific error codes must update their logic.___

Updated `VatValidationResult.Error` to use user-facing messages from `ViesErrorCodes.UserMessage` (e.g., “The VAT number is too long.”). Consumers parsing specific error messages may need adjustments.
