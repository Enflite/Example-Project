using Infor.DocumentManagement.ICP;
using System;
using System.IO;

namespace Example
{
	class Add
	{
		static void Main(string[] args)
		{
			try
			{
				// Create and connect the connection
				Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
				conn.Connect();

				// Create a new item
				CMItem item = new CMItem();
				item.EntityName = "MDS_File";

				// Then add two attributes, here we show two different methods of doing this
				item.Attributes.Add(new CMAttribute("MDS_Name", "The item name"));
				item.SetAttributeValue("MDS_Status", 20);

				// Creates multi value attribute object
				/* Not enabled by default on MDS_File example document type
				CMCollections colls = new CMCollections("multi");
				foreach (object value in new List<object> { "a", "b" })
				{
					CMCollection coll = new CMCollection("multi");
					CMAttribute attr = new CMAttribute("Value", value, DataType.String);
					coll.Attributes.Add(attr);
					colls.Add(coll);
				}
				item.AddCollections(colls);
				*/

				// Add a file, if a stream is already available then this method can be used to create the byte array: CMResource.CreateByteArray(...)
				byte[] byteArr = File.ReadAllBytes("..\\..\\input\\Penguins.jpg");
				long sizeMb = byteArr.LongLength / 1024 / 1024;
				if (sizeMb < Convert.ToInt64(conn.Properties["MaxFileSize"]))
				{
					// Simple case when file is smaller than size limit
					CMResource res = new CMResource("Penguins.jpg", byteArr);
					item.Resources.Add(res);
					item.Add(conn);
				}
				else
				{
					// For large file we need to work in 2 steps - first add file without resource, then update resource
					item.Add(conn);
					item.UpdateResource(conn, true, true, "Penguins.jpg", new FileStream("..\\..\\input\\Penguins.jpg", FileMode.Open));
				}

				// Print the item pid that the item has been updated with
				Console.WriteLine("Item was added successfully.");
				Console.WriteLine("Pid: " + item.Pid);
				Console.WriteLine("Main resource url: " + item.Resources.Get(CMResource.Main).Url);

				// Now that we know that it worked we will remove the item so we don't add a lot of unnecessary items
				item.Delete(conn);
				Console.WriteLine("Item was successfully deleted.");

				conn.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
