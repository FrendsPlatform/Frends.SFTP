# Frends.SFTP.WriteFile

[![Frends.SFTP.WriteFile Main](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/WriteFile_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/WriteFile_build_and_test_on_main.yml)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.SFTP.WriteFile?label=NuGet)
![GitHub](https://img.shields.io/github/license/FrendsPlatform/Frends.SFTP?label=License)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.SFTP/Frends.SFTP.WriteFile|main)

Writes a file to SFTP server.

## Installing

You can install the task via FRENDS UI Task View or you can find the NuGet package from the following NuGet feed

### Source

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Directory | `string` | Directory of the server. | `/` |
| FileName | `string` | File name with extension to fetch. | `test.txt` |

### Destination

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Directory | `string` | Directory of the server. | `/` |
| Operation | `enum` | Operation to determine what to do if destination file exists. | `Error` |

### Connection

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Address | `string` | SFTP host address. | `HOSTNAME` |
| Port | `int` | Port number. | `22` |
| Authentication | `enum` | Selection for authentication type. | `UsernamePassword` |
| UserName | `string` | Username. | `foo` |
| Password | `string` | Password. | `pass` |
| PrivateKeyFileName | `string` | Full path to private key file. | `, ` |
| Passphrase | `string` | Passphrase for the private key file. | `, ` |

### Result

A result object with parameters.

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| FileName | `string` | The name of the file. Does not include the path. | `test.txt` |
| SourcePath | `string` | The full source path of the file. | `C:\test.txt` |
| Success | `bool` | Boolean value of the successful transfer. | `true` |

# Building

Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.SFTP.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

