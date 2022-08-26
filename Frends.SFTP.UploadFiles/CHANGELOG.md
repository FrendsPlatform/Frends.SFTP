# Changelog

#[2.0.1] - 2022-08-26
### Fixed
- Fixed issue with server fingerprint given by user in SHA256 hex format was not accepted: Added conversion to the fingerprint given by user.
- Fixed issue when using invalid server fingerprint in MD5 string format throws wrong error message: Added more specific error messages.
- Changed how MD5 string is handled. MD5 can now be given without ':' or '-' characters.
- Fixed issue that Sha256 was only accepted in Base64 format: Added support for Sha256 in hex format.
- Changed the used HostKeyAlgorithm by forcing to use ssh-rsa default was ed25519.
- Added more tests for using server fingerprints.
- Fixed issue where when using SourceOperation.Move the source file cannot be restored when exception occurs. 
- Removed FileEncoding UTF-16 which was not implemented and threw exception if selected.

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
