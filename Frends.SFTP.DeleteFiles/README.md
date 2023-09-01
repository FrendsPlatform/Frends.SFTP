# Frends.SFTP.DeleteFiles
Frends Task for deleting files from SFTP server.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/FrendsPlatform/Frends.SFTP/actions/workflows/DeleteFiles_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.SFTP/actions)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.SFTP/Frends.SFTP.DeleteFiles|main)

## Installing

You can install the Task via frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.SFTP.git`

### Build the project

`dotnet build`

### Run tests

`cd Frends.SFTP.DeleteFiles.Tests`

Run the Docker compose from Frends.SFTP.DeleteFiles.Tests directory using

`docker-compose up -d`

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`

### Third-party licenses

SonarAnalyzer.CSharp version (unmodified version 9.8.0.76515) used to analyze bugs, vulnerabilities and code smells uses LGPLv3, full text and source code can be found in https://github.com/SonarSource/sonar-dotnet.