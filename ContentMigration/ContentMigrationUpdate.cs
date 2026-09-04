using Infor.DocumentManagement.ICP;
using System;
using System.IO;


namespace Example
{
    class ContentMigrationUpdate
    {
        private const string TENANTID = "tenantId";

        static void Main(string[] args)
        {
            try
            {
                // Create and connect the connection
                Connection conn = new Connection("https://<server>:<port>/ca/", "<client id>", "<client secret>", AuthenticationMode.Token);
                conn.Connect();

                // Create a new item
                CMItem item = new CMItem
                {
                    EntityName = "MDS_File"
                };

                // Then add two attributes, here we show two different methods of doing this
                item.Attributes.Add(new CMAttribute("MDS_Name", "The item name"));
                item.SetAttributeValue("MDS_Status", 20);

                // Add a file
                byte[] byteArr = File.ReadAllBytes("input\\Penguins.jpg");
                CMResource res = new CMResource("Penguins.jpg", byteArr);
                item.Resources.Add(res);

                // Add the item and print the item PID
                ContentMigration contentMigration = new ContentMigration();
                CMItem storedItem = contentMigration.AddItem(conn, TENANTID, item, false);

                Console.WriteLine("Item was added successfully.");
                Console.WriteLine("Pid: " + storedItem.Pid);
                Console.WriteLine("Main resource url: " + storedItem.Resources[CMResource.Main].Url);

                // Update the item
                storedItem.SetAttributeValue("MDS_Name", "UPDATED");
                CMItem updatedItem = contentMigration.UpdateItem(conn, TENANTID, storedItem, storedItem.Pid, true, true, false);
                Console.WriteLine("Pid: " + updatedItem.Pid);

                conn.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
