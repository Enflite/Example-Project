using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infor.DocumentManagement.ICP;

namespace Example.PrivateSpace
{
	class UpdatePrivateSpaceDocument
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

				CMItem item = new CMItem();
				item.Pid2 = "ICR_PrivateSpaceName_EntityName-D3E8C39F-C23D-4AFE-BD0E-5B5850E3E1T9";

				CMCollections collections1 = new CMCollections();
				collections1.Name = "MultiValueAttribute1";

				CMCollection collection1 = new CMCollection();
				collection1.EntityName = "MultiValueAttribute1";
				collection1.SetAttributeValue("MultiValueAttribute1", "value3", DataType.String);
				CMCollection collection2 = new CMCollection();
				collection2.EntityName = "MultiValueAttribute1";
				collection2.SetAttributeValue("MultiValueAttribute1", "value4", DataType.String);

				collections1.Add(collection1);
				collections1.Add(collection2);

				item.AddCollections(collections1);
				item.Update(conn, true, true);

				Console.WriteLine("Update item was successful.");

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
