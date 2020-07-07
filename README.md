# Project template repository

This directory contains a "template repo" for creating new repositories from.

It defaults to .dotnet core 2.2, but if you update global.json, it will support netcore 3.0 and 3.1.

When using this,  please run

`Scripts\New-InitialDotNetCoreProjects.ps1 -Prefix <project name> -ProjectType <project type>`

to create the project structure and correctly populate the project guids in the csproj files.

Supported project types are currently:

* mvc
* console
* classlib
* function

Then delete this section, and remove the Scripts folder from the repo.

Then it can be PR'd into the appropriate branch

# SomeProjectName

## Introduction

An introduction to the project goes here!