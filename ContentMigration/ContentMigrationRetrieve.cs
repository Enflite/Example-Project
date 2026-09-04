using System;
using System.Collections.Generic;
using System.IO;
using Infor.DocumentManagement.ICP;

namespace Example
{
    class ContentMigrationRetrieve
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
                CMItems items = contentMigration.Search(conn, TENANTID, "<xQuery>", CMItems.SearchRetrieveFirstIndex, 10, false, SearchState.Active);
                List<string> resourceNames = new List<string>();

                foreach (CMItem item in items)
                {
                    // Save all the resources to disk
                    // TODO: Update the Output path below to specify where you want to store the files
                    FileStream fos = new FileStream("Output\\" + item.Filename, FileMode.Create);
                    CMResourceData resData = contentMigration.GetResourceStream(conn, TENANTID, item.Pid);
                    CMResource.StreamData(resData.Stream, fos, true);
                    resourceNames.Add(item.Filename);
                }
                Console.WriteLine($" {resourceNames.Count} Files stored in output folder. FileNames: {resourceNames}");
                conn.Disconnect();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
