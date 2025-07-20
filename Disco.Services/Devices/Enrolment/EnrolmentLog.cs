using Disco.Models.ClientServices;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.Enrolment
{
    public class EnrolmentLog : LogBase
    {
        public enum EventTypeIds
        {
            SessionStarting = 10,
            SessionProgress,
            SessionDevice,
            SessionDeviceInfo,
            SessionPending,
            SessionPendingApproved,
            SessionPendingRejected,
            SessionContinuing,
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
            SessionTaskProvisioningCertificate = 62,
            SessionTaskProvisioningWirelessProfile = 63,
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
        private static void Log(EventTypeIds EventTypeId, params object[] Args)
        {
            EnrolmentLog.Current.Log((int)EventTypeId, Args);
        }
        public static void LogSessionStarting(string SessionId, string HostId, EnrolmentTypes EnrolmentType)
        {
            Log(EventTypeIds.SessionStarting, new object[]
            {
                SessionId,
                HostId,
                System.Enum.GetName(EnrolmentType.GetType(), EnrolmentType)
            });
        }
        public static void LogSessionPending(string SessionId, string HostId, EnrolmentTypes EnrolmentType, string Reason, string Identifier, int? DeviceProfileId, int? DeviceBatchId)
        {
            Log(EventTypeIds.SessionPending, new object[]
            {
                SessionId,
                HostId,
                System.Enum.GetName(EnrolmentType.GetType(), EnrolmentType),
                Reason,
                Identifier,
                DeviceProfileId,
                DeviceBatchId,
            });
        }
        public static void LogSessionPendingApproved(string SessionId, string Username, string Reason)
        {
            Log(EventTypeIds.SessionPendingApproved, new object[]
            {
                SessionId,
                Username,
                Reason
            });
        }
        public static void LogSessionPendingRejected(string SessionId, string Username, string Reason)
        {
            Log(EventTypeIds.SessionPendingRejected, new object[]
            {
                SessionId,
                Username,
                Reason
            });
        }
        public static void LogSessionContinuing(string SessionId, string HostId, EnrolmentTypes EnrolmentType)
        {
            Log(EventTypeIds.SessionContinuing, new object[]
            {
                SessionId,
                HostId,
                System.Enum.GetName(EnrolmentType.GetType(), EnrolmentType)
            });
        }
        public static void LogSessionDevice(string SessionId, string DeviceSerialNumber, int? DeviceModelId)
        {
            Log(EventTypeIds.SessionDevice, new object[]
            {
                SessionId,
                DeviceSerialNumber,
                DeviceModelId
            });
        }
        public static void LogSessionDeviceInfo(string SessionId, string SerialNumber, string UUID, string ComputerName, string LanMacAddress, string WlanMacAddress, string Manufacturer, string Model, string ModelType)
        {
            Log(EventTypeIds.SessionDeviceInfo, new object[]
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
        public static void LogSessionDeviceInfo(string SessionId, Enrol Request)
        {
            EnrolmentLog.LogSessionDeviceInfo(
                SessionId,
                Request.SerialNumber,
                Request.Hardware.UUID,
                Request.ComputerName,
                Request.Hardware?.NetworkAdapters?
                    .Where(a => !a.IsWlanAdapter)
                    .Select(a => a.MACAddress)
                    .Aggregate((string)null, (s, m) => $"{s}{m};")?.TrimEnd(';') ?? null,
                Request.Hardware?.NetworkAdapters?
                    .Where(a => a.IsWlanAdapter)
                    .Select(a => a.MACAddress)
                    .Aggregate((string)null, (s, m) => $"{s}{m};")?.TrimEnd(';') ?? null,
                Request.Hardware.Manufacturer,
                Request.Hardware.Model,
                Request.Hardware.ModelType);
        }
        
        public static void LogSessionProgress(string SessionId, int Progress, string Status)
        {
            Log(EventTypeIds.SessionProgress, new object[]
            {
                SessionId,
                Progress,
                Status
            });
        }
        public static void LogSessionFinished(string SessionId)
        {
            Log(EventTypeIds.SessionFinished, new object[]
            {
                SessionId
            });
        }
        public static void LogSessionDiagnosticInformation(string SessionId, string Message)
        {
            Log(EventTypeIds.SessionDiagnosticInformation, new object[]
            {
                SessionId,
                Message
            });
        }
        public static void LogSessionWarning(string SessionId, string Message)
        {
            Log(EventTypeIds.SessionWarning, new object[]
            {
                SessionId,
                Message
            });
        }
        public static void LogSessionError(string SessionId, System.Exception Ex)
        {
            if (Ex.InnerException == null)
            {
                Log(EventTypeIds.SessionError, new object[]
                {
                    SessionId,
                    Ex.GetType().Name,
                    Ex.Message,
                    Ex.StackTrace
                });
            }
            else
            {
                Log(EventTypeIds.SessionErrorWithInner, new object[]
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
            Log(EventTypeIds.SessionClientError, new object[]
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
            Log(EventTypeIds.SessionTaskAddedDevice, new object[]
            {
                SessionId,
                DeviceSerialNumber
            });
        }
        public static void LogSessionTaskUpdatingDevice(string SessionId, string DeviceSerialNumber)
        {
            Log(EventTypeIds.SessionTaskUpdatingDevice, new object[]
            {
                SessionId,
                DeviceSerialNumber
            });
        }
        public static void LogSessionTaskCreatedDeviceModel(string SessionId, string DeviceSerialNumber, string Manufacturer, string Model)
        {
            Log(EventTypeIds.SessionTaskCreatedDeviceModel, new object[]
            {
                SessionId,
                DeviceSerialNumber,
                Manufacturer,
                Model
            });
        }
        public static void LogSessionTaskProvisioningADAccount(string SessionId, string DeviceSerialNumber, string ADAccountName)
        {
            Log(EventTypeIds.SessionTaskProvisioningADAccount, new object[]
            {
                SessionId,
                DeviceSerialNumber,
                ADAccountName
            });
        }
        public static void LogSessionTaskAssigningUser(string SessionId, string DeviceSerialNumber, string UserDisplayName, string UserUsername, string UserDomain, string UserSID)
        {
            Log(EventTypeIds.SessionTaskAssigningUser, new object[]
            {
                SessionId,
                DeviceSerialNumber,
                UserDisplayName,
                UserUsername,
                UserDomain,
                UserSID
            });
        }
        public static void LogSessionTaskProvisioningCertificate(string SessionId, string DeviceSerialNumber, string CertificateName)
        {
            Log(EventTypeIds.SessionTaskProvisioningCertificate, new object[]
            {
                SessionId,
                DeviceSerialNumber,
                CertificateName
            });
        }
        public static void LogSessionTaskProvisioningWirelessProfile(string SessionId, string DeviceSerialNumber, string WirelessProfileName)
        {
            Log(EventTypeIds.SessionTaskProvisioningWirelessProfile, new object[]
            {
                SessionId,
                DeviceSerialNumber,
                WirelessProfileName
            });
        }
        public static void LogSessionTaskRenamingDevice(string SessionId, string OldComputerName, string NewComputerName)
        {
            Log(EventTypeIds.SessionTaskRenamingDevice, new object[]
            {
                SessionId,
                OldComputerName,
                NewComputerName
            });
        }
        public static void LogSessionTaskMovingDeviceOrganisationUnit(string SessionId, string OldOrganisationUnit, string NewOrganisationUnit)
        {
            Log(EventTypeIds.SessionTaskMovingDeviceOrganisationUnit, new object[]
            {
                SessionId,
                OldOrganisationUnit,
                NewOrganisationUnit
            });
        }
        public static void LogClientError(string ClientIP, string ClientIdentifier, string ClientVersion, string Error, string RawError)
        {
            Log(EventTypeIds.ClientError, new object[]
            {
                ClientIP,
                ClientIdentifier,
                ClientVersion,
                Error,
                RawError
            });
        }
        protected override List<LogEventType> LoadEventTypes()
        {
            return new List<LogEventType>
            {
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionStarting,
                    ModuleId = _ModuleId,
                    Name = "Session Starting",
                    Format = "Starting '{2}' Enrolment for {1} (Session# {0})",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionProgress,
                    ModuleId = _ModuleId,
                    Name = "Session Progress",
                    Format = "Processing Session# {0}; {1}% Complete; Status: {2}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = false,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionDevice,
                    ModuleId = _ModuleId,
                    Name = "Session Device",
                    Format = null,
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionDeviceInfo,
                    ModuleId = _ModuleId,
                    Name = "Session Device Info",
                    Format = null,
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionPending,
                    ModuleId = _ModuleId,
                    Name = "Session Pending",
                    Format = "Pending '{2}' Enrolment for {1} (Session# {0}; Reason: {3}; Identifier: {4})",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionPendingApproved,
                    ModuleId = _ModuleId,
                    Name = "Session Pending Approved",
                    Format = "Pending enrolment approved by {1} (Session# {0}; Reason: {2})",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionPendingRejected,
                    ModuleId = _ModuleId,
                    Name = "Session Pending Rejected",
                    Format = "Pending enrolment rejected by {1} (Session# {0}; Reason: {2})",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionContinuing,
                    ModuleId = _ModuleId,
                    Name = "Session Continuing",
                    Format = "Continuing '{2}' Enrolment for {1} (Session# {0})",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionFinished,
                    ModuleId = _ModuleId,
                    Name = "Session Finished",
                    Format = "Finished Session# {0}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionDiagnosticInformation,
                    ModuleId = _ModuleId,
                    Name = "Session Diagnostic Information",
                    Format = null,
                    Severity = 0,
                    UseLive = true,
                    UsePersist = false,
                    UseDisplay = false
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionWarning,
                    ModuleId = _ModuleId,
                    Name = "Session Warning",
                    Format = null,
                    Severity = 1,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionError,
                    ModuleId = _ModuleId,
                    Name = "Session Error",
                    Format = "An Error Occurred: [{1}] {2}",
                    Severity = 2,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionErrorWithInner,
                    ModuleId = _ModuleId,
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
                    Id = (int)EventTypeIds.SessionTaskAddedDevice,
                    ModuleId = _ModuleId,
                    Name = "Task - Added Device",
                    Format = "Creating Disco ICT Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskUpdatingDevice,
                    ModuleId = _ModuleId,
                    Name = "Task - Updating Device",
                    Format = "Updating Disco ICT Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskCreatedDeviceModel,
                    ModuleId = _ModuleId,
                    Name = "Task - Creating Device Model",
                    Format = "Creating Device Model '{2} {3}' for Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskProvisioningADAccount,
                    ModuleId = _ModuleId,
                    Name = "Task - Provisioning Active Directory Account",
                    Format = "Provisioning Active Directory Account '{2}' for Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskAssigningUser,
                    ModuleId = _ModuleId,
                    Name = "Task - Assigning User",
                    Format = "Assigning User '{2}' ({4}\\{3} {{{5}}}) for Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskProvisioningCertificate,
                    ModuleId = _ModuleId,
                    Name = "Task - Provisioning Certificate",
                    Format = "Provisioning Certificate '{2}' for Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskProvisioningWirelessProfile,
                    ModuleId = _ModuleId,
                    Name = "Task - Provisioning Wireless Profile",
                    Format = "Provisioning Wireless Profile '{2}' for Device {1}",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskRenamingDevice,
                    ModuleId = _ModuleId,
                    Name = "Task - Renaming Device",
                    Format = "Renaming Device '{1}' to '{2}'",
                    Severity = 0,
                    UseLive = true,
                    UsePersist = true,
                    UseDisplay = true
                },
                new LogEventType
                {
                    Id = (int)EventTypeIds.SessionTaskMovingDeviceOrganisationUnit,
                    ModuleId = _ModuleId,
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
