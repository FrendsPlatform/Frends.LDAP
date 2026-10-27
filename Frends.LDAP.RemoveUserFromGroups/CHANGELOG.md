# Changelog

## [2.0.0] - 2026-10-27
### Changed
- **[Breaking Change]** Upgraded the Task to target .NET 8.
- **[Breaking Change]** Added `Options` and `CancellationToken` parameters to the Task method.
- **[Breaking Change]** Added `ThrowErrorOnFailure` and `ErrorMessageOnFailure` options. When `ThrowErrorOnFailure` is false, errors are returned in the result instead of being thrown.
- **[Breaking Change]** Removed the `HandleLDAPError` input property; use `ThrowErrorOnFailure` instead.
- **[Breaking Change]** `Result.Error` is now an object `{ Message, AdditionalInfo }` instead of a string.
- The Task now stops when the cancellation token is cancelled.

## [1.1.0] - 2026-02-03
### Changed
- Improved documentation for Host parameter usage

## [1.0.0] - 2022-10-13
### Added
- Initial implementation