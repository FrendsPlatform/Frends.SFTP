# Changelog

## [2.3.0] - 2025-01-10
### Fixed
- Fixed issue with ConnectionInfoBuilder having static properties for connection and input paramaters which lead to Task not being thread safe.

## [2.2.0] - 2024-08-19
### Updated
- Updated Renci.SshNet library to version 2024.1.0.

## [2.1.0] - 2023-12-22
### Updated
- [Breaking] Updated dependency SSH.NET to the newest version 2023.0.0.

### Changed
- Changed connection info builder to create the connection info as it's done in DownloadFiles.
- [Breaking] Changed PrivateKeyFilePassphrase parameter to PrivateKeyPassphrase and enabled it when PrivateKeyString was used.

## [2.0.1] - 2022-12-01
### Updated
- Updated dependency Microsoft.Extensions.DependencyInjection to the newest version.

## [2.0.0] - 2022-11-09
### Added
- Added keyboard authentication method
- [Breaking] Added parameter for keyboard authentication
- Added more tests e.g. Serverfingerprint tests
- Added System.Text.CodePages NuGet to the project
- Added ConnectionInfoBuilder
- Added HostKeyAlgorithm enum

## [1.0.2] - 2022-06-23
### Changed
- Changed the main method to read from a file.
- Updated tests

## [1.0.1] - 2022-06-20
### Changed
- SSH.NET version upgrade from 2020.0.1 to 2020.0.2

## [1.0.0] - 2022-03-25
### Added
- Initial implementation
