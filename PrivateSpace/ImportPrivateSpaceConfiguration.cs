using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Infor.DocumentManagement.ICP;
using Infor.DocumentManagement.ICP.Config;

namespace Example.PrivateSpace
{
    class ImportPrivateSpaceConfiguration
    {
        static void Main(string[] args)
        {
            // Create and connect the connection
            Connection conn = new Connection("https://<server>:<port>/ca/", "<client id>", "<client secret>",
                     AuthenticationMode.Token);
            conn.Connect();
            conn.Tenant = "<tenant>";
            conn.Username = "<IFS username (UPN or Guid)>";
            conn.isPrivateSpace = true;

            String path = "..\\..\\input\\PrivateSpaceConfiguration.xml";

            // Validate that the path exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The specified configuration could not be found. Path: " + path);
            }

            // Load the Xml configuration file
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            } catch
            {
                throw new XmlException("Error while parsing the configuration file");
            }
            try
            {
                Configuration.ImportConfiguration(conn, doc);
                Console.WriteLine("Import Configuration finished");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
