# Changelog

## [2.0.0] - 2026-10-27
### Changed
- Task is now a static class and targets .NET 8.
- Added required `Options` parameter and `CancellationToken` to the Task method.
- **[Breaking Change]** LDAP errors are now thrown by default (`ThrowErrorOnFailure`); disable it to get a failed result instead.
- **[Breaking Change]** `Result.Error` is now an object with `Message` and `AdditionalInfo` instead of a string. Missing connection parameters are also handled by the same error handling.
- Added `ErrorMessageOnFailure` option to override the error message.

## [1.1.0] - 2026-02-03
### Changed
- Improved documentation for Host parameter usage

## [1.0.0] - 2022-09-26
### Added
- Initial implementation
