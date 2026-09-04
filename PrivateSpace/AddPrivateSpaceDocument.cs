using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infor.DocumentManagement.ICP;

namespace Example.PrivateSpace
{
    class AddPrivateSpaceDocument
    {
		static void Main(string[] args)
		{
			try
			{
				// Create and connect the connection
				Connection conn = new Connection("https://<server>:<port>/ca/", "<client id>", "<client secret>",
					AuthenticationMode.Token);
				conn.Connect();
				conn.Tenant = "<tenant>";
				conn.Username = "<IFS username (UPN or Guid)>";
				conn.isPrivateSpace = true;

				// Create a new item
				CMItem item = new CMItem();
				item.EntityName = "ICR_PrivateSpaceName_EntityName";

				// Then add two single value attributes, here we show two different methods of doing this
				item.Attributes.Add(new CMAttribute("Number", 8, DataType.Short));
				item.SetAttributeValue("Number", 8, DataType.Short);

				//Adding 2 Multi valued attributes
				//First Attribute
				CMCollections collections1 = new CMCollections();
				collections1.Name = "MultiValueAttribute1";

				CMCollection collection1 = new CMCollection();
				collection1.EntityName = "MultiValueAttribute1";
				collection1.SetAttributeValue("MultiValueAttribute1", "value1", DataType.String);
				CMCollection collection2 = new CMCollection();
				collection2.EntityName = "MultiValueAttribute1";
				collection2.SetAttributeValue("MultiValueAttribute1", "value2", DataType.String);

				collections1.Add(collection1);
				collections1.Add(collection2);

				//Second Attribute
				CMCollections collections2 = new CMCollections();
				collections2.Name = "MultiValueAttribute2";

				CMCollection collection3 = new CMCollection();
				collection3.EntityName = "MultiValueAttribute2";
				collection3.SetAttributeValue("MultiValueAttribute2", 1, DataType.Short);
				CMCollection collection4 = new CMCollection();
				collection4.EntityName = "MultiValueAttribute2";
				collection4.SetAttributeValue("MultiValueAttribute2", 2, DataType.Short);

				collections2.Add(collection3);
				collections2.Add(collection4);

				// Adding both multivalue attributes to CMItem
				item.AddCollections(collections1);
				item.AddCollections(collections2);


				// Add a file, if a stream is already available then this method can be used to create the byte array: CMResource.CreateByteArray(...)
				byte[] byteArr = File.ReadAllBytes("..\\..\\input\\Penguins.jpg");
				CMResource res = new CMResource("Penguins.jpg", byteArr);
				item.Resources.Add(res);

				// Add the item and print the item pid that the item has been updated with
				item.Add(conn);
				Console.WriteLine("Item was added successfully.");
				Console.WriteLine("Pid: " + item.Pid);
				Console.WriteLine("Pid2: " + item.Pid2);
				Console.WriteLine("Main resource url: " + item.Resources.Get(CMResource.Main).Url);

				// Now that we know that it worked we will remove the item so we don't add a lot of unnecessary items
				item.Delete(conn);
				Console.WriteLine("Item was successfully deleted.");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
