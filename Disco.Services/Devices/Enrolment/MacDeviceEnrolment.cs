using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using PListNet;
using PListNet.Nodes;
using Renci.SshNet;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Disco.Services.Devices.Enrolment
{
    public static class MacDeviceEnrolment
    {
        public static MacSecureEnrolResponse SecureEnrol(DiscoDataContext Database, string Host)
        {
            MacEnrol trustedRequest = new MacEnrol();
            string sessionId = Guid.NewGuid().ToString("B");
            MacSecureEnrolResponse MacSecureEnrol;
            try
            {
                EnrolmentLog.LogSessionStarting(sessionId, Host, EnrolmentTypes.MacSecure);
                EnrolmentLog.LogSessionProgress(sessionId, 0, $"Connecting to '{Host}' as '{Database.DiscoConfiguration.Bootstrapper.MacSshUsername}'");

                var sshConnectionInfo = new KeyboardInteractiveConnectionInfo(Host, Database.DiscoConfiguration.Bootstrapper.MacSshUsername);
                sshConnectionInfo.AuthenticationPrompt += (sender, e) =>
                {
                    foreach (var prompt in e.Prompts)
                    {
                        if (prompt.Request.StartsWith("Password", StringComparison.OrdinalIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 10, $"Authenticating at '{Host}' as '{Database.DiscoConfiguration.Bootstrapper.MacSshUsername}'");
                            prompt.Response = Database.DiscoConfiguration.Bootstrapper.MacSshPassword;
                        }
                    }
                };

                using (var sshClient = new SshClient(sshConnectionInfo))
                {
                    sshClient.Connect();

                    try
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 30, "Retrieving System Profile Information");
                        var sshResult = sshClient.RunCommand("system_profiler -xml SPHardwareDataType SPNetworkDataType SPSoftwareDataType");

                        ArrayNode profilerData;

                        using (var sshResultStream = new MemoryStream())
                        {
                            using (var sshResultWriter = new StreamWriter(sshResultStream, Encoding.UTF8, 0x400, true))
                            {
                                sshResultWriter.Write(sshResult.Result);
                            }
                            sshResultStream.Position = 0;

                            profilerData = PList.Load(sshResultStream) as ArrayNode;
                        }

                        EnrolmentLog.LogSessionProgress(sessionId, 90, "Processing System Profile Information");

                        DictionaryNode profilerDataHardware = null;
                        ArrayNode profilerDataNetwork = null;
                        DictionaryNode profilerDataSoftware = null;

                        if (profilerData == null)
                            throw new InvalidOperationException("System Profiler didn't return the expected response");

                        foreach (var node in profilerData.OfType<DictionaryNode>())
                        {
                            var nodeItems = ((ArrayNode)node["_items"]);
                            PNode nodeDataType;

                            if (node.TryGetValue("_dataType", out nodeDataType) && nodeDataType is StringNode)
                            {
                                switch (((StringNode)nodeDataType).Value)
                                {
                                    case "SPHardwareDataType":
                                        profilerDataHardware = (DictionaryNode)nodeItems[0];
                                        break;
                                    case "SPNetworkDataType":
                                        profilerDataNetwork = nodeItems;
                                        break;
                                    case "SPSoftwareDataType":
                                        profilerDataSoftware = (DictionaryNode)nodeItems[0];
                                        break;
                                }
                            }

                        }

                        if (profilerDataHardware == null || profilerDataNetwork == null || profilerDataSoftware == null)
                            throw new InvalidOperationException("System Profiler didn't return information for a requested data type");

                        trustedRequest.DeviceSerialNumber = ((StringNode)profilerDataHardware["serial_number"]).Value;
                        trustedRequest.DeviceUUID = ((StringNode)profilerDataHardware["platform_UUID"]).Value;
                        trustedRequest.DeviceComputerName = ((StringNode)profilerDataSoftware["local_host_name"]).Value;

                        var profilerDataNetworkEthernet = profilerDataNetwork.OfType<DictionaryNode>().FirstOrDefault(e => ((StringNode)e["_name"]).Value == "Ethernet");
                        if (profilerDataNetworkEthernet != null)
                        {
                            trustedRequest.DeviceLanMacAddress = ((StringNode)((DictionaryNode)profilerDataNetworkEthernet["Ethernet"])["MAC Address"]).Value;
                        }

                        var profilerDataNetworkWiFi = profilerDataNetwork.OfType<DictionaryNode>().FirstOrDefault(e => ((StringNode)e["_name"]).Value == "Wi-Fi");
                        if (profilerDataNetworkWiFi != null)
                        {
                            trustedRequest.DeviceWlanMacAddress = ((StringNode)((DictionaryNode)profilerDataNetworkWiFi["Ethernet"])["MAC Address"]).Value;
                        }

                        trustedRequest.DeviceManufacturer = "Apple Inc.";
                        trustedRequest.DeviceModel = ((StringNode)profilerDataHardware["machine_model"]).Value;

                        trustedRequest.DeviceModelType = ParseMacModelType(((StringNode)profilerDataHardware["machine_name"]).Value);

                        EnrolmentLog.LogSessionProgress(sessionId, 99, "Disconnecting");

                        sshClient.Disconnect();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (sshClient != null)
                        {
                            bool connected = sshClient.IsConnected;
                            if (connected)
                            {
                                sshClient.Disconnect();
                            }
                        }
                    }
                }

                EnrolmentLog.LogSessionProgress(sessionId, 100, "Disconnected, Starting Disco ICT Enrolment");
                MacSecureEnrolResponse response = MacSecureEnrolResponse.FromMacEnrolResponse(Enrol(Database, trustedRequest, true, sessionId));
                EnrolmentLog.LogSessionFinished(sessionId);
                MacSecureEnrol = response;
            }
            catch (System.Exception ex)
            {
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

        #endregion

        public static MacEnrolResponse Enrol(DiscoDataContext Database, MacEnrol Request, bool Trusted, string OpenSessionId = null)
        {
            string sessionId;
            if (OpenSessionId == null)
            {
                sessionId = Guid.NewGuid().ToString("B");
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
                if (Request.DeviceSerialNumber.Contains("/") || Request.DeviceSerialNumber.Contains(@"\"))
                    throw new EnrolmentSafeException(@"The serial number cannot contain '/' or '\' characters.");

                EnrolmentLog.LogSessionProgress(sessionId, 10, "Querying Database");
                Device RepoDevice = Database.Devices.Include("AssignedUser").Include("DeviceProfile").Include("DeviceProfile").Where(d => d.SerialNumber == Request.DeviceSerialNumber).FirstOrDefault();
                if (!Trusted)
                {
                    if (RepoDevice == null)
                        throw new EnrolmentSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.DeviceSerialNumber));
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                        throw new EnrolmentSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.DeviceSerialNumber));
                }
                if (RepoDevice == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "New Device, Building Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.DeviceSerialNumber);
                    DeviceProfile deviceProfile = Database.DeviceProfiles.Find(Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer, Request.DeviceModel, Request.DeviceModelType);
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

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer, Request.DeviceModel, Request.DeviceModelType);
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
            catch (EnrolmentSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new MacEnrolResponse { ErrorMessage = ex.Message };
            }
            catch (Exception ex2)
            {
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
    }
}
