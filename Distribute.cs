using ICP.Standard.Signature;
using ICP.Standard.Signature.V1;
using Infor.DocumentManagement.ICP;
using Infor.DocumentManagement.ICP.Distribution.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static ICP.Standard.Signature.V1.SignatureEnvelope.Recipient;

namespace Example
{
	class Distribute
	{
		static void Main(string[] args)
		{
			string printerId = "abcdefg";
			string ionApiPath = "C:/Temp/vaclavtest.ionapi";
            Connection conn = new Connection(null, ionApiPath, null, AuthenticationMode.OAuth2);
			conn.Connect(false);

            // MDC traceability headers for logging correlation across services
            // CorrelationId: auto-generated with "req-" prefix if not set (e.g. "req-4610ea31-37b2-4e25")
            // conn.CorrelationId = "req-custom-id-here";
            // CallerId: auto-generated based on auth mode:
            //   OAuth1:        DOTNETSDK-OAuth1-{first 4 chars of ConsumerKey}
            //   OAuth2:        DOTNETSDK-OAuth2-{first 4 chars of ClientId}
            //   Token:         DOTNETSDK-Token-{clienttype from JWT}
            //   Authorization: DOTNETSDK-Auth
            // conn.CallerId = "MyCustomApp";

            SubmitExample(conn, printerId);
			Console.ReadLine();
		}

