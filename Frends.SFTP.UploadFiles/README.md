# Frends.SFTP.UploadFiles

[![Frends.SFTP.UploadFiles Main](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/UploadFiles_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/UploadFiles_build_and_test_on_main.yml)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.SFTP.UploadFiles?label=NuGet)
![GitHub](https://img.shields.io/github/license/FrendsPlatform/Frends.SFTP?label=License)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.SFTP/Frends.SFTP.UploadFiles|main)

Uploads a file to SFTP server. Operations log is implemented by using Serilog library. 

## Installing

You can install the task via FRENDS UI Task View or you can find the NuGet package from the following NuGet feed

# Building

Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.SFTP.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

