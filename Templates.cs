using Infor.DocumentManagement.ICP;
using Infor.DocumentManagement.ICP.Template;
using System;
using System.IO;

namespace Example
{
	class Templates
	{
		static void Main(string[] args)
		{
			try
			{
				// Create and connect the connection
				Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
				conn.Connect();

				// Example 1
				PopulateTemplateFromController(conn);

				// Example 2
				PopulateTemplateFromXML(conn);

				// Example 3
				PopulateTemplateAddItem(conn);

				conn.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		/*
		Uses a controller file to populate the template and saves the result to disk.
		*/
		public static void PopulateTemplateFromController(Connection conn)
		{
			// Load template from disk
			FileStream template = new FileStream("..\\..\\input\\ItemListTemplate.docx", FileMode.Open);

			// Load template controller file from disk
			FileStream templateController = new FileStream("..\\..\\input\\ItemListController.xml", FileMode.Open);

			// Create the template request
			// Specifies the template, controller and the output filename
			TemplateRequest templateRequest = new TemplateRequest.TemplateRequestBuilder(template)
			.TemplateController(templateController, TemplateRequest.TemplateRequestBuilder.ControllerType.Xml)
			.Filename("PopulatedItemList.pdf")
			.Build();

			// Populate the template
			CMResourceData resData = Template.PopulateTemplate(conn, templateRequest);

			// Save the file response to disk
			string savePath = "..\\..\\output\\" + resData.Filename;
			FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate);
			CMResource.StreamData(resData.Stream, fs, true);

			Console.WriteLine("Response saved to location: " + savePath);
		}

		/*
		Uses a BOD xml for the data parameter to populate the template and saves the result to disk.
	    InvoiceTemplate has been mapped using InvoiceBod.xml and InvoiceBod2.xml contains new data.
		*/
		public static void PopulateTemplateFromXML(Connection conn)
		{
			// Load template from disk
			FileStream template = new FileStream("..\\..\\input\\InvoiceTemplate.docx", FileMode.Open);

			// Load template controller file from disk
			FileStream data = new FileStream("..\\..\\input\\InvoiceBod2.xml", FileMode.Open);

			// Create the template request
			// Specifies the template, data and the output filename
			TemplateRequest templateRequest = new TemplateRequest.TemplateRequestBuilder(template)
			.Data(data)
			.Filename("PopulatedInvoice.pdf")
			.Build();

			// Populate the template
			CMResourceData resData = Template.PopulateTemplate(conn, templateRequest);

			// Save the file response to disk
			string savePath = "..\\..\\output\\" + resData.Filename;
			FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate);
			CMResource.StreamData(resData.Stream, fs, true);

			Console.WriteLine("Response saved to location: " + savePath);
		}

		/*
		Populates a template using a controller file and adds the result to IDM.
		*/
		public static void PopulateTemplateAddItem(Connection conn)
		{
			// Load template from disk
			FileStream template = new FileStream("..\\..\\input\\ItemListTemplate.docx", FileMode.Open);

			// Load template controller file from disk
			FileStream templateController = new FileStream("..\\..\\input\\ItemListController.xml", FileMode.Open);

			// Create the item that should be added to IDM
			CMItem item = new CMItem();
			item.EntityName = "MDS_File";
			item.SetAttributeValue("MDS_Name", "Item List 1");
			item.SetAttributeValue("MDS_Status", 5);

			// Create the template request
			// Specifies the template, controller and the output filename
			TemplateRequest templateRequest = new TemplateRequest.TemplateRequestBuilder(template)
			.TemplateController(templateController, TemplateRequest.TemplateRequestBuilder.ControllerType.Xml)
			.Item(item)
			.Filename("ItemList.pdf")
			.Build();

			// Populate the template, add it to IDM and then delete it
			CMItem newItem = Template.PopulateTemplateAddItem(conn, templateRequest);
			newItem.Delete(conn);

			Console.WriteLine("Item was successfully added and then deleted in IDM.");
		}
	}
}