		private static void SubmitExample(Connection conn, String printerId)
		{

			SubmitJob job = CreateSubmitJob(printerId);
			SubmitResult status = null;
			try
			{
				status = Distribution.Distribute(conn, job);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return;
			}

			if (!status.Success)
			{
				Console.WriteLine("Could not Submit job. Error: " + status.ErrorMessage);
				return;
			}

			StatusResult res = null;
			for (int i = 0; i < 20; i++)
			{
				res = Distribution.Status(conn, status.JobId);
				if (res.Status == StatusEnum.pending || res.Status == StatusEnum.inProgress)
				{
					Console.WriteLine("Job not completed, sleep for 2 sec and ask again.");
					Thread.Sleep(2000);
				}
				else
				{
					break;
				}
			}

			if (res.Status == StatusEnum.pending)
			{
				Console.WriteLine("Job is still not done. Investigate why!");
				return;
			}
			else if (res.Status == StatusEnum.ok)
			{
				Console.WriteLine("Job has finished. Exection time (ms) = " + res.TimeToComplete + ". Job submitted at (UTC) " + res.TimeFromSubmit);
				foreach (TargetStatus target in res.Targets)
				{
					if (target.Type == TargetType.print)
					{
						Console.WriteLine("Printed a document (use separate API to query print job). Print ID = " + target.Id);
					}
					else if (target.Type == TargetType.signature)
					{
						Console.WriteLine("Dpcument sent to Sign: " + target.Id);
					}
				}
			}
			else if (res.Status == StatusEnum.fail)
			{
				if (res.Phase == PhaseEnum.validate)
				{
					Console.WriteLine("Job failed during validation. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.retrieveIdmDocuments)
				{
					Console.WriteLine("Job failed when retrived documents from IDM. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.generate)
				{
					Console.WriteLine("Job failed when generating documents. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.assemble)
				{
					Console.WriteLine("Job failed when assembling documents. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.distribute)
				{
					Console.WriteLine("One or more of the targets could not be fulfilled.");
					foreach (TargetStatus target in res.Targets)
					{
						if (res.Status == StatusEnum.fail)
						{
							if (target.Type == TargetType.email)
							{
								Console.WriteLine("Failed to send mail. Error: " + target.Description);
							}
							else if (target.Type == TargetType.item)
							{
								Console.WriteLine("Failed to save to IDM. Error: " + target.Description);
							}
							else if (target.Type == TargetType.print)
							{
								Console.WriteLine("Failed to print. Error: " + target.Description);
							}
							else if (target.Type == TargetType.signature)
							{
								Console.WriteLine("Failed to Sign. Error: " + target.Description);
							}
						}
						else
						{
							if (target.Type == TargetType.print)
							{
								Console.WriteLine("Printed a document (use separate API to query print job). Print ID = " + target.Id);
							}
							else if (target.Type == TargetType.email)
							{
								Console.WriteLine("E-mail: " + target.Description);
							}
							else if (target.Type == TargetType.signature)
							{
								Console.WriteLine("Signature: " + target.Description);
							}

						}
					}
				}
			}
		}

		private static SubmitJob CreateSubmitJob(string printerId)
		{
			SubmitJob job;
			string orderNo = Guid.NewGuid().ToString();
			job = CreateGenerateJob(orderNo);

			AddStoreToIdmTarget(job, orderNo);
			if (printerId != null)
			{
				AddPrintTarget(job, printerId);
			}

			// AddSignatureTarget(job);
			// AddSignatureTargetWithTabs(job);
			return job;
		}

        private static SubmitJob CreateGenerateJob(string orderNo)
		{

            Byte[] bytes = File.ReadAllBytes("..\\..\\input\\IDMMailTemplateLayout.docx");
            String base64template = Convert.ToBase64String(bytes);

			bytes = File.ReadAllBytes("..\\..\\input\\IDMMailTemplateLayout.xml");
			String base64data = Convert.ToBase64String(bytes);

            SubmitJob job = new SubmitJob();

            DataFile template = new DataFile("IDMMailTemplateLayout.docx", base64template);
            DataFile data = new DataFile("IDMMailTemplateLayout.xml", base64data);
            GenerateFile generate = new GenerateFile("generated_HTML_file.pdf", data, template);
            job.Input.Add(generate);

	    /*	//For the input type Embed
			EmbedFile embed = new EmbedFile("SDK_Embedded_File.pdf", generate, PdfA3Format.PDF_A3_U);
			EmbeddedFile embeddedFile = new EmbeddedFile(data, "embeddingSameGeneratedFile.pdf", "SDK Embeddeding", "application/xml", AFRelationship.Alternative);

			//add metadata
			embed.MetaData = new Metadata
			{
				Title = "Test PDF Document",
				Author = "Test Author",
				Subject = "PDF/A-3 with Embedded File",
				Keywords = "PDF, Embedded, Factur-X"
			};

			// Add XMP metadata 
			embed.XmpMetaData = new XmpMetadata
			{
				SchemaPrefix = "fx",
				SchemaNamespaceUri = "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#",
				SchemaDescription = "Factur-X PDFA Extension Schema",
				Properties = new List<XmpProperty>
				{
			new XmpProperty
			{
				Name = "DocumentType",
				Value = "Invoice",
				ValueType = "Text",
				categoryType = XMPPropertyCategoryType.External,
				Description = "The type of the hybrid document in capital letters, e.g. INVOICE or ORDER"
						},
						new XmpProperty
						{
							Name = "DocumentFileName",
							Value = "factur-x.xml",
							ValueType = "Text",
							categoryType = XMPPropertyCategoryType.External,
							Description = "This the embedded file name used to generated the invoice"
						}
						}
			};

			embed.EmbeddedFiles.Add(embeddedFile);

			// Add to job input
			job.Input.Add(embed);  */

			return job;
		}

		private static SubmitJob AddStoreToIdmTarget(SubmitJob job, string orderNo)
		{
			string generateFileName = "OrderConfirmation_" + orderNo + ".pdf";

			CMItem item = new CMItem();
			item.EntityName = "MDS_File";

			//item.setAttributeValue("MDS_Name", generateFileName, DataType.STRING);

			ItemTarget target = new ItemTarget();
			target.SetItemDataFile(item);

			job.Targets.Add(target);
			return job;
		}

		private static SubmitJob AddPrintTarget(SubmitJob job, string printerId)
		{
			PrintTarget printTarget = new PrintTarget(printerId)
			{
				MediaSize = new MediaSize()
				{
					Height = 500, Width = 200, MarginFromLeft = 10, MarginFromTop = 10
				},
				Scaling = ICP.Standard.Distribution.V1.Scaling.SHRINK_TO_FIT
			};
			job.Targets.Add(printTarget);
            return job;
		}

        private static SubmitJob AddSignatureTarget(SubmitJob job)
		{
			SignatureTarget target = new SignatureTarget();
			SignatureEnvelope envelope = new SignatureEnvelope();
			envelope.Message = "TEST IDM SIGNATURE";
			envelope.Subject = "DOCUSIGN TEST";

			List<SignatureEnvelope.Recipient> recipients = new List<SignatureEnvelope.Recipient>();
			SignatureEnvelope.Recipient recipient = new SignatureEnvelope.Recipient();
			recipient.Name = "Test User";
			recipient.Email = "test.user@infor.com";
			recipient.Order = 1;

            RecipientSignatureProviderOptions providerOptions = new RecipientSignatureProviderOptions
            {
                Sms = "+91XXXXXXXXXX" //Enter your phone number
            };

            RecipientSignatureProvider signatureProvider = new RecipientSignatureProvider
            {
                SignatureProviderName = "UniversalSignaturePen_OpenTrust_Hash_TSP",
                SignatureProviderOptions = providerOptions
            };

            recipient.RecipientSignatureProviders = new List<RecipientSignatureProvider> { signatureProvider };

            // Add the recipient
            recipients.Add(recipient);

			envelope.Recipients = recipients;
			target.Envelope = envelope;

			job.Targets.Add(target);
			return job;
		}

		private static SubmitJob AddSignatureTargetWithTabs(SubmitJob job)
		{
			SignatureTarget target = new SignatureTarget();
			SignatureEnvelope envelope = new SignatureEnvelope();
			envelope.Message = "TEST IDM SIGNATURE";
			envelope.Subject = "DOCUSIGN TEST";

			List<SignatureEnvelope.Recipient> recipients = new List<SignatureEnvelope.Recipient>();
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
			//recipients.Add(recipientTwo);

			envelope.Recipients = recipients;
			target.Envelope = envelope;

			job.Targets.Add(target);
			return job;
		}
	}
}
