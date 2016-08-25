using Disco.Data.Repository;
using Exceptionless;
using Quartz;
using Quartz.Impl.Triggers;
using System;
using System.IO;

namespace Disco.Services.Documents.AttachmentImport
{
    [PersistJobDataAfterExecution]
    public class ImporterJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var sessionId = context.JobDetail.JobDataMap["SessionId"] as string;
            if (sessionId == null)
            {
                sessionId = Guid.NewGuid().ToString();
                context.JobDetail.JobDataMap["SessionId"] = sessionId;
            }

            var filename = context.JobDetail.JobDataMap["Filename"] as string;
            var retryCount = (int)context.JobDetail.JobDataMap["RetryCount"];

            using (DiscoDataContext database = new DiscoDataContext())
            {
                try
                {
                    DocumentsLog.LogImportStarting(sessionId, Path.GetFileName(filename));

                    // Returns null if unrecoverable error (eg. Not matched document)
                    Importer.Import(database, sessionId, filename);

                    // Success - Delete File
                    if (File.Exists(filename))
                        File.Delete(filename);

                    DocumentsLog.LogImportFinished(sessionId);

                    // All Done - Delete job
                    context.Scheduler.DeleteJob(context.JobDetail.Key);
                }
                catch (FileNotFoundException)
                {
                    // File not found - Delete job and don't reschedule
                    context.Scheduler.DeleteJob(context.JobDetail.Key);
                    DocumentsLog.LogImportFinished(sessionId);
                    return;
                }
                catch (Exception ex)
                {
                    ex.ToExceptionless().Submit();

                    // Retry 18 times (for 3 minutes)
                    if (retryCount < 18)
                    {
                        context.JobDetail.JobDataMap["RetryCount"] = ++retryCount;
                        DocumentsLog.LogImportWarning(sessionId, string.Format("{0}; Will try again in 10 Seconds", ex.Message));
                        // Reschedule Job for 10 seconds
                        var trig = new SimpleTriggerImpl(Guid.NewGuid().ToString(), new DateTimeOffset(DateTime.Now.AddSeconds(10)));
                        context.Scheduler.RescheduleJob(context.Trigger.Key, trig);
                    }
                    else
                    {
                        // To Many Errors
                        DocumentsLog.LogImportError(sessionId, $"To many errors occurred trying to import (SessionId: {sessionId})");
                        // Move to Errors Folder
                        if (File.Exists(filename))
                        {
                            try
                            {
                                var folderError = DataStore.CreateLocation(database, "DocumentDropBox_Errors");
                                var filenameError = Path.Combine(folderError, Path.GetFileName(filename));
                                var filenameErrorCount = 0;
                                while (File.Exists(filenameError))
                                {
                                    filenameError = Path.Combine(folderError, $"{Path.GetFileNameWithoutExtension(filename)} ({++filenameErrorCount}){Path.GetExtension(filename)}");
                                }
                                File.Move(filename, filenameError);
                            }
                            catch
                            {
                                // Ignore Errors
                            }
                        }
                        DocumentsLog.LogImportFinished(sessionId);
                    }
                }
            }
        }
    }
}
