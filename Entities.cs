using Infor.DocumentManagement.ICP;
using System;

namespace Example
{
	class Entities
	{
		static void Main(string[] args)
		{
			try
			{
				// Create and connect the connection
				Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
				conn.Connect();

				// Retrieve all the user entities and print information about them
				CMEntities entities = CMEntities.GetEntities(conn);
				foreach (CMEntity entity in entities.Values)
				{
					Console.WriteLine(entity.Name + " - " + entity.Description);
					Console.WriteLine("\t" + "IsTextSearchable: " + entity.IsTextSearchable);
					Console.WriteLine("\t" + "Number of attributes: " + entity.Attributes.Count);
					Console.WriteLine("\t" + "Number of child entities: " + (entity.ChildEntities != null ? entity.ChildEntities.Count : 0));
					Console.WriteLine("\t" + "Number of acls: " + entity.AccessControlLists.Count);
					Console.WriteLine("\t" + "Represents item: " + (entity.RepresentsItemName != null ? entity.RepresentsItemName : null));
					Console.WriteLine("\t" + "Number of templates: " + (entity.IsTemplateEnabled ? entity.ListTemplates(conn).Count.ToString() : "Not supported"));
				}

				conn.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
