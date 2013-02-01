using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using Disco.Models.ClientServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace Disco.BI.DeviceBI
{
    public class EnrolmentLog : LogBase
    {
        public enum EventTypeIds
        {
            SessionStarting = 10,
            SessionProgress,
            SessionDevice,
            SessionDeviceInfo,
            SessionFinished = 20,
            SessionDiagnosticInformation,
            SessionWarning,
            SessionError,
            SessionErrorWithInner,
            SessionClientError,
            SessionTaskAddedDevice = 50,
            SessionTaskUpdatingDevice,
            SessionTaskCreatedDeviceModel = 56,
            SessionTaskProvisioningADAccount = 58,
            SessionTaskAssigningUser = 60,
            SessionTaskProvisioningWirelessCertificate = 62,
            SessionTaskRenamingDevice = 64,
            SessionTaskMovingDeviceOrganisationUnit = 66,
            ClientError = 400
        }
        private const int _ModuleId = 50;
        public static EnrolmentLog Current
        {
            get
            {
                return (EnrolmentLog)LogContext.LogModules[50];
            }
        }
        public override string ModuleDescription
        {
            get
            {
                return "Device Enrolment";
            }
        }
        public override int ModuleId
        {
            get
            {
                return 50;
            }
        }
        public override string ModuleName
        {
            get
            {
                return "DeviceEnrolment";
            }
        }
        [System.Diagnostics.DebuggerNonUserCode]
        public EnrolmentLog()
        {
        }
        private static void Log(EnrolmentLog.EventTypeIds EventTypeId, params object[] Args)
        {
            EnrolmentLog.Current.Log((int)EventTypeId, Args);
        }
        public static void LogSessionStarting(string SessionId, string HostId, DeviceEnrol.EnrolmentTypes EnrolmentType)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionStarting, new object[]
			{
				SessionId, 
				HostId, 
				System.Enum.GetName(EnrolmentType.GetType(), EnrolmentType)
			});
        }
        public static void LogSessionDevice(string SessionId, string DeviceSerialNumber, int? DeviceModelId)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionDevice, new object[]
			{
				SessionId, 
				DeviceSerialNumber, 
				DeviceModelId
			});
        }
        public static void LogSessionDeviceInfo(string SessionId, string SerialNumber, string UUID, string ComputerName, string LanMacAddress, string WlanMacAddress, string Manufacturer, string Model, string ModelType)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionDeviceInfo, new object[]
			{
				SessionId, 
				SerialNumber, 
				UUID, 
				ComputerName, 
				LanMacAddress, 
				WlanMacAddress, 
				Manufacturer, 
				Model, 
				ModelType
			});
        }
        public static void LogSessionDeviceInfo(string SessionId, MacEnrol Request)
        {
            EnrolmentLog.LogSessionDeviceInfo(SessionId, Request.DeviceSerialNumber, Request.DeviceUUID, Request.DeviceComputerName, Request.DeviceLanMacAddress, Request.DeviceWlanMacAddress, Request.DeviceManufacturer, Request.DeviceModel, Request.DeviceModelType);
        }
        public static void LogSessionDeviceInfo(string SessionId, Models.ClientServices.Enrol Request)
        {
            EnrolmentLog.LogSessionDeviceInfo(SessionId, Request.DeviceSerialNumber, Request.DeviceUUID, Request.DeviceComputerName, Request.DeviceLanMacAddress, Request.DeviceWlanMacAddress, Request.DeviceManufacturer, Request.DeviceModel, Request.DeviceModelType);
        }
        public static void LogSessionProgress(string SessionId, int Progress, string Status)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionProgress, new object[]
			{
				SessionId, 
				Progress, 
				Status
			});
        }
        public static void LogSessionFinished(string SessionId)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionFinished, new object[]
			{
				SessionId
			});
        }
        public static void LogSessionDiagnosticInformation(string SessionId, string Message)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionDiagnosticInformation, new object[]
			{
				SessionId, 
				Message
			});
        }
        public static void LogSessionWarning(string SessionId, string Message)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionWarning, new object[]
			{
				SessionId, 
				Message
			});
        }
        public static void LogSessionError(string SessionId, System.Exception Ex)
        {
            if (Ex.InnerException == null)
            {
                EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionError, new object[]
			    {
				    SessionId, 
				    Ex.GetType().Name, 
				    Ex.Message,
                    Ex.StackTrace
			    });
            }
            else
            {
                EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionErrorWithInner, new object[]
			    {
				    SessionId, 
				    Ex.GetType().Name, 
				    Ex.Message,
                    Ex.InnerException.GetType().Name,
                    Ex.InnerException.Message,
                    Ex.StackTrace,
                    Ex.InnerException.StackTrace
			    });
            }
        }
        public static void LogSessionClientError(string SessionId, string ClientIP, string ClientIdentifier, string ClientVersion, string Error, string RawError)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionClientError, new object[]
			{
				SessionId,
                ClientIP, 
				ClientIdentifier, 
                ClientVersion,
				Error,
                RawError
			});
        }
        public static void LogSessionTaskAddedDevice(string SessionId, string DeviceSerialNumber)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskAddedDevice, new object[]
			{
				SessionId, 
				DeviceSerialNumber
			});
        }
        public static void LogSessionTaskUpdatingDevice(string SessionId, string DeviceSerialNumber)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskUpdatingDevice, new object[]
			{
				SessionId, 
				DeviceSerialNumber
			});
        }
        public static void LogSessionTaskCreatedDeviceModel(string SessionId, string DeviceSerialNumber, string Manufacturer, string Model)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskCreatedDeviceModel, new object[]
			{
				SessionId, 
				DeviceSerialNumber, 
				Manufacturer, 
				Model
			});
        }
        public static void LogSessionTaskProvisioningADAccount(string SessionId, string DeviceSerialNumber, string ADAccountName)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskProvisioningADAccount, new object[]
			{
				SessionId, 
				DeviceSerialNumber, 
				ADAccountName
			});
        }
        public static void LogSessionTaskAssigningUser(string SessionId, string DeviceSerialNumber, string UserDisplayName, string UserUsername, string UserDomain, string UserSID)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskAssigningUser, new object[]
			{
				SessionId, 
				DeviceSerialNumber, 
				UserDisplayName, 
				UserUsername, 
				UserDomain, 
				UserSID
			});
        }
        public static void LogSessionTaskProvisioningWirelessCertificate(string SessionId, string DeviceSerialNumber, string CertificateName)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskProvisioningWirelessCertificate, new object[]
			{
				SessionId, 
				DeviceSerialNumber, 
				CertificateName
			});
        }
        public static void LogSessionTaskRenamingDevice(string SessionId, string OldComputerName, string NewComputerName)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskRenamingDevice, new object[]
			{
				SessionId, 
				OldComputerName, 
				NewComputerName
			});
        }
        public static void LogSessionTaskMovingDeviceOrganisationUnit(string SessionId, string OldOrganisationUnit, string NewOrganisationUnit)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.SessionTaskMovingDeviceOrganisationUnit, new object[]
			{
				SessionId, 
				OldOrganisationUnit, 
				NewOrganisationUnit
			});
        }
        public static void LogClientError(string ClientIP, string ClientIdentifier, string ClientVersion, string Error, string RawError)
        {
            EnrolmentLog.Log(EnrolmentLog.EventTypeIds.ClientError, new object[]
			{
				ClientIP, 
				ClientIdentifier, 
                ClientVersion,
				Error,
                RawError
			});
        }
        protected override System.Collections.Generic.List<LogEventType> LoadEventTypes()
        {
            return new System.Collections.Generic.List<LogEventType>
			{
				new LogEventType
				{
					Id = 10, 
					ModuleId = 50, 
					Name = "Session Starting", 
					Format = "Starting '{2}' Enrolment for {1} (Session# {0})", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 12, 
					ModuleId = 50, 
					Name = "Session Device", 
					Format = null, 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 11, 
					ModuleId = 50, 
					Name = "Session Progress", 
					Format = "Processing Session# {0}; {1}% Complete; Status: {2}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 13, 
					ModuleId = 50, 
					Name = "Session Device Info", 
					Format = null, 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 20, 
					ModuleId = 50, 
					Name = "Session Finished", 
					Format = "Finished Session# {0}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 21, 
					ModuleId = 50, 
					Name = "Session Diagnostic Information", 
					Format = null, 
					Severity = 0, 
					UseLive = true, 
					UsePersist = false, 
					UseDisplay = false
				}, 
				new LogEventType
				{
					Id = 22, 
					ModuleId = 50, 
					Name = "Session Warning", 
					Format = null, 
					Severity = 1, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 23, 
					ModuleId = 50, 
					Name = "Session Error", 
					Format = "An Error Occurred: [{1}] {2}", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
                new LogEventType
				{
					Id = 24, 
					ModuleId = 50, 
					Name = "Session Error with Internal", 
					Format = "An Error Occurred: [{1}] {2}; Internal Error: [{3}] {4}", 
					Severity = 2, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				},  
                new LogEventType
				{
					Id = (int)EventTypeIds.SessionClientError, 
					ModuleId = _ModuleId, 
					Name = "Client Error", 
					Format = "IP: {1}; Device ID: {2}; Version: {3} Error: {4}; Session# {0}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 50, 
					ModuleId = 50, 
					Name = "Task - Added Device", 
					Format = "Creating Disco Device {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 51, 
					ModuleId = 50, 
					Name = "Task - Updating Device", 
					Format = "Updating Disco Device {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 56, 
					ModuleId = 50, 
					Name = "Task - Creating Device Model", 
					Format = "Creating Device Model '{2} {3}' for Device {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 58, 
					ModuleId = 50, 
					Name = "Task - Provisioning Active Directory Account", 
					Format = "Provisioning Active Directory Account '{2}' for Device {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 60, 
					ModuleId = 50, 
					Name = "Task - Assigning User", 
					Format = "Assigning User '{2}' ({4}\\{3} {{{5}}}) for Device {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 62, 
					ModuleId = 50, 
					Name = "Task - Provisioning Wireless Certificate", 
					Format = "Provisioning Wireless Certificate '{2}' for Device {1}", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 64, 
					ModuleId = 50, 
					Name = "Task - Renaming Device", 
					Format = "Renaming Device '{1}' to '{2}'", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = 66, 
					ModuleId = 50, 
					Name = "Task - Moving Device Organisation Unit", 
					Format = "Moving Device Organisation Unit '{1}' to '{2}'", 
					Severity = 0, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.ClientError, 
					ModuleId = _ModuleId, 
					Name = "Client Error", 
					Format = "IP: {0}; Device ID: {1}; Version: {2} Error: {3}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}
			};
        }
    }
}
