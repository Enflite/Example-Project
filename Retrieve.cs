using Infor.DocumentManagement.ICP;
using System;
using System.Collections.Generic;
using System.IO;

namespace Example
{
	class Retrieve
	{
		static void Main(string[] args)
		{
			try
			{
				// Create and connect the connection
				Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
				conn.Connect();

				// Find the first item in the specified document type (entity)
				CMItem item = CMItem.Search(conn, new SearchQueries(new SearchQuery("MDS_File")));

				// Print all the regular attributes
				Console.WriteLine("Attributes\n===========");
				foreach (CMAttribute attr in item.Attributes.Values)
				{
					Console.WriteLine(attr.Name + " = " + attr.Value);
				}

				// Print all the multivalue attributes
				Console.WriteLine("\nMultivalue Attributes\n===========");
				foreach (KeyValuePair<string, CMCollections> entry in item.AllCollections)
				{
					var values = new List<object>();
					foreach (CMCollection collection in entry.Value)
					{
						foreach (CMAttribute attr in collection.Attributes.Values)
						{
							values.Add(attr.Value);
						}
					}
					Console.WriteLine(entry.Key + " = " + String.Join(", ", values));
				}

				// Print all the resource information
				Console.WriteLine("\nResources\n===========");
				foreach (CMResource res in item.Resources.Values)
				{
					Console.WriteLine("\"" + res.Name + "\", " + res.MimeType + ", " + res.Size + ", " + res.Filename);
				}

				// Save all the resources to disk
				foreach (CMResource res in item.Resources.Values)
				{
					Stream outputStream = new FileStream("..\\..\\output\\" + res.Filename, FileMode.Create);
					CMResourceData resData = item.RetrieveResourceData(conn, res.Name);
					CMResource.StreamData(resData.Stream, outputStream, true);
				}
				Console.WriteLine("\n" + item.Resources.Count + " resources saved to folder");

				conn.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
