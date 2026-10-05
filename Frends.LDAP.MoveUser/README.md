# Frends.LDAP.MoveUser

Moves a user to another distinguished name on the same LDAP server and domain.

[![MoveUser_build](https://github.com/FrendsPlatform/Frends.LDAP/actions/workflows/MoveUser_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.LDAP/actions/workflows/MoveUser_test_on_main.yml)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.LDAP/Frends.LDAP.MoveUser|main)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## Installing

You can install the Task via Frends UI Task View.

The task targets .NET 8.0 and is independently packaged as `Frends.LDAP.MoveUser` version `1.0.0`.

## Usage

Provide the user's **full source DN** and **full destination DN**, not just the destination container:

| Input | Example |
| --- | --- |
| `SourceDistinguishedName` | `CN=Jane Doe,OU=Sales,DC=example,DC=com` |
| `DestinationDistinguishedName` | `CN=Jane Doe,OU=Support,DC=example,DC=com` |

Both DNs must end in the same `DC` components, compared case-insensitively. Moving to another domain, including a child domain, is not supported. Use escaped LDAP DN values where necessary, for example `CN=Doe\, Jane,OU=Sales,DC=example,DC=com`. Escaped separators and multivalued RDNs are preserved.

The task sends a single LDAP **Modify DN** request to move the existing user. It does not copy, delete, or recreate the account. Keep the first RDN (`CN=Jane Doe` above) unchanged to move without renaming; a different first RDN also renames the entry and removes the old RDN attribute value.

The destination container must already exist, and the destination DN must not belong to another entry. The source must be a person/user entry; groups, organizational units, and computer accounts are rejected. Directory-side permissions, schema rules, and referential-integrity behavior still apply.

### Connection

| Parameter | Description | Default |
| --- | --- | --- |
| `Host` | One server hostname or IP address, without `ldap://`, a port, or a host list. For Active Directory, use a writable domain controller in the user's domain. | Required |
| `Port` | TCP port; `0` chooses 636 for LDAPS or 389 otherwise. | `0` |
| `SecureSocketLayer` | Use LDAPS with a trusted server certificate. | `false` |
| `TLS` | StartTLS before binding, with a trusted server certificate. Cannot be combined with `SecureSocketLayer`. | `false` |
| `User` | Bind DN or a server-supported username with permission to read the source user and move it to the destination container. | Required |
| `Password` | Bind password; supply a secret environment variable such as `#env.LdapPassword`. | Required |

**Enable LDAPS or StartTLS in production.** With both disabled, credentials and LDAP traffic are unencrypted. Certificate validation is not bypassed. LDAP referrals are disabled, so the task does not follow a move or read request to another server.

### Options

| Parameter | Description | Default |
| --- | --- | --- |
| `TimeoutSeconds` | Connection and per-operation timeout, from 1 to 2147483 seconds. | `30` |
| `ThrowErrorOnFailure` | Throw on validation, authentication, or LDAP errors. If disabled, return a failed result instead. | `true` |
| `ErrorMessageOnFailure` | Custom thrown error message, or a prefix for the returned error message. The original exception is retained in either case. | Empty |

The task uses the same synchronous `Novell.Directory.Ldap.NETStandard` 3.6.0 client as the other LDAP tasks. Cancellation is checked before each LDAP operation and is always propagated, regardless of `ThrowErrorOnFailure`. It does not interrupt an operation already in progress. A timeout or lost response during Modify DN can leave the outcome uncertain; check both DNs before retrying.

### Result

```json
{
  "Success": true,
  "SourceDistinguishedName": "CN=Jane Doe,OU=Sales,DC=example,DC=com",
  "DestinationDistinguishedName": "CN=Jane Doe,OU=Support,DC=example,DC=com",
  "Error": null
}
```

When `ThrowErrorOnFailure` is `false`, failures return `Success = false`, null DN properties, and `Error { Message, AdditionalInfo }`. `AdditionalInfo` contains the original exception, including the result code for LDAP errors.

## Development

Run commands from the `Frends.LDAP.MoveUser` solution directory with the .NET 8 SDK or newer. The full test suite also requires a running Docker daemon configured for Linux containers:

```powershell
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet pack --configuration Release
```

Integration tests run automatically with `dotnet test`. [Testcontainers for .NET](https://dotnet.testcontainers.org/) starts a disposable `osixia/openldap:1.5.0` container, waits for LDAP readiness, and stops and removes the container in fixture teardown, including when tests fail. A random host port avoids collisions with other LDAP servers or concurrent test runs. The first run downloads the required container images.

No manual container commands, `.env` files, LDAP credentials, or environment variables are needed locally or in CI. The fixture generates temporary admin credentials and never connects to an external directory. Docker startup failures fail the integration tests rather than silently skipping them.

The tests create uniquely named OUs beneath `dc=example,dc=com`, move users with escaped DNs, verify stable entry identifiers and other attributes, exercise collisions and missing entries, and remove their test entries afterwards.

To run only unit tests without Docker:

```powershell
dotnet test --filter "TestCategory!=Integration"
```

## Dependencies and licenses

The task is MIT-licensed. Its runtime LDAP dependency is [Novell.Directory.Ldap.NETStandard 3.6.0](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard/tree/3.6.0), copyright (c) 2003 Novell Inc., used under the [MIT license](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard/blob/3.6.0/LICENSE). Its Microsoft runtime dependency is also MIT-licensed.

Build and test dependencies are FrendsTaskAnalyzers, StyleCop.Analyzers, NUnit, NUnit3TestAdapter, Microsoft.NET.Test.Sdk, coverlet.collector, and Testcontainers (MIT), plus NSubstitute (BSD-3-Clause) and its Castle.Core dependency (Apache-2.0). Build and test tools are not shipped as task dependencies.
