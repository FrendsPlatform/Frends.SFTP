# Changelog

## [2.5.4] - 2023-02-14
### Added
- Re-enabled key exchange algorithms 'curve25519-sha256' and 'curve25519-sha256@libssh.org'.

## [2.5.3] - 2023-02-08
### Fixed
- Fixed issue with file not found when downloading from server when server has automatic delete action enabled.

## [2.5.2] - 2023-01-31
### Added
- Added cancellation token to the method listing the source files so Task won't freeze in that execution. 

## [2.5.1] - 2022-12-30
### Fixed
- Fixed issue with empty source dir throws an exception by setting empty source directory as '/'.
- Fixed issue with when no source files userResultMessage still said no. files transferred and not no files transferred.

## [2.5.0] - 2022-12-16
### Fixed
- [Breaking] Fixed issue where keepaliveinterval and operationtimeout is set as the connectiontimeout by creating them for the keepaliveinterval its own parameter and removed operationtimeout.

## [2.4.2] - 2022-12-01
### Updated
- Updated dependencies System.Text.Encoding.CodePages and Microsoft.Extensions.DependencyInjection to the newest version.
- Modified test to run against net471 instead of net6.

## [2.4.1] - 2022-11-24
### Fixed 
- Fixed issue with files not found when the name consist of special characters by adding a check for the files exact name.

## [2.4.0] - 2022-11-03
### Added
- [Breaking] Added parameters for the file extension of temporary source and destination files when rename options are enabled.
- Fixed task to enable transfers with file paths. 
- Fixed operations log to show correct state when source files are not found with filePaths.
- Fixed operations log to use temp work path when getting source files to temp directory.
- Added tests for the filePaths.
- Modified tests to not use physical files but to create them during test run.

## [2.3.0] - 2022-10-12
### Added
- Added boolean parameter for adding a new line when appending to an existing file.
- Changed the appending to use AppendAllText instead of AppendAllLines.

## [2.2.3] - 2022-10-12
### Fixed
- Fixed OperationTimeout and KeepAliveInterval attributes to use directly user input.

## [2.2.2] - 2022-10-03
### Added
- Added OperationTimeout and KeepAliveInterval attributes to SftpClient and set them to same value as the ConnectionTimeout parameter.

## [2.2.1] - 2022-09-30
### Fixed
- Added possibility to give different directory to the task when using SourceOperation.Rename.
- Updated documentation.

## [2.2.0] - 2022-09-26
### Fixed
- [Breaking] Added option to enable keyboard-interactive authentication method. This change will break the automatic updates of the task.
- Fixed issue with connecting to server which uses keyboard-interactive authentication method. Fixed by adding UseKeyboardInteractiveAuthentication parameter and handled the method inside tha task.

## [2.1.2] - 2022-09-23
### Fixed
- Fixed issue with the error message by changing how the error message is build. Error message had 'SFTP://' in both endpoints.

## [2.1.1] - 2022-09-16
### Fixed
- Fixed error handler by adding connection check to FileTransporter and SingleFileTransfer classes. If the client is not connected the task tries to connect again before handling errors.

## [2.1.0] - 2022-09-09
### Fixed
- [Beaking] Removed UTF-16 and Unicode FileEncoding because their implementation didn't work. These were used as a parameter so autoupdate won't work.
- Fixed how the Encoding on windows-1252 is handled. Added NuGet System.Text.Encoding.CodePages which can handle that encoding.
- Fixed error handling by adding catch to FileTransporter to catch SftpPathNotFoundException and general Exception.
- Added tests to test the file name and content encoding.
- Updated the document to state that Ssh.Net only supports private keys in OpenSSH and ssh.com formats.
- Added documentation on the private key formatingm from putty.ppk to OpenSSH.
- Fixed HostKeyAlgorithm by removing the forcing of the ssh-rsa.
- Added HostKeyAlgorithm parameter which enables users to change the host key algorithm used in the task. Before task defaults to ED25519.
- Added enum HostKeyAlgorithms with the supported algorithms.
- Modified tests to create testfiles instead of using files from project directory.

## [2.0.3] - 2022-08-19
### Fixed
- Fixed issue with server fingerprint given by user in SHA256 hex format was not accepted: Added conversion to the fingerprint given by user.
- Fixed issue when using invalid server fingerprint in MD5 string format throws wrong error message: Added more specific error messages.
- Changed how MD5 string is handled. MD5 can now be given without ':' or '-' characters.
- Fixed issue that Sha256 was only accepted in Base64 format: Added support for Sha256 in hex format.
- Changed the used HostKeyAlgorithm by forcing to use ssh-rsa default was ed25519.
- Added more tests for using server fingerprints.

## [2.0.2] - 2022-08-10
### Fixed
- Fixed issue where when using Rename options and appending / overwrite the task would throw an exception because the work directory given by user was used with renaming destination file.
- Added error message if a file with the same name as source file exists in DirectoryToMoveAfterTransfer.
- Fixed issue where when using SourceOperation.Move the source file cannot be restored when exception occurs.  
- Fixed issue that if SourceOperation is ignore and no source files were found no operations log is created.
- Removed FileEncoding UTF-16 which was not implemented and threw exception if selected.

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
