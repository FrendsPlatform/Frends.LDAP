# Changelog

## [2.0.0] - 2026-10-27
### Changed
- Task now targets .NET 8.
- Added `Options` and `CancellationToken` parameters to the task.
- **[Breaking Change]** `Error` in the result is now an object with `Message` and `AdditionalInfo` instead of a string.
- LDAP and other errors are now thrown by default; set `Options.ThrowErrorOnFailure` to false to get them in the result instead.
### Added
- `Options.ThrowErrorOnFailure` and `Options.ErrorMessageOnFailure` for error handling control.

## [1.2.0] - 2026-02-03
### Changed
- Improved documentation for Host parameter usage

## [1.1.0] - 2022-12-14
### Added
- New Input parameters CreateDN and DistinguishedName to give an option to insert DN without combining it from Input.CommonName and Input.Path.

## [1.0.0] - 2022-09-30
### Added
- Initial implementation
