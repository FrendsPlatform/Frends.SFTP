# Changelog

## [2.7.0] - 2023-06-08
### Fixed
- Modified private key passphrase to be visible when all private key authentication options were enabled.
- [Breaking] Added new parameter for keyboard-interactive authentication where users can add prompts and responses.
- Modified operations log to list current system and sftp server information.
- Fixed operations log to show case exceptions more precisely.

## [2.6.1] - 2023-05-17
### Fixed
- Fixed issue with TransferredFileNames was incorrect when FilePaths parameter was used. 

## [2.6.0] - 2023-05-16
### Added
- Added new result attribute TransferredDestinationFilePaths list consisting of the file paths of destination files.

## [2.5.5] - 2023-05-05
### Fixed
- Fixed Permission denied error when using mounted CIFS share by canonizing the destination path. 

## [2.5.4] - 2023-02-24
### Fixed
- Fixed bug with task trying to rename the source file after exception without it being renamed in the first place.

## [2.5.3] - 2023-02-21
### Fixed
- Fixed bug with FilePaths when using list of objects in parameter.
- Documentational fixes to the FilePaths parameter.

## [2.5.2] - 2023-02-14
### Added
- Re-enabled key exchange algorithms 'curve25519-sha256' and 'curve25519-sha256@libssh.org'.

## [2.5.1] - 2023-02-09
### Added
- Added cancellationToken to method ListSourceFiles so Task's execution can be terminated via Frends.

## [2.5.0] - 2022-12-30
### Fixed
- [Breaking] Fixed issue where keepaliveinterval and operationtimeout is set as the connectiontimeout by creating them for the keepaliveinterval its own parameter and removed operationtimeout.

## [2.4.1] - 2022-12-01
### Updated
- Updated dependencies System.Text.Encoding.CodePages and Microsoft.Extensions.DependencyInjection to the newest version.
- Modified test to run against net471 instead of net6.

## [2.4.0]
### Added
- [Breaking] Added parameters for the file extension of temporary source and destination files when rename options are enabled.
- Fixed operations log to show correct state when source files are not found with filePaths.
- Fixed operations log to use temp work path when getting source files to temp directory.
- Added tests for the filePaths.

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

## [2.2.0] - 2022-09-28
### Fixed
- [Breaking] Added option to enable keyboard-interactive authentication method. This change will break the automatic updates of the task.
- Fixed issue with connecting to server which uses keyboard-interactive authentication method. Fixed by adding UseKeyboardInteractiveAuthentication parameter and handled the method inside tha task.

## [2.1.3] - 2022-09-23
### Fixed
- Fixed issue with the error message by changing how the error message is build. Error message had 'SFTP://' in both endpoints.

## [2.1.2] - 2022-09-16
### Fixed
- Fixed error handler by adding connection check to FileTransporter and SingleFileTransfer classes. If the client is not connected the task tries to connect again before handling errors.
- Added some tests and separated Appending tests to their own class.

## [2.1.1] - 2022-09-14
### Updated
- Updated depricated library Microsoft.Extensions.DependencyInjection from 5.0.1 to 6.0.0

## [2.1.0] - 2022-09-08
### Fixed
- [Beaking] Removed UTF-16 and Unicode FileEncoding because their implementation didn't work. These were used as a parameter so autoupdate won't work.
- Fixed how the Encoding on windows-1252 is handled. Added NuGet System.Text.Encoding.CodePages which can handle that encoding.
- Fixed error handling by adding catch to FileTransporter to catch SftpPathNotFoundException and general Exception.
- Added HostKeyAlgorithm parameter which enables users to change the host key algorithm used in the task. Before task defaults to ED25519.
- Added tests to test the file name and content encoding.
- Updated the document to state that Ssh.Net only supports private keys in OpenSSH and ssh.com formats.
- Added documentation on the private key formatingm from putty.ppk to OpenSSH.
- Moved all the SourceOperation tests to their own test class. 
- Fixed issue with server fingerprint given by user in SHA256 hex format was not accepted: Added conversion to the fingerprint given by user.
- Fixed issue when using invalid server fingerprint in MD5 string format throws wrong error message: Added more specific error messages.
- Changed how MD5 string is handled. MD5 can now be given without ':' or '-' characters.
- Fixed issue that Sha256 was only accepted in Base64 format: Added support for Sha256 in hex format.
- Added selector for host key algorithm which when enabled will force the task to use specific algorithm.
- Added more tests for using server fingerprints.
- Fixed issue where when using SourceOperation.Move the source file cannot be restored when exception occurs. 

## [2.0.0] - 2022-08-08
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
