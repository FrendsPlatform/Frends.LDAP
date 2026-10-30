# Changelog

## [2.0.0] - 2026-10-27
### Changed
- Upgraded the Task to target .NET 8.
- The Task now stops when the cancellation token is canceled.
- **[Breaking Change]** Removed the `HandleLDAPError` input property; use `ThrowErrorOnFailure` instead.
- **[Breaking Change]** `Result.Error` is now an object `{ Message, AdditionalInfo }` instead of a string.
- **[Breaking Change]** Task rethrows exception when occured instead of creating a new generic Exception.

### Added
- Added `Options` and `CancellationToken` parameters to the Task method.
- Added `ThrowErrorOnFailure` and `ErrorMessageOnFailure` options. When `ThrowErrorOnFailure` is false, errors are returned in the result instead of being thrown.

## [1.1.0] - 2026-02-03
### Changed
- Improved documentation for Host parameter usage

## [1.0.0] - 2022-10-13
### Added
- Initial implementation
