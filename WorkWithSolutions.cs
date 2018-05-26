
// =====================================================================
//
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
//
// =====================================================================
//<snippetWorkWithSolutions>
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO;
using System.Reflection;

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// found in the SDK\bin folder.
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;

// This namespace is found in Microsoft.Crm.Sdk.Proxy.dll assembly
// found in the SDK\bin folder.
using Microsoft.Crm.Sdk.Messages;
using Newtonsoft.Json.Linq;


namespace Microsoft.Crm.Sdk.Samples
{

    /// <summary>
    /// Demonstrates common tasks performed using Microsoft Dynamics CRM Solutions
    /// </summary>
    public class WorkWithSolutions
    {
        #region Class Level Members

        /// <summary>
        /// Stores the organization service proxy.
        /// </summary>
        OrganizationServiceProxy _serviceProxy;

        private Guid _crmSdkPublisherId;
        private string userName;
        private string password;
        private string crmUrl;
        private System.String _customizationPrefix;
        private Guid _solutionsSampleSolutionId;
        System.String ManagedSolutionLocation;
        String outputDir = @"C:\temp\";
        public WorkWithSolutions(string filepath, string environment)
        {
            ManagedSolutionLocation = filepath;
            string configFile = "Configuration." + environment + ".json";
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configFile);
            JObject jso = JObject.Parse(File.ReadAllText(path));
            crmUrl = (string)jso["CrmUrl"];
            Console.WriteLine(crmUrl);
            userName = (string)jso["UserName"];
            Console.WriteLine(userName);
            password = (string)jso["Password"];
        }

        // Specify which language code to use in the sample. If you are using a language
        // other than US English, you will need to modify this value accordingly.
        // See http://msdn.microsoft.com/en-us/library/0h88fahh.aspx
        private const int _languageCode = 1033;
        
        #endregion Class Level Members

        private OrganizationServiceProxy AdultConnection(bool reset = false)
        {
            try
            {
                AuthenticationCredentials credentials = new AuthenticationCredentials();
                credentials.ClientCredentials.UserName.UserName = userName;
                credentials.ClientCredentials.UserName.Password = password;

                
                var orgServiceManagementAdult = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(new Uri(crmUrl));
                var authCredentialsAdult = orgServiceManagementAdult.Authenticate(credentials);
                

                OrganizationServiceProxy proxy = new OrganizationServiceProxy(orgServiceManagementAdult, authCredentialsAdult.SecurityTokenResponse);
                proxy.Timeout = new TimeSpan(0, 30, 0);
                return proxy;
            }
            catch (Exception e)
            {
                
                return null;
            }
        }

        #region How To Sample Code
        /// <summary>
        /// Shows how to perform the following tasks with solutions:
        /// - Create a Publisher
        /// - Retrieve the Default Publisher
        /// - Create a Solution
        /// - Retrieve a Solution
        /// - Add an existing Solution Component
        /// - Remove a Solution Component
        /// - Export or Package a Solution
        /// - Install or Upgrade a solution
        /// - Delete a Solution
        /// </summary>
        /// <param name="serverConfig">Contains server connection information.</param>
        /// <param name="promptForDelete">When True, the user will be prompted to delete all
        /// created entities.</param>
        public void Run( bool promptForDelete)
        {
            try
            {

                // Connect to the Organization service. 
                // The using statement assures that the service proxy will be properly disposed.
                using (_serviceProxy = AdultConnection())
                {
                    // This statement is required to enable early-bound type support.
                    _serviceProxy.EnableProxyTypes();

                    // Call the method to create any data that this sample requires.

                    //<snippetWorkWithSolutions3>
                    // Create a Solution
                    //Define a solution
                    //<snippetWorkWithSolutions8>
                    // Install or Upgrade a Solution                  
                    
                    byte[] fileBytesWithMonitoring = File.ReadAllBytes(ManagedSolutionLocation);

                    ImportSolutionRequest impSolReqWithMonitoring = new ImportSolutionRequest()
                    {
                        CustomizationFile = fileBytesWithMonitoring,
                        ImportJobId = Guid.NewGuid()
                    };
                    
                    _serviceProxy.Execute(impSolReqWithMonitoring);
                    Console.WriteLine("Imported Solution with Monitoring from {0}", ManagedSolutionLocation);

                    ImportJob job = (ImportJob)_serviceProxy.Retrieve(ImportJob.EntityLogicalName, impSolReqWithMonitoring.ImportJobId, new ColumnSet(new System.String[] { "data", "solutionname" }));
                    

                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.LoadXml(job.Data);

                    String ImportedSolutionName = doc.SelectSingleNode("//solutionManifest/UniqueName").InnerText;
                    String SolutionImportResult = doc.SelectSingleNode("//solutionManifest/result/@result").Value;

                    Console.WriteLine("Report from the ImportJob data");
                    Console.WriteLine("Solution Unique name: {0}", ImportedSolutionName);
                    Console.WriteLine("Solution Import Result: {0}", SolutionImportResult);
                    Console.WriteLine("");

                    // This code displays the results for Global Option sets installed as part of a solution.

                    System.Xml.XmlNodeList optionSets = doc.SelectNodes("//optionSets/optionSet");
                    foreach (System.Xml.XmlNode node in optionSets)
                    {
                        string OptionSetName = node.Attributes["LocalizedName"].Value;
                        string result = node.FirstChild.Attributes["result"].Value;

                        if (result == "success")
                        {
                            Console.WriteLine("{0} result: {1}",OptionSetName, result);
                        }
                        else
                        {
                            string errorCode = node.FirstChild.Attributes["errorcode"].Value;
                            string errorText = node.FirstChild.Attributes["errortext"].Value;

                            Console.WriteLine("{0} result: {1} Code: {2} Description: {3}",OptionSetName, result, errorCode, errorText);
                        }
                    }
                }
            }

            // Catch any service fault exceptions that Microsoft Dynamics CRM throws.
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
                // You can handle an exception here or pass it back to the calling method.
                throw;
            }
        }

        #endregion How To Sample Code

        #region Main
        /// <summary>
        /// Standard Main() method used by most SDK samples.
        /// </summary>
        /// <param name="args"></param>
        static public void Main(string[] args)
        {
            try
            {
                // DON'T NEED THIS AS I AM CONNECTING TO CRM VIA AdultConnection METHOD
                // Obtain the target organization's Web address and client logon 
                // credentials from the user.
                //ServerConnection serverConnect = new ServerConnection();
                //ServerConnection.Configuration config = serverConnect.GetServerConfiguration();

                WorkWithSolutions app = new WorkWithSolutions(args[0], args[1]);
                app.Run(true);
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                Console.WriteLine("Message: {0}", ex.Detail.Message);
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                Environment.Exit(-1);
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
                Environment.Exit(-1);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);

                    FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                        Console.WriteLine("Message: {0}", fe.Detail.Message);
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText);
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
                Environment.Exit(-1);

            }
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

        }
        #endregion Main

    }
}
//</snippetWorkWithSolutions>