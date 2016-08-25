using Disco.Models.Repository;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;

namespace Disco.Services.Documents
{
    public class DocumentsLog : LogBase
    {

        public enum EventTypeIds
        {
            ImportStarting = 10,
            ImportProgress,
            ImportFinished,
            ImportWarning = 15,
            ImportError,
            ImportAttachmentExpressionEvaluated = 50,
            ImportPageStarting = 100,
            ImportPageImageUpdate = 104,
            ImportPageProgress,
            ImportPageDetected = 110,
            ImportPageUndetected = 115,
            ImportPageError = 120,
            ImportPageUndetectedStored = 150,
            DocumentGenerated = 500,
            DocumentGeneratedWithExpression
        }

        private const int _ModuleId = 40;

        public static DocumentsLog Current
        {
            get
            {
                return (DocumentsLog)LogContext.LogModules[_ModuleId];
            }
        }

        public override string ModuleDescription
        {
            get
            {
                return "Documents";
            }
        }
        public override int ModuleId
        {
            get
            {
                return _ModuleId;
            }
        }
        public override string ModuleName
        {
            get
            {
                return "Documents";
            }
        }
        private static void Log(DocumentsLog.EventTypeIds EventTypeId, params object[] Args)
        {
            DocumentsLog.Current.Log((int)EventTypeId, Args);
        }
        public static void LogImportStarting(string SessionId, string DocumentName)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportStarting, new object[]
            {
                SessionId,
                DocumentName
            });
        }
        public static void LogImportProgress(string SessionId, int? Progress, string Status)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportProgress, new object[]
            {
                SessionId,
                Progress,
                Status
            });
        }
        public static void LogImportFinished(string SessionId)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportFinished, new object[]
            {
                SessionId
            });
        }
        public static void LogImportWarning(string SessionId, string Message)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportWarning, new object[]
            {
                SessionId,
                Message
            });
        }
        public static void LogImportError(string SessionId, string Message)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportError, new object[]
            {
                SessionId,
                Message
            });
        }
        public static void LogImportAttachmentExpressionEvaluated(DocumentTemplate template, IAttachmentTarget target, IAttachment attachment, string Result)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportAttachmentExpressionEvaluated, new object[]
            {
                template.Id,
                target.AttachmentReferenceId,
                attachment.Id,
                Result
            });
        }
        public static void LogImportPageStarting(string SessionId, int PageNumber)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageStarting, new object[]
            {
                SessionId,
                PageNumber
            });
        }
        public static void LogImportPageImageUpdate(string SessionId, int PageNumber)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageImageUpdate, new object[]
            {
                SessionId,
                PageNumber
            });
        }
        public static void LogImportPageProgress(string SessionId, int PageNumber, int? Progress, string Status)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageProgress, new object[]
            {
                SessionId,
                PageNumber,
                Progress,
                Status
            });
        }
        public static void LogImportPageDetected(string SessionId, int PageNumber, string DocumentTypeId, string DocumentTypeName, string TargetType, string AssignedId, string AssignedName)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageDetected, new object[]
            {
                SessionId,
                PageNumber,
                DocumentTypeId,
                DocumentTypeName,
                TargetType,
                AssignedId,
                AssignedName
            });
        }
        public static void LogImportPageUndetected(string SessionId, int PageNumber)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageUndetected, new object[]
            {
                SessionId,
                PageNumber
            });
        }
        public static void LogImportPageError(string SessionId, int PageNumber, string Message)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageError, new object[]
            {
                SessionId,
                PageNumber,
                Message
            });
        }
        public static void LogImportPageUndetectedStored(string SessionId, int PageNumber)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.ImportPageUndetectedStored, new object[]
            {
                SessionId,
                PageNumber
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, Device Device, User Author, string ExpressionResult)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGeneratedWithExpression, new object[]
            {
                Template.Id,
                Device.SerialNumber,
                Author.UserId,
                ExpressionResult
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, Job Job, User Author, string ExpressionResult)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGeneratedWithExpression, new object[]
            {
                Template.Id,
                Job.Id,
                Author.UserId,
                ExpressionResult
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, User User, User Author, string ExpressionResult)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGeneratedWithExpression, new object[]
            {
                Template.Id,
                User.UserId,
                Author.UserId,
                ExpressionResult
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, IAttachmentTarget Data, User Author, string ExpressionResult)
        {
            if (Data is Job)
                LogDocumentGenerated(Template, (Job)Data, Author, ExpressionResult);
            else if (Data is User)
                LogDocumentGenerated(Template, (User)Data, Author, ExpressionResult);
            else if (Data is Device)
                LogDocumentGenerated(Template, (Device)Data, Author, ExpressionResult);
            else
                DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGeneratedWithExpression, new object[]
                {
                    Template.Id,
                    "UNKNOWN",
                    Author.UserId,
                    ExpressionResult
                });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, Device Device, User Author)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGenerated, new object[]
            {
                Template.Id,
                Device.SerialNumber,
                Author.UserId
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, Job Job, User Author)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGenerated, new object[]
            {
                Template.Id,
                Job.Id,
                Author.UserId
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, User User, User Author)
        {
            DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGenerated, new object[]
            {
                Template.Id,
                User.UserId,
                Author.UserId
            });
        }
        public static void LogDocumentGenerated(DocumentTemplate Template, object Data, User Author)
        {
            if (Data is Job)
                LogDocumentGenerated(Template, (Job)Data, Author);
            else if (Data is User)
                LogDocumentGenerated(Template, (User)Data, Author);
            else if (Data is Device)
                LogDocumentGenerated(Template, (Device)Data, Author);
            else
                DocumentsLog.Log(DocumentsLog.EventTypeIds.DocumentGenerated, new object[]
                {
                    Template.Id,
                    "UNKNOWN",
                    Author.UserId
                });
        }
        protected override System.Collections.Generic.List<LogEventType> LoadEventTypes()
        {
            return new System.Collections.Generic.List<LogEventType>
            {
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportStarting,
                    ModuleId = _ModuleId,
                    Name = "Import Starting",
                    Format = "Starting import of document: {1} (SessionId: {0})",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportProgress,
                    ModuleId = _ModuleId,
                    Name = "Import Progress",
                    Format = "Processing: {1}% Complete; Status: {2}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = false,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportFinished,
                    ModuleId = _ModuleId,
                    Name = "Import Finished",
                    Format = "Import of document complete (SessionId: {0})",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportWarning,
                    ModuleId = _ModuleId,
                    Name = "Import Warning",
                    Format = "Import Warning: {1} (SessionId: {0})",
                    Severity = (int)LogEventType.Severities.Warning,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportError,
                    ModuleId = _ModuleId,
                    Name = "Import Error",
                    Format = "Import Error: {1} (SessionId: {0})",
                    Severity = (int)LogEventType.Severities.Error,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportAttachmentExpressionEvaluated,
                    ModuleId = _ModuleId,
                    Name = "Import Attachment Expression Evaluated",
                    Format = "The import attachment expression for '{0}' was evaluated for '{1}' (attachment id: {2}) with the result: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageStarting,
                    ModuleId = _ModuleId,
                    Name = "Import Page Starting",
                    Format = "Starting import of page: {1} (SessionId: {0})",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageImageUpdate,
                    ModuleId = _ModuleId,
                    Name = "Import Page Image Update",
                    Format = null,
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = false,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageProgress,
                    ModuleId = _ModuleId,
                    Name = "Import Page Progress",
                    Format = "Processing: Page {1}; {2}% Complete; Status: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = false,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageDetected,
                    ModuleId = _ModuleId,
                    Name = "Import Page Assigned",
                    Format = "Page {1} of type '{3}' assigned to {4}: '{6}'",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageUndetected,
                    ModuleId = _ModuleId,
                    Name = "Import Page Undetected",
                    Format = "Page {1} not detected",
                    Severity = (int)LogEventType.Severities.Warning,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageError,
                    ModuleId = _ModuleId,
                    Name = "Import Page Error",
                    Format = "Page {1}, Import Error: {2}",
                    Severity = (int)LogEventType.Severities.Error,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.ImportPageUndetectedStored,
                    ModuleId = _ModuleId,
                    Name = "Import Page Undetected Stored",
                    Format = null,
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = false,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.DocumentGenerated,
                    ModuleId = _ModuleId,
                    Name = "Document Generated",
                    Format = "A '{0}' document was generated for '{1}' by '{2}'",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.DocumentGeneratedWithExpression,
                    ModuleId = _ModuleId,
                    Name = "Document Generated with Expression",
                    Format = "A '{0}' document was generated for '{1}' by '{2}'. The expression returned: {3}",
                    Severity = (int)LogEventType.Severities.Information,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                }
            };
        }

    }
}
