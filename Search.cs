using Infor.DocumentManagement.ICP;
using System;

namespace Example
{
	class Search
	{
		static void Main(string[] args)
		{
			try
			{
                // Create and connect the connection
                Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
                conn.Connect();

				// Execute an XQuery search and print the display name for all items 
				CMItems items = CMItems.Search(conn, "/MDS_File[@MDS_Name IS NOT NULL]", CMItems.SearchRetrieveFirstIndex, 10);
				foreach (CMItem item in items)
				{
					object displayName = item.DisplayName;
					Console.WriteLine(displayName != null ? displayName : "<no name>");
				}

				// It is also possible to use a SearchQueries object to execute a search, here we do a count on the same query as we used before
				SearchQueries queries = new SearchQueries();
				SearchQuery query = new SearchQuery("MDS_File");
				query.Arguments.Add(new SearchArgument("MDS_Name", SearchArgument.SearchOpIsNotNull));
				queries.Add(query);
				long count = CMItems.SearchCount(conn, queries);
				Console.WriteLine("===========\nShowing " + items.Count + " of " + count + " items");

				conn.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
