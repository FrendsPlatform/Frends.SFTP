# Changelog

## [2.0.0] - 2026-01-27

### Changed

#### Breaking changes!

- Updated dependency SSH.NET to the newest version 2025.1.0.
- Drop DSS support

## [1.5.0] - 2026-01-16

### Fixed

- Task can now create nested target directories.

## [1.4.0] - 2026-01-13

### Fixed

- Operation timeout is now using the Connection.ConnectionTimeout parameter.

## [1.3.0] - 2025-01-10

### Fixed

- Fixed issue with ConnectionInfoBuilder having static properties for connection and input parameters which lead to Task
  not being thread safe.

## [1.2.0] - 2024-08-19

### Updated

- Updated Renci.SshNet library to version 2024.1.0.

### Added

- Added more tests.

## [1.1.0] - 2023-12-22

### Updated

- [Breaking] Updated dependency SSH.NET to the newest version 2023.0.0.

### Changed

- Changed connection info builder to create the connection info as it's done in DownloadFiles.
- [Breaking] Changed PrivateKeyFilePassphrase parameter to PrivateKeyPassphrase and enabled it when PrivateKeyString was
  used.

### Added

- Added FileEncoding for file names.

## [1.0.0] - 2023-22-05

### Added

- Initial implementation
