# Changelog

# [2.0.0] - 2022-08-08
### Fixed
- [Breaking] Changed the implementation to work similar to Cobalt by moving the source file to local Temp folder before transferring to destination.
- Fixed issue where PreserveModified caused exceptions because the method used wrong file path.
- Fixed bug where source files were deleted if RenameSourceFileBeforeTransfer was enabled and SourceOperation.Move had directory that didn't exist.
- Added logger usage in places where it was needed to make the operations log and error info more informative.
- Modified the logger usage that the logger.NotifyInformation is done after the action so it's easier to see where errors has occurred.

## [1.0.2] - 2022-06-08
### Fixed
- Fixed Source Operations removed source file when RenameSourceFileBeforeTransfer was enabled and transfer failed.
- Fixed Destination file being removed when RenameDestinationFileDuringTransfer was enabled and transfer failed.
- Removed permission changes to destination file.
- Fixed documentation.

## [1.0.1] - 2022-06-07
### Fixed
- Cleaned up test files, added more test for macros, fixed source file rename and move after transfer

## [1.0.0] - 2022-05-24
### Added
- Initial implementation
