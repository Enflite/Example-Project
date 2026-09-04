using System;
using Infor.DocumentManagement.ICP;

namespace Example
{
    class ContentMigrationSearch
    {
        private const string TENANTID = "tenantId";

        static void Main(string[] args)
        {
            try
            {
                // Create and connect the connection
                Connection conn = new Connection("https://<server>:<port>/ca/", "<client id>", "<client secret>", AuthenticationMode.Token);
                conn.Connect();

                ContentMigration contentMigration = new ContentMigration();
                CMItems items = contentMigration.Search(conn, TENANTID, "<xQuery>", CMItems.SearchRetrieveFirstIndex, 10, true, SearchState.Active);
                Console.WriteLine(items);

                foreach (CMItem item in items)
                {
                    object displayName = item.Filename;
                    Console.WriteLine(displayName != null ? displayName : "<no name>");
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
