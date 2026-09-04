using ICP.Standard.Signature;
using ICP.Standard.Signature.V1;
using Infor.DocumentManagement.ICP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Example
{
    class Signature
    {
        static void Main(string[] args)
        {
            try
            {
                // Create and connect the connection
                Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
                conn.Connect();

                //example values
                string templateId = "fd1fd9a1-a99c-4f7c-b10d-efce31560965";
                string code = "eyJ0eXAiOiJNVCIsImFsZyI6IlJTMjU2Iiwia2lkIjoiNjgxODVmZjEtNGU1MS00Y2U5LWFmMWMtNjg5ODEyMjAzMzE3In0.AQsAAAABAAYABwCAJJ5PwpLYSAgAgLAkl8KS2EgCALnIV80lCYJKqq8SH7dNgGUVAAEAAAAYAAMAAAAKAAAAHQAAAAUAAAANACQAAABlMTEyN2VkMy1mMWI1LTQwZTQtYjQzMS0wNmMxOWYzOTgzYmQiACQAAABlMTEyN2VkMy1mMWI1LTQwZTQtYjQzMS0wNmMxOWYzOTgzYmQwAIAknk_CkthIEgABAAAACwAAAGludGVyYWN0aXZlNwCgpjuMxqqbQoABdbw4bHkj.P5CF96E2sFHxQKFG6PIgF9LMXk0KHBGK6jBePRE3ToJGjwtnYvfGG2r_ENKWG9GkX751SnhQDowXOOc3ccFTOiPRKaz1PxQBAzs6RcBPO6FY3OFf41cj1DXwuDfX1yBfaNj7ZYxgIB0AZEROFnska-ixK6bDEFh1gSoxigltYB0ig1NaMRJSBmXiyrIagd13S9qKgLZrexkFJTSAQHhjcXT_lMPhrbpajfVhN3eQg-Y_amktyXUsPKuqnOgQbozmBf7TweVZKiZ-xqFHnXOcM-eE3cg1LN0lqrED3tJp9C9qOhD7kKWR9PkmcdeeignIS6XwSGycShGUXryny1M4qg";
                string state = "";

                SignatureResponse<SignatureTemplate> templateByIdResponse = ICP.Standard.Signature.V1.Signature.getTemplate(conn, templateId);
                SignatureAuthorization authorization = ICP.Standard.Signature.V1.Signature.loginAuthorization(conn, code, state);

                SignatureEnvelope envelope = new SignatureEnvelope();
                List<SignatureEnvelope.Recipient> recipients = new List<SignatureEnvelope.Recipient>();
                List<string> items = new List<string>();
                List<SignatureEnvelope.Content> contents = new List<SignatureEnvelope.Content>();

                items.Add("MDS_File-1-1-LATEST");
                items.Add("MDS_File-2-1-LATEST");

                SignatureEnvelope.Recipient recipientOne = new SignatureEnvelope.Recipient();
                SignatureEnvelope.Recipient recipientTwo = new SignatureEnvelope.Recipient();
                recipientOne.Name = "Test User1";
                recipientOne.Email = "test.user1@infor.com";
                recipientOne.Order = 1;

                /*recipientTwo.Name = "Test User2";
                recipientTwo.Email = "test.user2a@infor.com";
                recipientTwo.Order = 2;*/

                // Declare objects to set up various tab options
                SignatureTabs.TabPosition position;
                SignatureTabs.TabOptions options;
                SignatureTabs.FontOptions fontOptions;

                // Create and fill up a tab of SignHereTab type
                SignatureTabs.SignHereTab signHereTab = new SignatureTabs.SignHereTab();
                // Set up tab position
                position = new SignatureTabs.TabPosition();
                position.anchorUse = true;
                position.anchorString = "Sign here please";
                position.anchorXOffset = 0;
                position.anchorYOffset = 13;
                position.anchorUnits = "mms";
                position.anchorHorizontalAlignment = "right";

                options = new SignatureTabs.TabOptions();
                options.tabOrder = 1;
                options.tabLabel = "Test Tab 1";

                signHereTab.position = position;
                signHereTab.options = options;
                recipientOne.SignatureTabs.signHereTabs.Add(signHereTab);

                // Create and fill up a tab of initialHereTab type
                SignatureTabs.InitialHereTab initialHereTab = new SignatureTabs.InitialHereTab();
                // Set up tab position
                position = new SignatureTabs.TabPosition();
                position.anchorUse = true;
                position.anchorString = "Sign here please";
                position.anchorXOffset = 40;
                position.anchorYOffset = 13;
                position.anchorUnits = "mms";
                position.anchorHorizontalAlignment = "right";

                // Set up tab options
                options = new SignatureTabs.TabOptions();
                options.tabOrder = 2;
                options.tabLabel = "Test Tab 2";

                // Set all properties
                initialHereTab.position = position;
                initialHereTab.options = options;
                recipientOne.SignatureTabs.initialHereTabs.Add(initialHereTab);

                // Create and fill up a tab of fullNameTab type
                SignatureTabs.FullNameTab fullNameTab = new SignatureTabs.FullNameTab();
                // Set up tab position
                position = new SignatureTabs.TabPosition();
                position.documentId = "MDS_File-1-1-LATEST";
                position.pageNumber = 1;
                position.xPosition = 40;
                position.yPosition = 20;

                // Set up tab options
                options = new SignatureTabs.TabOptions();
                options.tabOrder = 3;
                options.tabLabel = "Test Tab 3";
                options.width = 100;
                options.height = 60;

                // Set up font options
                fontOptions = new SignatureTabs.FontOptions();
                fontOptions.font = "Arial";
                fontOptions.fontSize = 16;
                fontOptions.fontColor = "red";
                fontOptions.bold = true;
                fontOptions.italic = true;

                // Set all properties
                fullNameTab.position = position;
                fullNameTab.options = options;
                fullNameTab.fontOptions = fontOptions;

                // Add the tab
                recipientOne.SignatureTabs.fullNameTabs.Add(fullNameTab);

                // Create and fill up a tab of dateSignedTab type
                SignatureTabs.DateSignedTab dateSignedTab = new SignatureTabs.DateSignedTab();
                // Set up tab position
                position = new SignatureTabs.TabPosition();
                position.documentId = "MDS_File-2-1-LATEST";
                position.pageNumber = 1;
                position.xPosition = 500;
                position.yPosition = 720;

                // Set up tab options
                options = new SignatureTabs.TabOptions();
                options.tabOrder = 4;
                options.tabLabel = "Test Tab 4";
                options.width = 100;
                options.height = 60;

                // Set up font options
                fontOptions = new SignatureTabs.FontOptions();
                fontOptions.font = "Sans Serif";
                fontOptions.fontSize = 16;
                fontOptions.fontColor = "green";

                // Set all properties
                dateSignedTab.position = position;
                dateSignedTab.options = options;
                dateSignedTab.fontOption = fontOptions;

                // Add the tab
                recipientOne.SignatureTabs.dateSignedTabs.Add(dateSignedTab);

                // Add the recipient
                recipients.Add(recipientOne);
                recipients.Add(recipientTwo);

                SignatureEnvelope.Content content = new SignatureEnvelope.Content();
                if (templateByIdResponse != null && templateByIdResponse.Data != null)
                {
                    content.Id = templateByIdResponse.Data.Contents[0].Id;
                    content.File = templateByIdResponse.Data.Contents[0].File;
                    content.Ext = templateByIdResponse.Data.Contents[0].Ext;
                    content.Data = templateByIdResponse.Data.Contents[0].Data;
                    contents.Add(content);
                    envelope.Contents = contents;
                }

                envelope.Subject = "DS - Sample Signature Request";
                envelope.Message = "DocuSign service for Infor s.r.o.\\nSample email for sign envelope of documents.";
                envelope.Recipients = recipients;
                envelope.Items = items;

                SignatureResponse<SignatureEnvelope.EnvelopeStatus> sendEnvelopeResponse = ICP.Standard.Signature.V1.Signature.sendEnvelope(conn, false, envelope);
               /* SignatureResponse<SignatureListTemplatesResponse> templatesList = ICP.Standard.Signature.V1.Signature.listTemplates(conn);
                SignatureEnvelope.EnvelopeDetail envelopeByIdResponse = ICP.Standard.Signature.V1.Signature.getEnvelopeById(conn, signatureId);
                List<SignatureEnvelope.EnvelopeStatus> envelopeByPidResponse = ICP.Standard.Signature.V1.Signature.getEnvelopeByPid(conn, pid);
                SignatureResponse<string> response = ICP.Standard.Signature.V1.Signature.deleteEnvelope(conn, signatureId); 
                SignatureResourceData envelopeResourceByIdResponse = ICP.Standard.Signature.V1.Signature.getEnvelopesResource(conn, signatureId);
                if (envelopeResourceByIdResponse != null)
                {
                    Stream outputStream = new FileStream("..\\..\\output\\" + envelopeResourceByIdResponse.Filename, FileMode.Create);
                    CMResource.StreamData(envelopeResourceByIdResponse.Stream, outputStream, true);
                    Console.WriteLine("Successfully stored file: " + envelopeResourceByIdResponse.Filename + " to output folder");
                }
                List<UserFilterResponse.User> usersResponse = ICP.Standard.Signature.V1.Signature.getUsers(conn, "User", "TestUser@infor.com");  */    

                conn.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
