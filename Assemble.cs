using Infor.DocumentManagement.ICP;
using Infor.DocumentManagement.ICP.Distribution.V1;
using System;
using System.IO;
using System.Threading;

namespace Example
{
    /// <summary>
    /// This example demonstrates document assembly using the Submit Job API.
    /// 
    /// The assembled document is stored temporarily and downloaded, without permanent storage in IDM.
    /// Temporary files are automatically deleted after 24 hours.
    /// </summary>
    class Assemble
    {
        static void Main(string[] args)
        {
            try
            {

                //Connect to IDM using OAuth2
                string ionApiPath = "path to your .ionapi file";
                Connection conn = new Connection(null, ionApiPath, null, AuthenticationMode.OAuth2);
                conn.Connect();

                // MDC traceability headers for logging correlation across services
                // CorrelationId: auto-generated with "req-" prefix if not set
                // conn.CorrelationId = "req-custom-id-here";
                // CallerId: auto-generated based on auth mode:
                //   OAuth1:  DOTNETSDK-CLIENT-{first 4 chars of ConsumerKey}
                //   OAuth2:  DOTNETSDK-CLIENT-{first 4 chars of OAuth2 ClientId}
                //   Token:   DOTNETSDK-CLIENT-{clienttype from JWT} (falls back to first 4 chars of ClientId)
                // conn.CallerId = "MyCustomApp";
                // conn.MdcUserId = "<userId>";
                // conn.MdcTenantId = "<tenantId>";

                Console.WriteLine("Connected to IDM");

                // Assemble documents
                AssembleDocuments(conn);

                conn.Disconnect();
                Console.WriteLine("\nDisconnected from IDM");
                //Console.WriteLine("DEBUG: X-Correlation-ID: " + conn.CorrelationId);
                //Console.WriteLine("DEBUG: X-Caller-ID: " + conn.CallerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n=== ERROR ===");
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("\nStack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("\n=== Press Enter to exit ===");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Assemble multiple documents into a single PDF and download it
        /// </summary>
        static void AssembleDocuments(Connection conn)
        {
            try
            {
                Console.WriteLine("\n=== Assembling Documents ===");

                // Step 1: Generate a unique identifier to match the result
                string uniqueId = Guid.NewGuid().ToString();
                string filename = "assembled-document.pdf";

                // Step 2: Create a SubmitJob
                SubmitJob job = new SubmitJob();

                // Step 3: Create AssembleFile to combine multiple documents into one PDF
                AssembleFile assembleFile = new AssembleFile();
                assembleFile.Filename = filename;
                
                // Add documents to assemble
                assembleFile.Files.Add(new ItemPidFile("PID-of-first-document"));
                assembleFile.Files.Add(new ItemPidFile("PID-of-second-document"));

                
                // Add the assemble instruction to the job
                job.Input.Add(assembleFile);

                // Step 4: Add temporary file target with unique identifier
                CMItem tempItem = new CMItem();
                tempItem.EntityName = "MDS_TemporaryFile";
                tempItem.SetAttributeValue("MDS_Name", filename);
                tempItem.SetAttributeValue("MDS_ID", uniqueId, DataType.Uiid); // Store GUID for matching

                ItemTarget itemTarget = new ItemTarget();
                itemTarget.SetItemDataFile(tempItem);
                job.Targets.Add(itemTarget);

                Console.WriteLine("Target: Temporary file (auto-deleted after 24 hours)");
                Console.WriteLine($"Unique ID: {uniqueId}");

                // Step 5: Submit the job
                Console.WriteLine("Submitting assembly job...");

                SubmitResult submitResult = Distribution.Distribute(conn, job);

                //// Log MDC headers after first request (correlationId is now generated)
                //Console.WriteLine("DEBUG: X-Correlation-ID: " + conn.CorrelationId);
                //Console.WriteLine("DEBUG: X-Caller-ID: " + conn.CallerId);

                if (!submitResult.Success)
                {
                    Console.WriteLine("FAILED: Could not submit job.");
                    Console.WriteLine("Error: " + submitResult.ErrorMessage);
                    return;
                }

                Console.WriteLine("SUCCESS: Job submitted with ID: " + submitResult.JobId);

                // Step 5: Poll for completion
                Console.WriteLine("Waiting for assembly to complete...");
                StatusResult status = null;
                int maxAttempts = 30;

                for (int i = 0; i < maxAttempts; i++)
                {
                    status = Distribution.Status(conn, submitResult.JobId);

                    if (status.Status == StatusEnum.pending || status.Status == StatusEnum.inProgress)
                    {
                        Console.WriteLine($"  [{i + 1}/{maxAttempts}] Status: {status.Status}");
                        Thread.Sleep(2000); // Wait 2 seconds
                    }
                    else
                    {
                        break; // Job completed
                    }
                }

                // Step 6: Handle the result
                if (status.Status == StatusEnum.ok)
                {
                    Console.WriteLine("\n=== ASSEMBLY SUCCESSFUL ===");
                    Console.WriteLine($"Execution time: {status.TimeToComplete} ms");

                    // RECOMMENDED APPROACH: Search for the assembled file by GUID
                    Console.WriteLine($"\nSearching for assembled file with GUID: {uniqueId}");

                    try
                    {
                        // Search for MDS_TemporaryFile directly by GUID in MDS_ID
                        SearchQueries queries = new SearchQueries();
                        SearchQuery query = new SearchQuery("MDS_TemporaryFile");
                        query.Arguments.Add(new SearchArgument("MDS_ID", SearchArgument.SearchOpEqual, uniqueId));
                        queries.Add(query);

                        // Execute search
                        CMItems results = CMItems.Search(conn, queries, CMItems.SearchRetrieveFirstIndex, 10);

                        Console.WriteLine($"Search returned {results.Count} temporary file(s) matching GUID");

                        if (results.Count > 0)
                        {
                            CMItem assembledItem = results[0];
                            string pid = assembledItem.Pid;
                            string mdsName = assembledItem.GetAttributeValue("MDS_Name") as string;

                            Console.WriteLine($"✓ Found assembled file!");
                            Console.WriteLine($"  PID: {pid}");
                            Console.WriteLine($"  MDS_Name: {mdsName}");

                            // Download the assembled document
                            DownloadDocument(conn, pid, mdsName ?? filename);
                        }
                        else
                        {
                            Console.WriteLine($"✗ No file found with the specified GUID: {uniqueId}");
                        }
                        
                        // ALTERNATIVE APPROACH: Use DocumentInformation (also works)
                        // Uncomment the code below to use DocumentInformation instead of search
                        /*
                        string tempFilePid = null;
                        foreach (TargetStatus targetStatus in status.Targets)
                        {
                            if (targetStatus.Type == TargetType.item)
                            {
                                DocumentInformation docInfo = targetStatus.GetDocumentInformation(filename);
                                if (docInfo != null)
                                {
                                    tempFilePid = docInfo.Pid;
                                    Console.WriteLine($"✓ Found assembled file via DocumentInformation!");
                                    Console.WriteLine($"  PID: {tempFilePid}");
                                    Console.WriteLine($"  Filename: {docInfo.Filename}");
                                    
                                    DownloadDocument(conn, tempFilePid, docInfo.Filename);
                                    break;
                                }
                            }
                        }
                        
                        if (tempFilePid == null)
                        {
                            Console.WriteLine($"✗ No file found in DocumentInformation");
                        }
                        */
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR searching for assembled file: {ex.Message}");
                        Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
                    }
                }
                else if (status.Status == StatusEnum.fail)
                {
                    Console.WriteLine("\n=== ASSEMBLY FAILED ===");
                    Console.WriteLine($"Failed in phase: {status.Phase}");
                    Console.WriteLine($"Error: {status.ErrorMessage}");
                }
                else
                {
                    Console.WriteLine("\n=== JOB TIMED OUT ===");
                    Console.WriteLine("The job did not complete within 60 seconds.");
                    Console.WriteLine("Check Job Management for status.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAILED: Assembly failed.");
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Download a document from IDM and save it locally
        /// </summary>
        static void DownloadDocument(Connection conn, string pid, string filename)
        {
            try
            {
                Console.WriteLine($"\nDownloading {filename}...");

                // Create CMItem with the PID and retrieve it
                CMItem item = new CMItem(pid);
                item.Retrieve(conn);

                if (item.Resources != null && item.Resources.Count > 0)
                {

                    // Get the first resource key
                    string resourceType = null;
                    foreach (var kvp in item.Resources)
                    {
                        resourceType = kvp.Key;
                        break;
                    }

                    // Retrieve the resource data
                    CMResourceData resData = item.RetrieveResourceData(conn, resourceType);

                    if (resData != null && resData.Stream != null)
                    {
                        // Save to Windows temp folder
                        string tempFolder = Path.GetTempPath();
                        string outputPath = Path.Combine(tempFolder, filename);

                        // Save the stream to file
                        using (FileStream outputStream = new FileStream(outputPath, FileMode.Create))
                        {
                            CMResource.StreamData(resData.Stream, outputStream, true);
                        }

                        Console.WriteLine($"SUCCESS: File saved to: {outputPath}");
                        
                        // Open the file automatically
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = outputPath,
                                UseShellExecute = true
                            });
                            Console.WriteLine($"✓ File opened in default application");
                        }
                        catch (Exception openEx)
                        {
                            Console.WriteLine($"WARNING: Could not auto-open file: {openEx.Message}");
                            Console.WriteLine($"Please open manually: {outputPath}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Downloaded file is empty or null");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Item has no resources");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR downloading document: {ex.Message}");
            }
        }
    }
}
