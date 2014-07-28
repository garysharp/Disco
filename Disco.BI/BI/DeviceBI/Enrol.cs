using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tamir.SharpSsh;
using Exceptionless;

namespace Disco.BI.DeviceBI
{
    public class DeviceEnrol
    {
        public enum EnrolmentTypes
        {
            Normal,
            Mac = 5,
            MacSecure
        }

        private static Regex SshPromptRegEx = new Regex("[\\$,\\#]", RegexOptions.Multiline);
        public static MacSecureEnrolResponse MacSecureEnrol(DiscoDataContext Database, string Host)
        {
            MacEnrol trustedRequest = new MacEnrol();
            string sessionId = System.Guid.NewGuid().ToString("B");
            MacSecureEnrolResponse MacSecureEnrol;
            try
            {
                EnrolmentLog.LogSessionStarting(sessionId, Host, EnrolmentTypes.MacSecure);
                EnrolmentLog.LogSessionProgress(sessionId, 0, string.Format("Connecting to '{0}' as '{1}'", Host, Database.DiscoConfiguration.Bootstrapper.MacSshUsername));
                SshShell shell = new SshShell(Host, Database.DiscoConfiguration.Bootstrapper.MacSshUsername, Database.DiscoConfiguration.Bootstrapper.MacSshPassword);
                try
                {
                    shell.ExpectPattern = "#";
                    shell.Connect();
                    EnrolmentLog.LogSessionProgress(sessionId, 10, "Connected, Authenticating");
                    var output = shell.Expect(SshPromptRegEx);
                    bool sessionElevated = false;
                    EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                    if (!output.TrimEnd(new char[0]).EndsWith("#"))
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 22, "Connected, Elevating Credentials");
                        shell.WriteLine("sudo -k");
                        System.Threading.Thread.Sleep(250);
                        output = shell.Expect(SshPromptRegEx);
                        EnrolmentLog.LogSessionProgress(sessionId, 25, "Connected, Elevating Credentials");
                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                        shell.WriteLine("sudo -s -S");
                        System.Threading.Thread.Sleep(250);
                        output = shell.Expect(":");
                        EnrolmentLog.LogSessionProgress(sessionId, 27, "Connected, Elevating Credentials");
                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                        shell.WriteLine(Database.DiscoConfiguration.Bootstrapper.MacSshPassword);
                        System.Threading.Thread.Sleep(250);
                        output = shell.Expect(SshPromptRegEx);
                        sessionElevated = true;
                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                    }
                    EnrolmentLog.LogSessionProgress(sessionId, 20, "Retrieving Serial Number");
                    trustedRequest.DeviceSerialNumber = ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Serial Number\" | cut -d \":\" -f 2-", sessionId);
                    EnrolmentLog.LogSessionDevice(sessionId, trustedRequest.DeviceSerialNumber, null);
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "Retrieving Hardware UUID");
                    trustedRequest.DeviceUUID = ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Hardware UUID:\" | cut -d \":\" -f 2-", sessionId);
                    EnrolmentLog.LogSessionProgress(sessionId, 40, "Retrieving Computer Name");
                    trustedRequest.DeviceComputerName = ParseMacShellCommand(shell, "scutil --get ComputerName", sessionId);
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "Retrieving Ethernet MAC Address");
                    string lanNicId = ParseMacShellCommand(shell, "system_profiler SPEthernetDataType | egrep -o \"en0|en1|en2|en3|en4|en5|en6\"", sessionId);
                    if (!string.IsNullOrWhiteSpace(lanNicId))
                    {
                        trustedRequest.DeviceLanMacAddress = ParseMacShellCommand(shell, string.Format("ifconfig {0} | grep ether | cut -d \" \" -f 2-", lanNicId), sessionId);
                    }
                    EnrolmentLog.LogSessionProgress(sessionId, 65, "Retrieving Wireless MAC Address");
                    string wlanNicId = ParseMacShellCommand(shell, "system_profiler SPAirPortDataType | egrep -o \"en0|en1|en2|en3|en4|en5|en6\"", sessionId);
                    if (!string.IsNullOrWhiteSpace(wlanNicId))
                    {
                        trustedRequest.DeviceWlanMacAddress = ParseMacShellCommand(shell, string.Format("ifconfig {0} | grep ether | cut -d \" \" -f 2-", wlanNicId), sessionId);
                    }
                    trustedRequest.DeviceManufacturer = "Apple Inc.";
                    EnrolmentLog.LogSessionProgress(sessionId, 80, "Retrieving Model");
                    trustedRequest.DeviceModel = ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Model Identifier:\" | cut -d \":\" -f 2-", sessionId);
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Retrieving Model Type");
                    trustedRequest.DeviceModelType = ParseMacModelType(ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Model Name:\" | cut -d \":\" -f 2-", sessionId));
                    EnrolmentLog.LogSessionProgress(sessionId, 99, "Disconnecting");
                    output = ParseMacModelType(ParseMacShellCommand(shell, "exit", sessionId));
                    if (sessionElevated)
                    {
                        output = ParseMacModelType(ParseMacShellCommand(shell, "exit", sessionId));
                    }
                    if (shell.Connected)
                    {
                        shell.Close();
                    }
                    EnrolmentLog.LogSessionProgress(sessionId, 100, "Disconnected, Starting Disco Enrolment");
                    MacSecureEnrolResponse response = MacSecureEnrolResponse.FromMacEnrolResponse(MacEnrol(Database, trustedRequest, true, sessionId));
                    EnrolmentLog.LogSessionFinished(sessionId);
                    MacSecureEnrol = response;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (shell != null)
                    {
                        bool connected = shell.Connected;
                        if (connected)
                        {
                            shell.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ex.ToExceptionless().Submit();
                EnrolmentLog.LogSessionError(sessionId, ex);
                throw ex;
            }
            return MacSecureEnrol;
        }

        #region "Mac Enrol Helpers"

        private static string ParseMacModelType(string ModelName)
        {
            string ParseMacModelType;
            if (!string.IsNullOrWhiteSpace(ModelName))
            {
                string mn = ModelName.ToLower();
                if (mn.Contains("imac") || mn.Contains("mini"))
                {
                    ParseMacModelType = "Desktop";
                    return ParseMacModelType;
                }
                if (mn.Contains("macbook"))
                {
                    ParseMacModelType = "Mobile";
                    return ParseMacModelType;
                }
                if (mn.Contains("xserve"))
                {
                    ParseMacModelType = "Server";
                    return ParseMacModelType;
                }
            }
            ParseMacModelType = "Unknown";
            return ParseMacModelType;
        }

        private static string ParseMacShellCommand(SshShell Shell, string Command, string LogSessionId)
        {
            Shell.WriteLine(Command);
            System.Threading.Thread.Sleep(250);
            string Response = Shell.Expect(SshPromptRegEx);
            Response = Response.Replace("\r", string.Empty);
            EnrolmentLog.LogSessionDiagnosticInformation(LogSessionId, Response);
            bool flag = Response.Contains("\n");
            string ParseMacShellCommand;
            if (flag)
            {
                string[] ResponseLines = Response.Split(new char[]
					{
						'\n'
					});
                switch (ResponseLines.Length)
                {
                    case 0:
                    case 1:
                        {
                            ParseMacShellCommand = string.Empty;
                            break;
                        }
                    case 2:
                    case 3:
                        {
                            ParseMacShellCommand = ResponseLines[1].Trim();
                            break;
                        }
                    default:
                        {
                            System.Text.StringBuilder ResponseBuilder = new System.Text.StringBuilder();
                            int num = ResponseLines.Length - 2;
                            int lineIndex = 1;
                            while (true)
                            {
                                int arg_111_0 = lineIndex;
                                int num2 = num;
                                if (arg_111_0 > num2)
                                {
                                    break;
                                }
                                ResponseBuilder.AppendLine(ResponseLines[lineIndex]);
                                lineIndex++;
                            }
                            ParseMacShellCommand = ResponseBuilder.ToString().Trim();
                            break;
                        }
                }
            }
            else
            {
                ParseMacShellCommand = Response;
            }
            return ParseMacShellCommand;
        }

        #endregion

        public static MacEnrolResponse MacEnrol(DiscoDataContext Database, MacEnrol Request, bool Trusted, string OpenSessionId = null)
        {
            string sessionId;
            if (OpenSessionId == null)
            {
                sessionId = System.Guid.NewGuid().ToString("B");
                EnrolmentLog.LogSessionStarting(sessionId, Request.DeviceSerialNumber, EnrolmentTypes.Mac);
            }
            else
            {
                sessionId = OpenSessionId;
            }
            EnrolmentLog.LogSessionDeviceInfo(sessionId, Request);
            MacEnrolResponse response = new MacEnrolResponse();
            try
            {
                EnrolmentLog.LogSessionProgress(sessionId, 10, "Querying Database");
                Device RepoDevice = Database.Devices.Include("AssignedUser").Include("DeviceProfile").Include("DeviceProfile").Where(d => d.SerialNumber == Request.DeviceSerialNumber).FirstOrDefault();
                if (!Trusted)
                {
                    if (RepoDevice == null)
                        throw new EnrolSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.DeviceSerialNumber));
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                        throw new EnrolSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.DeviceSerialNumber));
                }
                if (RepoDevice == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "New Device, Building Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.DeviceSerialNumber);
                    DeviceProfile deviceProfile = Database.DeviceProfiles.Find(Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModelType.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice = new Device
                    {
                        SerialNumber = Request.DeviceSerialNumber,
                        DeviceDomainId = Request.DeviceComputerName,
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now
                    };
                    Database.Devices.Add(RepoDevice);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, Request.DeviceSerialNumber);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModelType.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice.DeviceModel = deviceModel;

                    RepoDevice.DeviceDomainId = Request.DeviceComputerName;
                    if (!RepoDevice.EnrolledDate.HasValue)
                    {
                        RepoDevice.EnrolledDate = DateTime.Now;
                    }
                }
                RepoDevice.LastEnrolDate = DateTime.Now;
                RepoDevice.AllowUnauthenticatedEnrol = false;
                // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
                //DeviceProfileConfiguration RepoDeviceProfileContext = RepoDevice.DeviceProfile.Configuration(Context);
                EnrolmentLog.LogSessionProgress(sessionId, 90, "Building Response");
                //if (RepoDeviceProfileContext.DistributionType == DeviceProfileConfiguration.DeviceProfileDistributionTypes.OneToOne && RepoDevice.AssignedUser != null)
                if (RepoDevice.DeviceProfile.DistributionType == DeviceProfile.DistributionTypes.OneToOne && RepoDevice.AssignedUser != null)
                {
                    ADUserAccount AssignedUserInfo = ActiveDirectory.RetrieveADUserAccount(RepoDevice.AssignedUser.UserId);
                    EnrolmentLog.LogSessionTaskAssigningUser(sessionId, RepoDevice.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.SamAccountName, AssignedUserInfo.Domain.NetBiosName, AssignedUserInfo.SecurityIdentifier.ToString());
                    response.DeviceAssignedUserUsername = AssignedUserInfo.SamAccountName;
                    response.DeviceAssignedUserDomain = AssignedUserInfo.Domain.NetBiosName;
                    response.DeviceAssignedUserName = AssignedUserInfo.DisplayName;
                    response.DeviceAssignedUserSID = AssignedUserInfo.SecurityIdentifier.ToString();
                }
                response.DeviceComputerName = RepoDevice.DeviceDomainId;
                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new MacEnrolResponse { ErrorMessage = ex.Message };
            }
            catch (System.Exception ex2)
            {
                ex2.ToExceptionless().Submit();
                EnrolmentLog.LogSessionError(sessionId, ex2);
                throw ex2;
            }
            finally
            {
                if (OpenSessionId == null)
                    EnrolmentLog.LogSessionFinished(sessionId);
            }
            return response;
        }
        public static EnrolResponse Enrol(DiscoDataContext Database, string Username, Models.ClientServices.Enrol Request)
        {
            ADMachineAccount adMachineAccount = null;

            EnrolResponse response = new EnrolResponse();

            AuthorizationToken authenticatedToken = null;
            bool isAuthenticated = false;

            ADDomain domain = null;
            Lazy<ADDomainController> domainController = new Lazy<ADDomainController>(() =>
            {
                if (domain == null)
                    throw new InvalidOperationException("The [domain] variable must be initialized first");
                return domain.GetAvailableDomainController(RequireWritable: true);
            });

            string sessionId = System.Guid.NewGuid().ToString("B");
            response.SessionId = sessionId;

            EnrolmentLog.LogSessionStarting(sessionId, Request.DeviceSerialNumber, EnrolmentTypes.Normal);
            EnrolmentLog.LogSessionDeviceInfo(sessionId, Request);

            try
            {
                EnrolmentLog.LogSessionProgress(sessionId, 10, "Loading User Data");
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    authenticatedToken = UserService.GetAuthorization(Username, Database);
                    isAuthenticated = (authenticatedToken != null);
                }
                EnrolmentLog.LogSessionProgress(sessionId, 13, "Loading Device Data");

                Device RepoDevice = Database.Devices.Include("AssignedUser").Include("DeviceModel").Include("DeviceProfile").Where(d => d.SerialNumber == Request.DeviceSerialNumber).FirstOrDefault();
                EnrolmentLog.LogSessionProgress(sessionId, 15, "Discovering User/Device Disco Permissions");
                if (isAuthenticated)
                {
                    if (!authenticatedToken.Has(Claims.Device.Actions.EnrolDevices))
                    {
                        if (!authenticatedToken.Has(Claims.ComputerAccount))
                            throw new EnrolSafeException(string.Format("Connection not correctly authenticated (SN: {0}; Auth User: {1})", Request.DeviceSerialNumber, authenticatedToken.User.UserId));

                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainByName(Request.DeviceDNSDomainName);

                        if (!authenticatedToken.User.UserId.Equals(string.Format(@"{0}\{1}$", domain.NetBiosName, Request.DeviceComputerName), System.StringComparison.OrdinalIgnoreCase))
                            throw new EnrolSafeException(string.Format("Connection not correctly authenticated (SN: {0}; Auth User: {1})", Request.DeviceSerialNumber, authenticatedToken.User.UserId));
                    }
                }
                else
                {
                    if (RepoDevice == null)
                    {
                        throw new EnrolSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.DeviceSerialNumber));
                    }
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                    {
                        if (RepoDevice.DeviceProfile.AllowUntrustedReimageJobEnrolment)
                        {
                            if (Database.Jobs.Count(j => j.DeviceSerialNumber == RepoDevice.SerialNumber && j.JobTypeId == JobType.JobTypeIds.SImg && !j.ClosedDate.HasValue) == 0)
                            {
                                throw new EnrolSafeException(string.Format("Device has no open 'Software - Reimage' job (SN: '{0}')", Request.DeviceSerialNumber));
                            }
                        }
                        else
                        {
                            throw new EnrolSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.DeviceSerialNumber));
                        }
                    }
                }
                if (Request.DeviceIsPartOfDomain && !string.IsNullOrWhiteSpace(Request.DeviceComputerName))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 20, "Loading Active Directory Computer Account");
                    System.Guid? uuidGuid = null;
                    System.Guid? macAddressGuid = null;
                    if (!string.IsNullOrEmpty(Request.DeviceUUID))
                        uuidGuid = ADMachineAccount.NetbootGUIDFromUUID(Request.DeviceUUID);
                    if (!string.IsNullOrEmpty(Request.DeviceLanMacAddress))
                        macAddressGuid = ADMachineAccount.NetbootGUIDFromMACAddress(Request.DeviceLanMacAddress);

                    if (domain == null)
                        domain = ActiveDirectory.Context.GetDomainByName(Request.DeviceDNSDomainName);

                    var requestDeviceId = string.Format(@"{0}\{1}", domain.NetBiosName, Request.DeviceComputerName);

                    adMachineAccount = domainController.Value.RetrieveADMachineAccount(requestDeviceId, uuidGuid, macAddressGuid);
                }
                if (RepoDevice == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "New Device, Creating Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.DeviceSerialNumber);
                    DeviceProfile deviceProfile = Database.DeviceProfiles.Find(Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);


                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModelType.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    if (domain == null)
                        domain = ActiveDirectory.Context.GetDomainByName(Request.DeviceDNSDomainName);

                    RepoDevice = new Device
                    {
                        SerialNumber = Request.DeviceSerialNumber,
                        DeviceDomainId = string.Format(@"{0}\{1}", domain.NetBiosName, Request.DeviceComputerName),
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now,
                        LastEnrolDate = DateTime.Now,
                        DeviceDetails = new List<DeviceDetail>()
                    };
                    Database.Devices.Add(RepoDevice);

                    if (!string.IsNullOrEmpty(Request.DeviceLanMacAddress))
                        RepoDevice.DeviceDetails.LanMacAddress(RepoDevice, Request.DeviceLanMacAddress);
                    if (!string.IsNullOrEmpty(Request.DeviceWlanMacAddress))
                        RepoDevice.DeviceDetails.WLanMacAddress(RepoDevice, Request.DeviceWlanMacAddress);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, Request.DeviceSerialNumber);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModelType.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice.DeviceModel = deviceModel;

                    if (!string.IsNullOrEmpty(Request.DeviceLanMacAddress))
                        RepoDevice.DeviceDetails.LanMacAddress(RepoDevice, Request.DeviceLanMacAddress);
                    if (!string.IsNullOrEmpty(Request.DeviceWlanMacAddress))
                        RepoDevice.DeviceDetails.WLanMacAddress(RepoDevice, Request.DeviceWlanMacAddress);

                    if (!RepoDevice.EnrolledDate.HasValue)
                        RepoDevice.EnrolledDate = DateTime.Now;
                    RepoDevice.LastEnrolDate = DateTime.Now;
                }

                if (adMachineAccount == null)
                {
                    if (isAuthenticated || RepoDevice.AllowUnauthenticatedEnrol)
                    {
                        if (RepoDevice.DeviceProfile.ProvisionADAccount)
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, "Provisioning an Active Directory Computer Account");

                            if (string.IsNullOrWhiteSpace(RepoDevice.DeviceProfile.OrganisationalUnit))
                                throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                            if (domain == null)
                                domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(RepoDevice.DeviceProfile.OrganisationalUnit);

                            if (string.IsNullOrEmpty(RepoDevice.DeviceDomainId) || RepoDevice.DeviceProfile.EnforceComputerNameConvention)
                                RepoDevice.DeviceDomainId = RepoDevice.ComputerNameRender(Database, domain);

                            string offlineProvisionDiagnosicInfo;
                            EnrolmentLog.LogSessionTaskProvisioningADAccount(sessionId, RepoDevice.SerialNumber, RepoDevice.DeviceDomainId);
                            adMachineAccount = domainController.Value.RetrieveADMachineAccount(RepoDevice.DeviceDomainId);

                            response.OfflineDomainJoin = domainController.Value.OfflineDomainJoinProvision(RepoDevice.DeviceDomainId, RepoDevice.DeviceProfile.OrganisationalUnit, ref adMachineAccount, out offlineProvisionDiagnosicInfo);

                            EnrolmentLog.LogSessionDiagnosticInformation(sessionId, offlineProvisionDiagnosicInfo);

                            response.RequireReboot = true;
                        }
                        if (adMachineAccount != null)
                        {
                            response.DeviceComputerName = adMachineAccount.Name;
                            response.DeviceDomainName = adMachineAccount.Domain.NetBiosName;
                        }
                        else if (ActiveDirectory.IsValidDomainAccountId(RepoDevice.DeviceDomainId))
                        {
                            string accountUsername;
                            ADDomain accountDomain;
                            ActiveDirectory.ParseDomainAccountId(RepoDevice.DeviceDomainId, out accountUsername, out accountDomain);

                            response.DeviceDomainName = accountDomain == null ? null : accountDomain.NetBiosName;
                            response.DeviceComputerName = accountUsername;
                        }
                        else
                        {
                            response.DeviceDomainName = Request.DeviceDNSDomainName;
                            response.DeviceComputerName = Request.DeviceComputerName;
                        }
                    }
                    else
                    {
                        response.DeviceComputerName = Request.DeviceComputerName;
                        response.DeviceDomainName = Request.DeviceDNSDomainName;
                    }
                }
                else
                {
                    RepoDevice.DeviceDomainId = adMachineAccount.Id.Trim('$');
                    response.DeviceComputerName = adMachineAccount.Name;
                    response.DeviceDomainName = adMachineAccount.Domain.NetBiosName;

                    // Enforce Computer Name Convention
                    if (!adMachineAccount.IsCriticalSystemObject && RepoDevice.DeviceProfile.EnforceComputerNameConvention)
                    {
                        if (string.IsNullOrWhiteSpace(RepoDevice.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(RepoDevice.DeviceProfile.OrganisationalUnit);

                        var calculatedComputerName = RepoDevice.ComputerNameRender(Database, domain);
                        string calculatedAccountUsername;
                        ActiveDirectory.ParseDomainAccountId(calculatedComputerName, out calculatedAccountUsername);

                        if (!Request.DeviceComputerName.Equals(calculatedAccountUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, string.Format("Renaming Device: {0} -> {1}", Request.DeviceComputerName, calculatedComputerName));
                            EnrolmentLog.LogSessionTaskRenamingDevice(sessionId, Request.DeviceComputerName, calculatedComputerName);

                            RepoDevice.DeviceDomainId = calculatedComputerName;
                            response.DeviceDomainName = domain.NetBiosName;
                            response.DeviceComputerName = calculatedAccountUsername;

                            // Create New Account
                            string offlineProvisionDiagnosicInfo;

                            response.OfflineDomainJoin = domainController.Value.OfflineDomainJoinProvision(RepoDevice.DeviceDomainId, RepoDevice.DeviceProfile.OrganisationalUnit, ref adMachineAccount, out offlineProvisionDiagnosicInfo);

                            EnrolmentLog.LogSessionDiagnosticInformation(sessionId, offlineProvisionDiagnosicInfo);

                            response.RequireReboot = true;
                        }
                    }

                    // Enforce Organisational Unit
                    if (!adMachineAccount.IsCriticalSystemObject && response.OfflineDomainJoin == null && RepoDevice.DeviceProfile.EnforceOrganisationalUnit)
                    {
                        var parentDistinguishedName = adMachineAccount.ParentDistinguishedName;
                        if (string.IsNullOrWhiteSpace(RepoDevice.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException(string.Format("The Organisational Unit for the Device Profile '{0}' [{1}] is not set.", RepoDevice.DeviceProfile.Name, RepoDevice.DeviceProfile.Id));

                        if (!parentDistinguishedName.Equals(RepoDevice.DeviceProfile.OrganisationalUnit, StringComparison.OrdinalIgnoreCase)) // Custom OU
                        {
                            var proposedDomain = ActiveDirectory.Context.GetDomainFromDistinguishedName(RepoDevice.DeviceProfile.OrganisationalUnit);
                            var currentDomain = ActiveDirectory.Context.GetDomainFromDistinguishedName(parentDistinguishedName);
                            if (currentDomain != proposedDomain)
                                throw new NotSupportedException("Unable to move the devices organisational unit when the source and destination domains are different.");
                            if (domain == null)
                                domain = proposedDomain;
                            else if (domain != proposedDomain)
                                throw new NotSupportedException("To many domains involved in this enrolment, contact support regarding your scenario.");

                            EnrolmentLog.LogSessionProgress(sessionId, 65, string.Format("Moving Device Organisational Unit: {0} -> {1}", parentDistinguishedName, RepoDevice.DeviceProfile.OrganisationalUnit));
                            EnrolmentLog.LogSessionTaskMovingDeviceOrganisationUnit(sessionId, parentDistinguishedName, RepoDevice.DeviceProfile.OrganisationalUnit);
                            adMachineAccount.MoveOrganisationalUnit(domainController.Value, RepoDevice.DeviceProfile.OrganisationalUnit);
                            response.RequireReboot = true;
                        }
                    }

                }
                if (adMachineAccount != null && !adMachineAccount.IsCriticalSystemObject)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 75, "Updating Active Directory Computer Account Properties");
                    adMachineAccount.UpdateNetbootGUID(Request.DeviceUUID, Request.DeviceLanMacAddress);
                    if (RepoDevice.AssignedUser != null)
                        adMachineAccount.SetDescription(RepoDevice);
                }
                if (RepoDevice.DeviceProfile.DistributionType == DeviceProfile.DistributionTypes.OneToOne)
                {
                    if (RepoDevice.AssignedUser == null)
                    {
                        response.AllowBootstrapperUninstall = false;
                    }
                    else
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 80, "Retrieving Active Directory Assigned User Account");
                        ADUserAccount AssignedUserInfo = ActiveDirectory.RetrieveADUserAccount(RepoDevice.AssignedUser.UserId);
                        EnrolmentLog.LogSessionTaskAssigningUser(sessionId, RepoDevice.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.SamAccountName, AssignedUserInfo.Domain.NetBiosName, AssignedUserInfo.SecurityIdentifier.ToString());
                        response.AllowBootstrapperUninstall = true;
                        response.DeviceAssignedUserIsLocalAdmin = RepoDevice.DeviceProfile.AssignedUserLocalAdmin;
                        response.DeviceAssignedUserUsername = AssignedUserInfo.SamAccountName;
                        response.DeviceAssignedUserDomain = AssignedUserInfo.Domain.NetBiosName;
                        response.DeviceAssignedUserName = AssignedUserInfo.DisplayName;
                        response.DeviceAssignedUserSID = AssignedUserInfo.SecurityIdentifier.ToString();
                    }
                }
                else
                {
                    response.AllowBootstrapperUninstall = true;
                }
                if (!string.IsNullOrEmpty(Request.DeviceWlanMacAddress) && !string.IsNullOrEmpty(RepoDevice.DeviceProfile.CertificateProviderId))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Provisioning a Wireless Certificate");

                    var allocationResult = RepoDevice.AllocateCertificate(Database);
                    var deviceCertificate = allocationResult.Item1;
                    if (deviceCertificate != null)
                    {
                        bool certAlreadyInstalled = false;
                        if (Request.DeviceCertificates != null && Request.DeviceCertificates.Count > 0)
                        {
                            foreach (string existingCertName in Request.DeviceCertificates)
                            {
                                if (existingCertName.Contains(deviceCertificate.Name))
                                {
                                    certAlreadyInstalled = true;
                                    break;
                                }
                            }
                        }
                        if (!certAlreadyInstalled)
                        {
                            EnrolmentLog.LogSessionTaskProvisioningWirelessCertificate(sessionId, RepoDevice.SerialNumber, deviceCertificate.Name);
                            response.DeviceCertificate = System.Convert.ToBase64String(deviceCertificate.Content);
                        }
                    }
                    response.DeviceCertificateRemoveExisting = allocationResult.Item2;
                }

                // Reset 'AllowUnauthenticatedEnrol'
                if (RepoDevice.AllowUnauthenticatedEnrol)
                    RepoDevice.AllowUnauthenticatedEnrol = false;

                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new EnrolResponse
                {
                    SessionId = sessionId,
                    ErrorMessage = ex.Message
                };
            }
            catch (System.Exception ex2)
            {
                ex2.ToExceptionless().Submit();
                EnrolmentLog.LogSessionError(sessionId, ex2);
                throw ex2;
            }
            finally
            {
                EnrolmentLog.LogSessionFinished(sessionId);
            }
            return response;
        }
    }
}