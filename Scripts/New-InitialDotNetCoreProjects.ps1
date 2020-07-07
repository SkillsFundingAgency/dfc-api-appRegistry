<#
.SYNOPSIS
Create an initial dotnet core project

.DESCRIPTION
Create an initial dotnet core project,  also populating the project guids from the solution file into the csproj files.

.PARAMETER Prefix
The prefix for the project name.  
The script will create:

A web project called $Prefix
A unit test project called $Prefix.UnitTests
A solution file called $Prefix.sln

.PARAMETER ProjectType
The type of project to create. Valid options are currently:
* mvc
* console
* classlib
* function

.EXAMPLE
New-InitialDotNetCoreProjects.ps1 --Prefix SomeProject --ProjectType web

#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string] $Prefix,
    [Parameter(Mandatory=$true)]
    [ValidateSet("mvc", "console", "classlib", "function")]
    [string] $ProjectType
)

function New-BasicProject {
    param(
        [Parameter(Mandatory=$true)]
        [string] $Name,
        [Parameter(Mandatory=$true)]
        [ValidateSet("mvc", "console", "classlib")]
        [string] $ProjectType
    )

    $testProject = $Name + ".UnitTests"

    & "dotnet" "new" $ProjectType "--name" $Name
    & "dotnet" "new" "xunit" "--name" $testProject

    & "dotnet" "new" "sln" "--name" $Name
    & "dotnet" "sln" "add" "$($Name)/$($Name).csproj"
    & "dotnet" "sln" "add" "$($testProject)/$($testProject).csproj"
}

function New-FunctionProject {
    param(
        [Parameter(Mandatory=$true)]
        [string] $Name
    )

    $testProject = $Name + ".UnitTests"

    # Ensure the azure function dotnet cli template is installed
    & "dotnet" "new" "-i" "Microsoft.Azure.WebJobs.ProjectTemplates::2.0.10369" | Out-Null

    New-Item -Path $Name -ItemType Directory
    Push-Location
    Set-Location -Path $Name

    & "dotnet" "new" "function" "--name" $Name

    Pop-Location

    & "dotnet" "new" "xunit" "--name" $testProject

    & "dotnet" "new" "sln" "--name" $Name
    & "dotnet" "sln" "add" "$($Name)/$($Name).csproj"
    & "dotnet" "sln" "add" "$($testProject)/$($testProject).csproj"
}

function Invoke-PopulateProjectGuidFromSolution {
    param(        
        [Parameter(Mandatory=$true)]
        [string] $SolutionName
    )

    $solutionContent = Get-Content -Path $SolutionName

    # Projects in the solution file have a distict structure:
    # Project("<Project Type ID>") = "<Project Name>", "<Path To Project>", "<Project Guid>"
    # The following searches for lines that match that pattern, and iterates over them,  placing
    # the Project Guid into the project file.

    $projects = $solutionContent | Select-String -Pattern '^Project\(.*\) = ".*", "(.*)", "(.*)"$'
    
    foreach($project in $projects) {
        $projectFile = Resolve-Path $project.Matches.Groups[1].Value
        $projectGuid = $project.Matches.Groups[2].Value

        [xml]$projectDocument = Get-Content -Path  $projectFile
        
        $projectGuidElement = $projectDocument.CreateElement("ProjectGuid")
        $projectGuidElement.InnerText = $projectGuid 

        $projectDocument.Project.PropertyGroup.AppendChild($projectGuidElement)

        $projectDocument.Save($projectFile)
    }
}

Write-Output "Creating template project for '$Prefix' with type '$ProjectType'"

switch($ProjectType) {
    "function" { 
        New-FunctionProject -Name $Prefix
    }
    default {
        New-BasicProject -Name $Prefix -ProjectType $ProjectType
    }
}

Write-Output "Updating project files with ProjectGuids from solution"
Invoke-PopulateProjectGuidFromSolution -SolutionName "$($Prefix).sln"

$PipelineContent = Get-Content -Path $PSScriptRoot\..\Resources\AzureDevOps\azure-pipelines.yml
$PipelineContent = $PipelineContent -replace "__SolutionBaseName__", $Prefix
Write-Output "Updating SolutionBaseName in azure-pipelines.yml to $Prefix"
Set-Content -Path $PSScriptRoot\..\Resources\AzureDevOps\azure-pipelines.yml -Value $PipelineContent

Write-Output "Done"