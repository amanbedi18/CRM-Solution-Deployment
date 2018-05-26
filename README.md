# CRM-Solution-Deployment

## Introduction
**_Solutions_** is a console app which provides importing a new CRM managed solution zip file / updating an existing CRM solution in a Microsoft Dynamics (Dynamics 365 (online) / Dynamics 365 (on-premises) / Dynamics CRM 2016 / Dynamics CRM Online) using the Microsoft Dynamics 365 SDK. 

## Integrate with VSTS
Enterprise solutions are often complex and are usually deployed manually to the target CRM environment, however with VSTS modern DevOps offerings and existing Dynamics 365 SDK, deployment automation can be achieved from the start. This console application can be used as a Batch Process in a VSTS release pipeline for continuous deployment of managed CRM solutions (assuming the build artifacts generate a deploy-able zip package of the same).

## Prerequisites
Before executing the console app ensure the following are in place:
* Open the **_Configuration.Dev.json_** / **_Configuration.Prod.json_** & update the following:
  1. **_CrmUrl_**: The URL for the CRM instance.
  2. **_UserName_**: The CRM admin user name.
  3. **_Password_**: The password for CRM admin user.
* Ensure a deploy-able zip package of an existing managed CRM solution is present.

## Execute
Execute the **_BasicOperations_** exe from the **_Bin/Debug_** or **_Bin/Release_** directory of the Solution with the following command line arguments as : "{Full path of the deploy-able zip package of an existing managed CRM solution}" "{Environment Identifier : Dev / Prod}"

On executing the **_BasicOperations_** exe with the required command line arguments the following steps are executed:
* Establishes a connection with the remote CRM instance via **_OrganizationServiceProxy_** by initializing the same with the configured CRM admin credentials for configured CRM environment endpoint for current environment's Configuration json file as chosen by command line argument for environment identifier.
* Imports the CRM managed solution / upgrades existing CRM solution in the target CRM instance.
