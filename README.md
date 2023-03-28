# dfc-api-appRegistry

## Introduction

This function app is used to manage CUI app registrations consumed by the Composite Shell, which are persisted in a Cosmos database collection.

## Getting Started

This is a self-contained Visual Studio 2019 solution containing a number of projects (azure function app with associated unit test project).

### Installing

Clone the project and open the solution in Visual Studio 2019.

## List of dependencies

|Item|Purpose|
|----|-------|
|DFC.Compui.Cosmos|Cosmos repository nuget|
|DFC.Compui.Subscriptions|Composite UI Subscription API client|
|DFC.Swagger.Standard|DFC Swagger generator|


## Local Config Files

Once you have cloned the public repo you need to remove the -template part from the configuration file names listed below.

|Location|Filename|Rename to|
|--------|--------|---------|
|DFC.Api.AppRegistry|local.settings-template.json|local.settings.json|

## Configuring to run locally

The project contains a "local.settings-template.json" file which contains appsettings for the function app project. To use this file, copy it to "local.settings.json" and edit and replace the configuration item values with values suitable for your environment.

By default, the appsettings include local Azure Cosmos Emulator configurations using the well known configuration values for app registration storage. These may be changed to suit your environment if you are not using the Azure Cosmos Emulator.

This app subscribes to change events in Event Grid. To make use of it you will need to configure the Subscriptions service which will require an APIM API key for that service.

## App Settings

|App setting|Value|
|-----------|-----|
|ApiSuffix|dev|
|Configuration__CosmosDbConnections__AppRegistry__AccessKey|__CosmosAccessKey__|
|Configuration__CosmosDbConnections__AppRegistry__EndpointUrl|__CosmosEndpoint__|
|Configuration__CosmosDbConnections__AppRegistry__DatabaseId|composition|
|Configuration__CosmosDbConnections__AppRegistry__CollectionId|appregistry|
|Configuration__CosmosDbConnections__AppRegistry__PartitionKey|/PartitionKey|
|PagesClientOptions__BaseAddress|__ContentApiEndpoint__/api/execute/|
|SubscriptionSettings__Endpoint|__ThisFunctionAppBaseAddress__/pages/webhook|
|SubscriptionSettings__SubscriptionServiceEndpoint|__SubscriptionServiceEndpoint__|
|SubscriptionSettings__ApiKey|__SubscriptionServiceApimKey__|
|SubscriptionSettings__Filter__BeginsWith|/dfc-app-pages/|
|SubscriptionSettings__Filter__IncludeEventTypes__0|published|
|SubscriptionSettings__Filter__IncludeEventTypes__1|unpublished|
|SubscriptionSettings__Filter__IncludeEventTypes__2|deleted|

## Running locally

To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally.

To run the project, start the function app. Once running, use a tool such as Postman to initiate requests.

## Deployments

This function app will be deployed as an individual stand-alone deployment.

## Built With

* Microsoft Visual Studio 2019
* .Net Core 3.1

## References

Please refer to https://github.com/SkillsFundingAgency/dfc-digital for additional instructions on configuring individual components like Cosmos.
