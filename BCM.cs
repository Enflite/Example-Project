using Infor.DocumentManagement.ICP.BusinessContext;
using Infor.DocumentManagement.ICP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Policy;

namespace Example
{
    class BCM
    {
        static void Main(string[] args)
        {
            // Create and connect the connection
            Connection conn = new Connection("https://<server>:<port>/ca/", "<username>", "<password>", AuthenticationMode.Basic);
            conn.Connect();
            try
            {
                // Fetch all business context models for the tenant
                BusinessContextModels models = BusinessContextModels.GetBusinessContextModels(conn);
                Console.WriteLine("Fetched Business Context Models:");

                // Iterate over  each business context models
                foreach (KeyValuePair<string, BusinessContextModel> entry in models)
                {
                    BusinessContextModel model = entry.Value;
                    Console.WriteLine($"Name: {model.Name}");
                    Console.WriteLine($"namespace: {model.Namespace}");
                    Console.WriteLine($"Editable: {model.IsEditable}");
                    Console.WriteLine($"XQuery: {model.XQuery}");
                    Console.WriteLine($"Additional Query: {model.XQuery}");
                    Console.WriteLine($"Access Control List: {model.AccessControlList}");
                    Console.WriteLine($"Additional Mappings: {model.AdditionalMappings}");
                    Console.WriteLine($"Search Queries: {(model.SearchQueries != null ? string.Join(", ", model.SearchQueries) : "None")}");
                    Console.WriteLine($"Entity Name: {model.Name}");
                    Console.WriteLine($"Product ID: {model.GetProductId()}");
                    Console.WriteLine($"Screen ID: {model.GetScreenId()}");
                    Console.WriteLine($"Doc Type: {model.GetDocType()}");
                }
            }
            catch (CMException exception)
            {
                Console.WriteLine($"An error occurred: {exception.Message}");
                Console.WriteLine(exception.StackTrace);
            }
        }
    }
}
