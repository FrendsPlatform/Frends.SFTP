# Changelog

## [3.0.0] - 2026-01-27

### Changed

#### Breaking changes!

- Updated dependency SSH.NET to the newest version 2025.1.0.
- Drop DSS support

## [2.3.0] - 2026-01-13

### Fixed

- Operation timeout is now using the Connection.ConnectionTimeout parameter.

## [2.2.0] - 2025-01-10

### Fixed

- Fixed issue with ConnectionInfoBuilder having static properties for connection and input paramaters which lead to Task
  not being thread safe.

## [2.1.0] - 2024-08-19

### Updated

- Updated Renci.SshNet library to version 2024.1.0.

## [2.0.0] - 2024-01-03

### Updated

- [Breaking] Updated dependency SSH.NET to the newest version 2023.0.0.

### Changed

- Changed connection info builder to create the connection info as it's done in DownloadFiles.
- [Breaking] Changed PrivateKeyFilePassphrase parameter to PrivateKeyPassphrase and enabled it when PrivateKeyString was
  used.

## [1.0.0] - 2023-05-23

### Added

- Initial implementation
