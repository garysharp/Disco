using System;
using System.IO;
using System.Web.Caching;
using Disco.Data.Repository;
using Quartz;
using Quartz.Impl.Triggers;

namespace Disco.BI.DocumentTemplateBI.Importer
{
    [PersistJobDataAfterExecution]
    public class DocumentImporterJob : IJob
    {

        void IJob.Execute(IJobExecutionContext context)
        {
            string sessionId = context.JobDetail.JobDataMap["SessionId"] as string;
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                context.JobDetail.JobDataMap["SessionId"] = sessionId;
            }

            string filename = context.JobDetail.JobDataMap["Filename"] as string;
            int retryCount = (int)context.JobDetail.JobDataMap["RetryCount"];
            Cache httpCache = context.JobDetail.JobDataMap["HttpCache"] as Cache;

            var friendlyFilename = filename;
            if (!string.IsNullOrEmpty(friendlyFilename))
                friendlyFilename = System.IO.Path.GetFileName(friendlyFilename);

            DocumentsLog.LogImportStarting(sessionId, friendlyFilename);

            if (!File.Exists(filename))
            {
                DocumentsLog.LogImportWarning(sessionId, string.Format("File not found: {0}", filename));
                DocumentsLog.LogImportFinished(sessionId);
                context.Scheduler.DeleteJob(context.JobDetail.Key);
                return;
            }

            try
            {
                using (DiscoDataContext database = new DiscoDataContext())
                {
                    if (retryCount < 18)
                    {
                        context.JobDetail.JobDataMap["RetryCount"] = (++retryCount);
                        bool processResult = Interop.Pdf.PdfImporter.ProcessPdfAttachment(filename, database, sessionId, httpCache);

                        if (processResult)
                        {
                            // Import Successful - Delete
                            if (File.Exists(filename))
                                File.Delete(filename);
                        }
                        else
                        {
                            // Import Failed - Move to Errors Folder
                            if (File.Exists(filename))
                            {
                                try
                                {
                                    string folderError = DataStore.CreateLocation(database, "DocumentDropBox_Errors");
                                    string filenameError = Path.Combine(folderError, Path.GetFileName(filename));
                                    int filenameErrorCount = 0;
                                    while (File.Exists(filenameError))
                                    {
                                        filenameError = Path.Combine(folderError, string.Format("{0} ({1}){2}", Path.GetFileNameWithoutExtension(filename), ++filenameErrorCount, Path.GetExtension(filename)));
                                    }
                                    File.Move(filename, filenameError);
                                }
                                catch
                                {
                                    // Ignore Errors
                                }
                            }
                        }
                    }
                    else
                    {
                        // To Many Errors
                        DocumentsLog.LogImportError(sessionId, string.Format("To many errors occurred trying to import '{1}' (SessionId: {0})", sessionId, friendlyFilename));
                        // Move to Errors Folder
                        if (File.Exists(filename))
                        {
                            try
                            {
                                string folderError = DataStore.CreateLocation(database, "DocumentDropBox_Errors");
                                string filenameError = Path.Combine(folderError, Path.GetFileName(filename));
                                int filenameErrorCount = 0;
                                while (File.Exists(filenameError))
                                {
                                    filenameError = Path.Combine(folderError, string.Format("{0} ({1}){2}", Path.GetFileNameWithoutExtension(filename), ++filenameErrorCount, Path.GetExtension(filename)));
                                }
                                File.Move(filename, filenameError);
                            }
                            catch
                            {
                                // Ignore Errors
                            }
                        }
                    }
                }
                DocumentsLog.LogImportFinished(sessionId);

                // All Done
                context.Scheduler.DeleteJob(context.JobDetail.Key);
            }
            catch (Exception ex)
            {
                DocumentsLog.LogImportWarning(sessionId, string.Format("{0}; Will try again in 10 Seconds", ex.Message));
                // Reschedule Job for 10 seconds
                SimpleTriggerImpl trig = new SimpleTriggerImpl(Guid.NewGuid().ToString(), new DateTimeOffset(DateTime.Now.AddSeconds(10)));
                context.Scheduler.RescheduleJob(context.Trigger.Key, trig);
            }

        }

    }
}
