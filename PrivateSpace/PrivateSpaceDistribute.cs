using ICP.Standard.Signature;
using ICP.Standard.Signature.V1;
using Infor.DocumentManagement.ICP;
using Infor.DocumentManagement.ICP.Distribution.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
	class PrivateSpaceDistribute
	{
		static void Main(string[] args)
		{
			String printerId = "00569c65-91fc-4a5f-a82f-2cc144ca6dd4";


            string currentTenant = "";
			string currentUser = "";

			string baseUrl = "https://mingle-ci-alb.mingle.inforos.dev.inforcloudsuite.com/authorizationserver/";
			string username = "";
			string password = "";

			Connection conn = new Connection(baseUrl, username, password, AuthenticationMode.Token);
			conn.Connect(false);
			conn.isPrivateSpace = true;
			conn.Tenant = currentTenant;
			conn.Username = currentUser;
            SubmitExample(printerId, currentTenant, currentUser, conn);
		}

		private static void SubmitExample(string printerId, string currentTenant, string currentUser, Connection conn)
		{

			SubmitJob job = CreateSubmitJob("HCM Jobs", conn);
			addPrint(job, printerId);


			SubmitResult status = null;
			try
			{
                List<PrinterResponse> printerResponse = Distribution.getAvailablePrinters(conn, "IEP");
                status = Distribution.Distribute(conn, job);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return;
			}

			if (!status.Success)
			{
				Console.WriteLine("Could not Submit job. Error: " + status.ErrorMessage);
				return;
			}

			StatusResult res = null;
			for (int i = 0; i < 20; i++)
			{
				res = Distribution.Status(conn, status.JobId);
                BatchStatusResult res2 = Distribution.BatchStatus(conn, "HCM%20Jobs", false);
                if (res.Status == StatusEnum.pending || res.Status == StatusEnum.inProgress)
				{
					Console.WriteLine("Job not completed, sleep for 2 sec and ask again.");
					Thread.Sleep(2000);
				}
				else
				{
					break;
				}
			}

			if (res.Status == StatusEnum.pending)
			{
				Console.WriteLine("Job is still not done. Investigate why!");
				return;
			}
			else if (res.Status == StatusEnum.ok)
			{
				Console.WriteLine("Job has finished. Exection time (ms) = " + res.TimeToComplete + ". Job submitted at (UTC) "
						+ res.TimeFromSubmit);
				foreach (TargetStatus target in res.Targets)
				{
					if (target.Type == TargetType.print)
					{
						Console.WriteLine("Printed a document (use separate API to query print job). Print ID = " + target.Id);
					}
				}
			}
			else if (res.Status == StatusEnum.fail)
			{
				if (res.Phase == PhaseEnum.validate)
				{
					Console.WriteLine("Job failed during validation. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.retrieveIdmDocuments)
				{
					Console.WriteLine("Job failed when retrived documents from IDM. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.generate)
				{
					Console.WriteLine("Job failed when generating documents. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.assemble)
				{
					Console.WriteLine("Job failed when assembling documents. Job aborted. Error: " + res.ErrorMessage);
				}
				else if (res.Phase == PhaseEnum.distribute)
				{
					Console.WriteLine("One or more of the targets could not be fulfilled.");
					foreach (TargetStatus target in res.Targets)
					{
						if (res.Status == StatusEnum.fail)
						{
							if (target.Type == TargetType.email)
							{
								Console.WriteLine("Failed to send mail. Error: " + target.Description);
							}
							else if (target.Type == TargetType.item)
							{
								Console.WriteLine("Failed to save to IDM. Error: " + target.Description);
							}
							else if (target.Type == TargetType.print)
							{
								Console.WriteLine("Failed to print. Error: " + target.Description);
							}
						}
						else
						{
							if (target.Type == TargetType.print)
							{
								Console.WriteLine("Printed a document (use separate API to query print job). Print ID = " + target.Id);
							}
							else if (target.Type == TargetType.email)
							{
								Console.WriteLine("E-mail: " + target.Description);
							}
							

						}
					}
				}
			}
		}

		private static SubmitJob CreateSubmitJob(string batchId, Connection conn)
		{
			SubmitJob job = CreateGenerateJob();
			job.BatchId = batchId;

			addEmail(job, "Test@infor.com", "Test@infor.com", "Order confirmation", "Your order have been placed.\n See attached details.\n\n Best Regards \nInfor");
			AddStoreToIdm(job);

			
			return job;
		}

        public static SubmitJob addEmail(SubmitJob job, String to, String from, String subject, String body)
        {
            EmailTarget email = new EmailTarget(to, subject, body);
            email.From = from;

            // Extra attachment to the e-mail
            email.AddAttachement(new ItemPid2File("ICR_HCM_SubmitJob-fc0e0ad8-fca6-40bf-a18a-7548bbc43f46"));

            job.Targets.Add(email);
            return job;
        }

        public static SubmitJob addPrint(SubmitJob job, String printerId)
		{
			PrintTarget print = new PrintTarget(printerId);
			job.Targets.Add(print);
			return job;
		}

    private static SubmitJob CreateGenerateJob()
		{
			SubmitJob job = new SubmitJob();

            ItemPid2File templateLayout = new ItemPid2File("ICR_HCM_SubmitJob-f8688ccf-f213-4e80-ac7a-733848d0639d");
			ItemPid2File xmlLayout = new ItemPid2File("ICR_HCM_SubmitJob-ce3e4208-f0d1-40da-b768-73a2fd7a8d2c");
            
			// The file to be generated
			GenerateFile generate = new GenerateFile("generated_PS_file.pdf", xmlLayout, templateLayout);
			job.Input.Add(generate);

			return job;
		}

		private static SubmitJob AddStoreToIdm(SubmitJob job)
		{

			CMItem item = new CMItem();
			item.EntityName = "ICR_HCM_SubmitJob";
            item.SetAttributeValue("MDS_Name", "HCM Job Update", DataType.String);

            ItemTarget target = new ItemTarget();
			target.SetItemDataFile(item);

            UpdateVersion uv = new UpdateVersion("MDS_ID", "f74f5ecd-3c7d-4405-b048-453e0d07f24b", true);
            target.SetUpdateVersion(uv);

            job.Targets.Add(target);
			return job;
		}
	}
}
