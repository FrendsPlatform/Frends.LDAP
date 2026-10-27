# Changelog

## [2.0.0] - 2026-10-27
### Changed
- **[Breaking Change]** Task is now a static class and targets .NET 8.
- **[Breaking Change]** Added required `Options` parameter and `CancellationToken` to the Task method.
- **[Breaking Change]** LDAP errors are now thrown by default (`ThrowErrorOnFailure`); disable it to get a failed result instead.
- Added `ErrorMessageOnFailure` option to override the error message.

## [1.1.0] - 2026-02-03
### Changed
- Improved documentation for Host parameter usage

## [1.0.0] - 2022-09-26
### Added
- Initial implementation