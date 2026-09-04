using Infor.DocumentManagement.ICP;
using System;
using System.IO;

namespace Example
{
	class Batch
	{
		static void Main(string[] args)
		{
			try
			{
				// Create and connect the connection
				Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
				conn.Connect();

				// Create two identical items with empty files, we use a Guid so we can find the items easily when we do the search
				string guid = "{D3E8C39F-C23D-4AFE-BD0E-5B5850E3E1A2}";
				CMItem item = new CMItem();
				item.EntityName = "MDS_File";
				item.Attributes.Add(new CMAttribute("MDS_Name", guid, DataType.String));
				item.Resources.Add(new CMResource("empty.txt", new byte[0]));
				CMItem item2 = new CMItem(item);

				// Create a search query that finds the two added files
				SearchQueries queries = new SearchQueries();
				SearchQuery query = new SearchQuery("MDS_File");
				query.Arguments.Add(new SearchArgument("MDS_Name", SearchArgument.SearchOpEqual, guid, DataType.String));
				queries.Add(query);

				// Then we execute our first batch call which is adding the two items and then searching for them.
				// All the operations will be executed synchronously so the adding will be done before the search.
				BatchOperations operations = new BatchOperations();
				operations.AddItem(item, false).AddItem(item2, false);
				operations.SearchItems(queries, CMItems.SearchRetrieveFirstIndex, CMItems.SearchRetrieveAllResults, false).SearchCount(queries);
				bool successful = operations.Execute(conn, true, false);

				// Since we used "false" as the last argument to execute(...) we need to verify if all the operations were successful
				CMItems searchedItems = null;
				if (successful)
				{
					Console.WriteLine("Items were added successfully.");
					// Get the search result, the operations are listed in the same order as they were added
					searchedItems = (CMItems)operations[2].Output;
					Console.WriteLine("Number of items found: " + operations[3].Output);
				}
				else
				{
					Console.WriteLine("One or more errors occurred.");
					// Find all the errors and print them
					foreach (BatchOperation operation in operations)
					{
						if (!operation.IsSuccessful)
						{
							Console.WriteLine(operation.Exception);
						}
					}
					return;	// Abort
				}

				// In the last batch call we will do a couple of operations and then delete the items.
				BatchOperations operations2 = new BatchOperations();
				operations2.CheckOutItems(searchedItems);
				operations2.UpdateItem(searchedItems[0], false, false);	// We only update one item and with the exact same data
				operations2.RetrieveItems(searchedItems);
				operations2.CheckInItems(searchedItems);
				operations2.DeleteItems(searchedItems);
				operations2.Execute(conn, false, true);	// We execute this without any transaction and we throw on first error

				// If we get here without any exception being thrown then everything worked
				Console.WriteLine("All operations were successful.");

				conn.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
