# Changelog

## [3.0.0] - 2026-01-27

### Changed

#### Breaking changes!

- Updated dependency SSH.NET to the newest version 2025.1.0.
- Drop DSS support

## [2.7.0] - 2026-01-13

### Fixed

- Operation timeout is now using the Connection.ConnectionTimeout parameter.

## [2.6.0] - 2025-12-16

### Added

- Added MaxExecutionTimeout parameter (default: 0 = no limit) to set a maximum time limit for the entire SFTP list
  operation.

## [2.5.0] - 2025-04-09

### Changed

- Clarified that the encoding parameter is used for the file name encoding.

## [2.4.0] - 2025-01-03

### Fixed

- Fixed issue with ConnectionInfoBuilder having static properties for connection and input paramaters which lead to Task
  not being thread safe.

## [2.3.0] - 2024-08-19

### Updated

- Updated Renci.SshNet library to version 2024.1.0.

## [2.2.0] - 2023-12-21

### Updated

- [Breaking] Updated dependency SSH.NET to the newest version 2023.0.0.

### Changed

- Changed connection info builder to create the connection info as it's done in DownloadFiles.
- [Breaking] Changed PrivateKeyFilePassphrase parameter to PrivateKeyPassphrase and enabled it when PrivateKeyString was
  used.

## [2.0.1] - 2022-12-01

### Updated

- Updated dependency Microsoft.Extensions.DependencyInjection to the newest version.

## [2.0.0] - 2022-11-09

### Added

- Added keyboard authentication method
- [Breaking] Added parameter for keyboard authentication
- Modified result Task to return object instead of list
- Added result attribute FileCount
- Added more tests e.g. Serverfingerprint tests
- Added System.Text.CodePages NuGet to the project
- Added ConnectionInfoBuilder
- Added HostKeyAlgorithm enum

## [1.0.2] - 2022-06-28

### Changed

- Reconstructed the Task to be similar to other SFTP tasks
- Implemented more authentication options
- Added timeout option
- Added File name encoding options
- Updated tests

## [1.0.1] - 2022-06-20

### Changed

- SSH.NET version upgrade from 2020.0.1 to 2020.0.2

## [1.0.0] - 2022-04-01

### Added

- Initial implementation
