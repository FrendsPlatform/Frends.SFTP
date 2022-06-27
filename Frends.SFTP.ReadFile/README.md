# Frends.SFTP.ReadFile

[![Frends.SFTP.ReadFile Main](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/ReadFile_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/ReadFile_build_and_test_on_main.yml)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.SFTP.ReadFile?label=NuGet)
![GitHub](https://img.shields.io/github/license/FrendsPlatform/Frends.SFTP?label=License)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.SFTP/Frends.SFTP.ReadFile|main)

Reads a file from SFTP server

## Installing

You can install the task via FRENDS UI Task View or you can find the NuGet package from the following NuGet feed

## Building

### Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.SFTP.git`

### Change directory to task directory.

`cd Frends.SFTP/Frends.SFTP.ReadFile`

### Build the project

`dotnet build`

### Run tests

`cd Frends.SFTP.ReadFile.Tests`

Run the Docker compose from Frends.SFTP.ReadFile.Tests directory using

`docker-compose up -d`

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`

