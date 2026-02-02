# Changelog

## [2.0.0] - 2026-01-29
### Changed
- [Breaking] Input: Changed GroupDistinguishedName (string) to GroupDistinguishedNames (string[]) to support adding users to multiple groups in a single operation.
- Result: Added Details property for operation details. Error now only contains error messages for failures.

## [1.2.0] - 2025-11-20
### Fixed
- Fixed LDAP exception when adding duplicate users with skip option enabled.

## [1.1.0] - 2025-08-13
### Fixed
- Fixed KeyNotFoundException when checking empty groups for existing users.

## [1.0.1] - 2024-03-01
### Fixed
- Fixed issue with UserExistsAction parameter when AD returned another error message by adding method to actually check if the user is in the group.

## [1.0.0] - 2022-10-07
### Added
- Initial implementation