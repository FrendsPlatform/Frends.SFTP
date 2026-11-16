# Changelog

## [3.2.0] - 2026-11-09

### Added

- Added `Success` and `Error` properties to the result. The task can now return `Success = false` with error details in `Error` instead of throwing.
- Added `ThrowErrorOnFailure` option (default `true`) to choose whether failures throw an exception or are returned in the result.
- Added `ErrorMessageOnFailure` option to specify a custom error message used when a failure occurs.

### Changed

- Upgraded the task to .NET 8.
- Updated SSH.NET to 2026.0.0 to address known security vulnerabilities.

## [3.1.0] - 2026-10-21

### Changed

- An empty or unknown encoding name now gives a clear error message ("Unknown encoding: '...'") instead of the default .NET message.

## [3.0.0] - 2026-01-27

### Changed

#### Breaking changes!

- Updated dependency SSH.NET to the newest version 2025.1.0.
- Drop DSS support

## [2.3.0] - 2026-01-13

### Fixed

- Operation timeout is now using the Connection.ConnectionTimeout parameter.

## [2.2.0] - 2025-01-13

### Fixed

- Fixed issue with ConnectionInfoBuilder having static properties for connection and input parameters which led to Task
  not being thread safe.

## [2.1.0] - 2024-08-19

### Updated

- Updated Renci.SshNet library to version 2024.1.0.

## [2.0.0] - 2024-01-02

### Updated

- [Breaking] Updated dependency SSH.NET to the newest version 2023.0.0.

### Changed

- Changed connection info builder to create the connection info as it's done in DownloadFiles.

## [1.0.0] - 2023-09-01

### Changed

- Initial implementation
