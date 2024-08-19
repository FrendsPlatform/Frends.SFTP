# Changelog

## [2.3.0] - 2024-08-19
### Updated
- Updated Renci.SshNet library to version 2024.1.0.

## [2.2.0] - 2024-01-03
### Updated
- [Breaking] Updated dependency SSH.NET to the newest version 2023.0.0.

### Changed
- Changed connection info builder to create the connection info as it's done in DownloadFiles.
- [Breaking] Changed PrivateKeyFilePassphrase parameter to PrivateKeyPassphrase and enabled it when PrivateKeyString was used.

## [2.0.1] - 2022-12-01
### Updated
- Updated dependency Microsoft.Extensions.DependencyInjection to the newest version.

## [2.0.0] - 2022-11-10
### Changed
- [Breaking] Added parameters for keyboard-interactive authentication and add new line when appending.
- Fixed overwrite deleting the original file before writing the new file.
- Added keyboard-interactive authentication method.
- Added more tests e.g. Serverfingerprint tests
- Added System.Text.CodePages NuGet to the project
- Added ConnectionInfoBuilder
- Added HostKeyAlgorithm enum

## [1.0.1] - 2022-06-14
### Changed
- Changed the main method to write from stream to a file.
- Updated tests
- Updated Renci.SshNet library

## [1.0.0] - 2022-03-10
### Added
- Initial implementation
