using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace Disco.BI.DocumentTemplateBI.Importer
{
	public class DocumentImporterLog : LogBase
	{
		public enum EventTypeIds
		{
			ImportStarting = 10,
			ImportProgress,
			ImportFinished,
			ImportWarning = 15,
			ImportError,
			ImportPageStarting = 100,
			ImportPageImageUpdate = 104,
			ImportPageProgress,
			ImportPageDetected = 110,
			ImportPageUndetected = 115,
			ImportPageError = 120,
			ImportPageUndetectedStored = 150
		}

		private const int _ModuleId = 40;
		
        public static DocumentImporterLog Current
		{
			get
			{
				return (DocumentImporterLog)LogContext.LogModules[_ModuleId];
			}
		}
		
        public override string ModuleDescription
		{
			get
			{
				return "Document Importer";
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
				return "DocumentImporter";
			}
		}
		private static void Log(DocumentImporterLog.EventTypeIds EventTypeId, params object[] Args)
		{
			DocumentImporterLog.Current.Log((int)EventTypeId, Args);
		}
		public static void LogImportStarting(string SessionId, string DocumentName)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportStarting, new object[]
			{
				SessionId, 
				DocumentName
			});
		}
		public static void LogImportProgress(string SessionId, int? Progress, string Status)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportProgress, new object[]
			{
				SessionId, 
				Progress, 
				Status
			});
		}
		public static void LogImportFinished(string SessionId)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportFinished, new object[]
			{
				SessionId
			});
		}
		public static void LogImportWarning(string SessionId, string Message)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportWarning, new object[]
			{
				SessionId, 
				Message
			});
		}
		public static void LogImportError(string SessionId, string Message)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportError, new object[]
			{
				SessionId, 
				Message
			});
		}
		public static void LogImportPageStarting(string SessionId, int PageNumber)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageStarting, new object[]
			{
				SessionId, 
				PageNumber
			});
		}
		public static void LogImportPageImageUpdate(string SessionId, int PageNumber)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageImageUpdate, new object[]
			{
				SessionId, 
				PageNumber
			});
		}
		public static void LogImportPageProgress(string SessionId, int PageNumber, int? Progress, string Status)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageProgress, new object[]
			{
				SessionId, 
				PageNumber, 
				Progress, 
				Status
			});
		}
		public static void LogImportPageDetected(string SessionId, int PageNumber, string DocumentTypeId, string DocumentTypeName, string TargetType, string AssignedId, string AssignedName)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageDetected, new object[]
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
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageUndetected, new object[]
			{
				SessionId, 
				PageNumber
			});
		}
		public static void LogImportPageError(string SessionId, int PageNumber, string Message)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageError, new object[]
			{
				SessionId, 
				PageNumber, 
				Message
			});
		}
		public static void LogImportPageUndetectedStored(string SessionId, int PageNumber)
		{
			DocumentImporterLog.Log(DocumentImporterLog.EventTypeIds.ImportPageUndetectedStored, new object[]
			{
				SessionId, 
				PageNumber
			});
		}
		protected override System.Collections.Generic.List<LogEventType> LoadEventTypes()
		{
			return new System.Collections.Generic.List<LogEventType>
			{
				new LogEventType
				{
					Id = 10, 
					ModuleId = 40, 
					Name = "Import Starting", 
					Format = "Starting import of document: {1} (SessionId: {0})", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 11, 
					ModuleId = 40, 
					Name = "Import Progress", 
					Format = "Processing: {1}% Complete; Status: {2}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 12, 
					ModuleId = 40, 
					Name = "Import Finished", 
					Format = "Import of document complete (SessionId: {0})", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 15, 
					ModuleId = 40, 
					Name = "Import Warning", 
					Format = "Import Warning: {1} (SessionId: {0})", 
					Severity = 1, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 16, 
					ModuleId = 40, 
					Name = "Import Error", 
					Format = "Import Error: {1} (SessionId: {0})", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 100, 
					ModuleId = 40, 
					Name = "Import Page Starting", 
					Format = "Starting import of page: {1} (SessionId: {0})", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 104, 
					ModuleId = 40, 
					Name = "Import Page Image Update", 
					Format = null, 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 105, 
					ModuleId = 40, 
					Name = "Import Page Progress", 
					Format = "Processing: Page {1}; {2}% Complete; Status: {3}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 110, 
					ModuleId = 40, 
					Name = "Import Page Assigned", 
					Format = "Page {1} of type '{3}' assigned to {4}: '{6}'", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 115, 
					ModuleId = 40, 
					Name = "Import Page Undetected", 
					Format = "Page {1} not detected", 
					Severity = 1, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 120, 
					ModuleId = 40, 
					Name = "Import Page Error", 
					Format = "Page {1}, Import Error: {2}", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 150, 
					ModuleId = 40, 
					Name = "Import Page Undetected Stored", 
					Format = null, 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}
			};
		}
	}
}
