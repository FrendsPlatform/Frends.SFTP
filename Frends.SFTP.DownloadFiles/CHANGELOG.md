# Changelog

## [2.0.1] - 2022-07-18
### Fixed
- Fixed issue with task throwing exception when no source files found and source action info or ignore selected.

## [2.0.0] - 2022-07-15
### Fixed
- [Breaking] Changed the implementation to work similar to Cobalt by moving the source file to local Temp folder before transfering to destination.
- Fixed issue where PreserveModified caused exceptions because the method used wrong file path.
- Fixed bug where source files were deleted if RenameSourceFileBeforeTransfer was enabled and SourceOperation.Move had directory that didn't exist.'
- Added logger usage in places where it was needed to make the operations log and error info more informative.
- Modified the logger usage that the logger.NotifyInformation is done after the action so it's easier to see where errors has occurred.

## [1.0.4] - 2022-06-30
### Fixed
- Fixed issue where '.' and '..' directories were also fetched when using '*' character as source file mask.
- Added check for GetSourceFiles so that only files are fetched and not directories.
- Updated Microsoft.Extension.DependencyInjection library.

## [1.0.3] - 2022-06-29
### Fixed
- Fixed issue with forward slash being added to the source directory.

## [1.0.2] - 2022-06-15
### Fixed
- Fixed issue with download failes when RenameDestinationFileDuringTransfer was enabled and destination file existed.
- Added tests to test the issue.

## [1.0.1] - 2022-06-13
### Fixed
- Fixed Source Operations removed source file when RenameSourceFileBeforeTransfer was enabled and transfer failed.
- Fixed Destination file being removed when RenameDestinationFileDuringTransfer was enabled and transfer failed.
- Removed permission changes to destination file.
- Fixed documentation.

## [1.0.0] - 2022-06-03
### Added
- Initial implementation
