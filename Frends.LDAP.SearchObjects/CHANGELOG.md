# Changelog

## [4.1.0] - 2025-06-17
### Added
- Improved LDAP error handling by validating bind status and checking search response codes.

## [4.0.0] - 2025-04-04
### Added
- [Breaking] - Parameter for PageSize to control how many entries are fetched per page during an LDAP search.
- Default value for the new parameter:
  - PageSize: 500
- If you want the Task to work exactly as before (non-paged search), set PageSize = 0.
- Default paging behavior (PageSize = 500) may improve performance on large result sets, 
  but changes how results are retrieved from the server.

## [3.1.0] - 2025-04-01
### Added
- Added example values for new parameters.

## [3.0.0] - 2025-03-17
### Added
[Breaking]
- Updated to version 3.0.0 with enhanced LDAP search capabilities.
- Added support for selecting custom attributes, allowing users to receive attribute values as either text, byte array or Guid.
- Introduced new encoding options—including BOM support and various encoding choices—for improved handling of search results.
- Enabled an option to return only specified attributes, enhancing control over the output.
- Added functionality to handle photo attributes in LDAP entries.
- Enhanced flexibility in specifying content encoding for search results.
- New methods and properties for improved search input handling and attribute retrieval.
- Changed the Result object to be a dictionary of string, object to allow for more flexibility in the output.

### Upgrade Instructions
- Task's default values for new and changed parameters are configured to match the previous behavior. If you want to use the new functionality, you will need to update the parameters accordingly.
	- 'SearchOnlySpecifiedAttributes' parameter defaults to true which was the previous behavior. If you just want to specify certain attributes' types, set it to false. If not enabled the non specified attributes will be returned as string typed.
	- 'ContentEncoding' parameter defaults to the Agent's default encoding. If you want to use a different encoding, set it to the desired encoding.
- Task's output has been updated to reflect the new Result object structure. This will have impact on how the Result object can be manipulated in other elements.

## [2.1.0] - 2025-01-02
### Fixed
- Fixed issue with AttributeSet not having all values that were returned from LDAP search.

## [2.0.0] - 2024-11-06
### Added
- [Breaking] Parameter for AnonymousBind to enable to connect without credentials.
- [Breaking] Parameter for LDAPProtocolVersion to choose what LDAP version should be used.
- Default values for new parameters are:
  - AnonymousBind: false
  - LDAPProtocolVersion: 3
- Use the default parameters if you want the Task to work as before.

- Check for Filter parameter that it's not empty and set null to it if it is so that the library sets objectClass=* as the filter.

## [1.0.0] - 2022-10-03
### Added
- Initial implementation