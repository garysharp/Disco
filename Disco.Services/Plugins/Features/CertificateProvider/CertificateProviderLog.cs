using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace Disco.Services.Plugins.Features.CertificateProvider
{
	public class CertificateProvidersLog : LogBase
	{
		public enum EventTypeIds
		{
			RetrievalStarting = 10,
			RetrievalProgress,
			RetrievalFinished,
			RetrievalWarning = 15,
			RetrievalError,
			RetrievalCertificateStarting = 20,
			RetrievalCertificateFinished = 22,
			RetrievalCertificateWarning = 25,
			RetrievalCertificateError,
			Allocated = 40,
			AllocationFailed = 50
		}
		private const int _ModuleId = 60;
		private static bool _IsCertificateRetrievalProcessing;
		private static string _CertificateRetrievalStatus;
		private static int _CertificateRetrievalProgress;
		public static CertificateProvidersLog Current
		{
			get
			{
				return (CertificateProvidersLog)LogContext.LogModules[60];
			}
		}
		public static bool IsCertificateRetrievalProcessing
		{
			get
			{
				return CertificateProvidersLog._IsCertificateRetrievalProcessing;
			}
		}
		public override string ModuleDescription
		{
			get
			{
				return "Certificate Providers";
			}
		}
		public override int ModuleId
		{
			get
			{
				return 60;
			}
		}
		public override string ModuleName
		{
			get
			{
				return "CertificateProviders";
			}
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public CertificateProvidersLog()
		{
		}
		private static void Log(CertificateProvidersLog.EventTypeIds EventTypeId, params object[] Args)
		{
			CertificateProvidersLog.Current.Log((int)EventTypeId, Args);
		}
		public static void LogRetrievalStarting(int CertificateCount, int CertificateIdFrom, int CertificateIdTo)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalStarting, new object[]
			{
				CertificateCount, 
				CertificateIdFrom, 
				CertificateIdTo
			});
		}
		public static void LogRetrievalFinished()
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalFinished, new object[0]);
		}
		public static void LogRetrievalWarning(string Message)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalWarning, new object[]
			{
				Message
			});
		}
		public static void LogRetrievalError(string Message)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalError, new object[]
			{
				Message
			});
		}
		public static void LogRetrievalCertificateStarting(string CertificateId)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalCertificateStarting, new object[]
			{
				CertificateId
			});
		}
		public static void LogRetrievalCertificateFinished(string CertificateId)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalCertificateFinished, new object[]
			{
				CertificateId
			});
		}
		public static void LogRetrievalCertificateWarning(string CertificateId, string Message)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalCertificateWarning, new object[]
			{
				CertificateId, 
				Message
			});
		}
		public static void LogRetrievalCertificateError(string CertificateId, string Message)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalCertificateError, new object[]
			{
				CertificateId, 
				Message
			});
		}
		public static void LogAllocated(string CertificateId, string DeviceSerialNumber)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.Allocated, new object[]
			{
				CertificateId, 
				DeviceSerialNumber
			});
		}
		public static void LogAllocationFailed(string DeviceSerialNumber)
		{
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.AllocationFailed, new object[]
			{
				DeviceSerialNumber
			});
		}
		public static void LogCertificateRetrievalProgress(bool? IsProcessing, int? Progress, string Status)
		{
			bool flag = IsProcessing.HasValue;
			if (flag)
			{
				CertificateProvidersLog._IsCertificateRetrievalProcessing = IsProcessing.Value;
			}
			flag = CertificateProvidersLog._IsCertificateRetrievalProcessing;
			if (flag)
			{
				bool flag2 = Status != null;
				if (flag2)
				{
					CertificateProvidersLog._CertificateRetrievalStatus = Status;
				}
				flag2 = Progress.HasValue;
				if (flag2)
				{
					CertificateProvidersLog._CertificateRetrievalProgress = Progress.Value;
				}
			}
			else
			{
				CertificateProvidersLog._CertificateRetrievalStatus = null;
				CertificateProvidersLog._CertificateRetrievalProgress = 0;
			}
			CertificateProvidersLog.Log(CertificateProvidersLog.EventTypeIds.RetrievalProgress, new object[]
			{
				CertificateProvidersLog._IsCertificateRetrievalProcessing, 
				CertificateProvidersLog._CertificateRetrievalProgress, 
				CertificateProvidersLog._CertificateRetrievalStatus
			});
		}
		protected override System.Collections.Generic.List<LogEventType> LoadEventTypes()
		{
			return new System.Collections.Generic.List<LogEventType>
			{
				new LogEventType
				{
					Id = 10, 
					ModuleId = 60, 
					Name = "Retrieval Starting", 
					Format = "Starting retrieval of {0} certificate/s ({1} to {2})", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 11, 
					ModuleId = 60, 
					Name = "Retrieval Progress", 
					Format = "Processing: {0}; {1}% Complete; Status: {2}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 12, 
					ModuleId = 60, 
					Name = "Retrieval Finished", 
					Format = "Retrieval of Certificates Complete", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 15, 
					ModuleId = 60, 
					Name = "Retrieval Warning", 
					Format = "Retrieval Warning: {0}", 
					Severity = 1, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 16, 
					ModuleId = 60, 
					Name = "Retrieval Error", 
					Format = "Retrieval Error: {0}", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 20, 
					ModuleId = 60, 
					Name = "Retrieval Certificate Starting", 
					Format = "Retrieving Certificate: {0}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 22, 
					ModuleId = 60, 
					Name = "Retrieval Certificate Finished", 
					Format = "Certificate Retrieved: {0}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 25, 
					ModuleId = 60, 
					Name = "Retrieval Certificate Warning", 
					Format = "{0} Certificate Warning: {1}", 
					Severity = 1, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 26, 
					ModuleId = 60, 
					Name = "Retrieval Certificate Error", 
					Format = "{0} Certificate Error: {1}", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 40, 
					ModuleId = 60, 
					Name = "Allocated", 
					Format = "Certificate {0} allocated to {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 50, 
					ModuleId = 60, 
					Name = "Allocation Failed", 
					Format = "No certificates available for Device: {0}", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}
			};
		}
	}
}
