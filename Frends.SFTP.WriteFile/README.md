# Frends.SFTP.WriteFile

[![Frends.SFTP.WriteFile Main](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/WriteFile_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/WriteFile_build_and_test_on_main.yml)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.SFTP.WriteFile?label=NuGet)
![GitHub](https://img.shields.io/github/license/FrendsPlatform/Frends.SFTP?label=License)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.SFTP/Frends.SFTP.WriteFile|main)

Executes file transfer through SFTP connection.

## Installing

You can install the task via FRENDS UI Task View or you can find the NuGet package from the following NuGet feed

### Properties

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Message | `string` | Some string that will be repeated. | `foo` |

### Options

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Amount | `int` | Amount how many times message is repeated. | `3` |
| Delimiter | `string` | Character(s) used between replications. | `, ` |

### Returns

A result object with parameters.

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Replication | `string` | Repeated string. | `foo, foo, foo` |

Usage:
To fetch result use syntax:

`#result.Replication`

# Building

Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.SFTP.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

