using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.BI.DocumentTemplateBI.ManagedGroups
{
    public class DocumentTemplateDevicesManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DocumentTemplate_{0}_Devices";
        private const string DeviceDescriptionFormat = "Devices with a {0} attachment will be added to this Active Directory group.";
        private const string DescriptionFormat = "{0}s with a {1} attachment will have any associated devices added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Related Devices Linked Group";
        private const string GroupDescriptionFormat = "{0} [Document Template Devices]";

        private IDisposable repositoryAddSubscription;
        private IDisposable repositoryRemoveSubscription;
        private IDisposable deviceRenameRepositorySubscription;
        private IDisposable jobCloseRepositorySubscription;
        private IDisposable deviceAssignmentRepositorySubscription;
        private string DocumentTemplateId;
        private string DocumentTemplateDescription;
        private string DocumentTemplateScope;

        public override string Description { get { return GetDescription(DocumentTemplateScope, DocumentTemplateDescription); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, DocumentTemplateDescription); } }
        public override bool IncludeFilterBeginDate { get { return true; } }

        private DocumentTemplateDevicesManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DocumentTemplate DocumentTemplate)
            : base(Key, Configuration)
        {
            this.DocumentTemplateId = DocumentTemplate.Id;
            this.DocumentTemplateDescription = DocumentTemplate.Description;
            this.DocumentTemplateScope = DocumentTemplate.Scope;
        }

        public override void Initialize()
        {
            // Subscribe to changes
            switch (DocumentTemplateScope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    // Observe Device Attachments
                    repositoryAddSubscription = DocumentTemplateManagedGroups.DeviceAttachmentAddRepositoryEvents.Value
                        .Where(e => ((DeviceAttachment)e.Entity).DocumentTemplateId == DocumentTemplateId)
                        .Subscribe(ProcessDeviceAttachmentAddEvent);
                    repositoryRemoveSubscription = DocumentTemplateManagedGroups.DeviceAttachmentRemoveEvents.Value
                        .Where(e => e.Item3 == DocumentTemplateId)
                        .Subscribe(ProcessDeviceAttachmentRemoveEvent);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    // Observe Job Attachments
                    repositoryAddSubscription = DocumentTemplateManagedGroups.JobAttachmentAddRepositoryEvents.Value
                        .Where(e => ((JobAttachment)e.Entity).DocumentTemplateId == DocumentTemplateId)
                        .Subscribe(ProcessJobAttachmentAddEvent);
                    repositoryRemoveSubscription = DocumentTemplateManagedGroups.JobAttachmentRemoveEvents.Value
                        .Where(e => e.Item3 == DocumentTemplateId)
                        .Subscribe(ProcessJobAttachmentRemoveEvent);
                    // Observe Job Close/Reopen
                    jobCloseRepositorySubscription = DocumentTemplateManagedGroups.JobCloseRepositoryEvents.Value
                        .Subscribe(ProcessJobCloseRepositoryEvent);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    // Observe User Attachments
                    repositoryAddSubscription = DocumentTemplateManagedGroups.UserAttachmentAddRepositoryEvents.Value
                        .Where(e => ((UserAttachment)e.Entity).DocumentTemplateId == DocumentTemplateId)
                        .Subscribe(ProcessUserAttachmentAddEvent);
                    repositoryRemoveSubscription = DocumentTemplateManagedGroups.UserAttachmentRemoveEvents.Value
                        .Where(e => e.Item3 == DocumentTemplateId)
                        .Subscribe(ProcessUserAttachmentRemoveEvent);
                    // Observe Device Assignments
                    deviceAssignmentRepositorySubscription = DocumentTemplateManagedGroups.DeviceAssignmentRepositoryEvents.Value
                        .Subscribe(ProcessDeviceAssignmentRepositoryEvent);
                    break;
            }

            // Observe Device Renaming (DeviceDomainId)
            deviceRenameRepositorySubscription = DocumentTemplateManagedGroups.DeviceRenameRepositoryEvents.Value
                .Subscribe(ProcessDeviceRenameRepositoryEvent);
        }

        public static string GetKey(DocumentTemplate DocumentTemplate)
        {
            return string.Format(KeyFormat, DocumentTemplate.Id);
        }
        private static string GetDescription(string DocumentTemplateScope, string DocumentTemplateDescription)
        {
            switch (DocumentTemplateScope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    return string.Format(DeviceDescriptionFormat, DocumentTemplateDescription);
                case DocumentTemplate.DocumentTemplateScopes.Job:
                case DocumentTemplate.DocumentTemplateScopes.User:
                    return string.Format(DescriptionFormat, DocumentTemplateScope, DocumentTemplateDescription);
                default:
                    throw new ArgumentException("Unknown Document Template Scope", "Scope");
            }
        }
        public static string GetDescription(DocumentTemplate DocumentTemplate)
        {
            return GetDescription(DocumentTemplate.Scope, DocumentTemplate.Description);
        }
        public static string GetCategoryDescription(DocumentTemplate DocumentTemplate)
        {
            return CategoryDescriptionFormat;
        }

        public static bool TryGetManagedGroup(DocumentTemplate DocumentTemplate, out DocumentTemplateDevicesManagedGroup ManagedGroup)
        {
            ADManagedGroup managedGroup;
            string key = GetKey(DocumentTemplate);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out managedGroup))
            {
                ManagedGroup = (DocumentTemplateDevicesManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static DocumentTemplateDevicesManagedGroup Initialize(DocumentTemplate Template)
        {
            var key = GetKey(Template);

            if (!string.IsNullOrEmpty(Template.DevicesLinkedGroup))
            {
                var config = ADManagedGroup.ConfigurationFromJson(Template.DevicesLinkedGroup);

                if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                {
                    var group = new DocumentTemplateDevicesManagedGroup(
                        key,
                        config,
                        Template);

                    // Add to AD Context
                    ActiveDirectory.Context.ManagedGroups.AddOrUpdate(group);

                    return group;
                }
            }

            // Remove from AD Context
            ActiveDirectory.Context.ManagedGroups.Remove(key);

            return null;
        }

        public override IEnumerable<string> DetermineMembers(DiscoDataContext Database)
        {
            switch (DocumentTemplateScope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    return Database.Devices
                        .Where(d => d.DeviceDomainId != null && d.DeviceAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))
                        .Select(d => d.DeviceDomainId)
                        .ToList()
                        .Where(ActiveDirectory.IsValidDomainAccountId)
                        .Select(id => id + "$");
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    return Database.Jobs
                        .Where(j => !j.ClosedDate.HasValue && j.Device.DeviceDomainId != null && j.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))
                        .Select(j => j.Device.DeviceDomainId)
                        .Distinct()
                        .ToList()
                        .Where(ActiveDirectory.IsValidDomainAccountId)
                        .Select(id => id + "$");
                case DocumentTemplate.DocumentTemplateScopes.User:
                    return Database.Users
                        .Where(u => u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))
                        .SelectMany(u => u.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue && dua.Device.DeviceDomainId != null), (u, dua) => dua.Device.DeviceDomainId)
                        .ToList()
                        .Where(ActiveDirectory.IsValidDomainAccountId)
                        .Select(id => id + "$");
                default:
                    return Enumerable.Empty<string>();
            }
        }

        #region Device Scope
        private bool DeviceContainsAttachment(DiscoDataContext Database, string DeviceSerialNumber, out string DeviceAccountId)
        {
            var result = Database.Devices
                .Where(d => d.SerialNumber == DeviceSerialNumber && d.DeviceDomainId != null)
                .Select(d => new Tuple<string, bool>(d.DeviceDomainId, d.DeviceAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId)))
                .FirstOrDefault();

            if (result == null)
            {
                DeviceAccountId = null;
                return false;
            }
            else
            {
                if (ActiveDirectory.IsValidDomainAccountId(result.Item1))
                {
                    DeviceAccountId = result.Item1 + "$";
                    return result.Item2;
                }
                else
                {
                    DeviceAccountId = result.Item1 + "$";
                    return false;
                }
            }
        }

        private void ProcessDeviceAttachmentAddEvent(RepositoryMonitorEvent e)
        {
            var attachment = (DeviceAttachment)e.Entity;

            string deviceAccountId;
            if (DeviceContainsAttachment(e.Database, attachment.DeviceSerialNumber, out deviceAccountId))
                AddMember(attachment.DeviceSerialNumber, (database) => new string[] { deviceAccountId });
        }
        private void ProcessDeviceAttachmentRemoveEvent(Tuple<DiscoDataContext, int, string, string> e)
        {
            var deviceSerialNumber = e.Item3;

            RemoveMember(deviceSerialNumber, (database) =>
            {
                string deviceAccountId;
                if (!DeviceContainsAttachment(database, deviceSerialNumber, out deviceAccountId) && deviceAccountId != null)
                    return new string[] { deviceAccountId };
                else
                    return null;
            });
        }
        #endregion

        #region Job Scope
        private bool JobsContainAttachment(DiscoDataContext Database, int JobId, out string DeviceAccountId, out string DeviceSerialNumber)
        {
            var result = Database.Jobs
                .Where(j => j.Id == JobId && j.Device.DeviceDomainId != null)
                .Select(j => new Tuple<string, string, bool>(
                    j.Device.DeviceDomainId,
                    j.Device.SerialNumber,
                    j.Device.Jobs.Where(dj => !dj.ClosedDate.HasValue).Any(dj => dj.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId)))
                ).FirstOrDefault();

            if (result == null)
            {
                DeviceAccountId = null;
                DeviceSerialNumber = null;
                return false;
            }
            else
            {
                if (ActiveDirectory.IsValidDomainAccountId(result.Item1))
                {
                    DeviceAccountId = result.Item1 + "$";
                    DeviceSerialNumber = result.Item2;
                    return result.Item3;
                }
                else
                {
                    DeviceAccountId = result.Item1 + "$";
                    DeviceSerialNumber = result.Item2;
                    return false;
                }
            }
        }

        private void ProcessJobAttachmentAddEvent(RepositoryMonitorEvent e)
        {
            var attachment = (JobAttachment)e.Entity;

            string deviceAccountId;
            string deviceSerialNumber;
            if (JobsContainAttachment(e.Database, attachment.JobId, out deviceAccountId, out deviceSerialNumber))
                AddMember(deviceSerialNumber, (database) => new string[] { deviceAccountId });
        }
        private void ProcessJobAttachmentRemoveEvent(Tuple<DiscoDataContext, int, string, int> e)
        {
            var jobId = e.Item4;
            string deviceSerialNumber = e.Item1.Jobs.Where(j => j.Id == jobId && j.DeviceSerialNumber != null).Select(j => j.DeviceSerialNumber).FirstOrDefault();

            if (deviceSerialNumber != null)
            {
                RemoveMember(deviceSerialNumber, (database) =>
                {
                    string deviceAccountId;
                    if (!JobsContainAttachment(database, jobId, out deviceAccountId, out deviceSerialNumber) &&
                            deviceSerialNumber != null && deviceAccountId != null)
                        return new string[] { deviceAccountId };
                    else
                        return null;
                });
            }
        }
        #endregion

        #region User Scope
        private bool DeviceUserContainAttachment(DiscoDataContext Database, string UserId, out List<Tuple<string, string>> Devices)
        {
            var result = Database.Users
                .Where(u => u.UserId == UserId)
                .Select(u => new Tuple<bool, IEnumerable<Tuple<string, string>>>(
                    u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId),
                    u.DeviceUserAssignments
                        .Where(dua => !dua.UnassignedDate.HasValue && dua.Device.DeviceDomainId != null)
                        .Select(dua => new Tuple<string, string>(dua.Device.DeviceDomainId, dua.Device.SerialNumber)))
                    ).FirstOrDefault();

            if (result == null)
            {
                Devices = null;
                return false;
            }
            else
            {
                Devices = result.Item2
                    .Where(d => ActiveDirectory.IsValidDomainAccountId(d.Item1))
                    .Select(d => Tuple.Create(d.Item1 + "$", d.Item2))
                    .ToList();
                return result.Item1;
            }
        }

        private void ProcessUserAttachmentAddEvent(RepositoryMonitorEvent e)
        {
            var attachment = (UserAttachment)e.Entity;

            List<Tuple<string, string>> devices;
            if (DeviceUserContainAttachment(e.Database, attachment.UserId, out devices) && devices != null)
                devices.ForEach(d => AddMember(d.Item2, (database) => new string[] { d.Item1 }));
        }
        private void ProcessUserAttachmentRemoveEvent(Tuple<DiscoDataContext, int, string, string> e)
        {
            var userId = e.Item4;

            RemoveMember(userId, (database) =>
            {
                List<Tuple<string, string>> devices;
                if (!DeviceUserContainAttachment(database, userId, out devices) && devices != null)
                    return devices.Select(d => d.Item1);
                else
                    return null;
            });
        }
        #endregion

        private void ProcessDeviceRenameRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var device = (Device)Event.Entity;
            var deviceSerialNumber = device.SerialNumber;
            var deviceAccountId = device.DeviceDomainId;
            var deviceAccountIdValid = ActiveDirectory.IsValidDomainAccountId(deviceAccountId);
            var devicePreviousAccountId = Event.GetPreviousPropertyValue<string>("DeviceDomainId");
            var devicePreviousAccountIdValid = ActiveDirectory.IsValidDomainAccountId(devicePreviousAccountId);

            if (deviceAccountIdValid || devicePreviousAccountIdValid)
            {
                Event.ExecuteAfterCommit(e =>
                {
                    switch (DocumentTemplateScope)
                    {
                        case DocumentTemplate.DocumentTemplateScopes.Device:
                            if (DeviceContainsAttachment(e.Database, device.SerialNumber, out deviceAccountId))
                            {
                                if (deviceAccountIdValid)
                                    AddMember(device.SerialNumber, (database) => new string[] { deviceAccountId });
                                if (devicePreviousAccountIdValid)
                                    RemoveMember(device.SerialNumber, (database) => new string[] { devicePreviousAccountId + "$" });
                            }
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.Job:
                            var jobsHaveTemplate = e.Database.Jobs
                                .Where(j => !j.ClosedDate.HasValue && j.DeviceSerialNumber == deviceSerialNumber)
                                .Any(j => j.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId));

                            if (jobsHaveTemplate)
                            {
                                if (deviceAccountIdValid)
                                    AddMember(device.SerialNumber, (database) => new string[] { deviceAccountId + "$" });
                                if (devicePreviousAccountIdValid)
                                    RemoveMember(device.SerialNumber, (database) => new string[] { devicePreviousAccountId + "$" });
                            }
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.User:
                            var userHasTemplate = e.Database.Devices
                                .Where(d => d.SerialNumber == deviceSerialNumber)
                                .Select(d => d.AssignedUser)
                                .Any(u => u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId));

                            if (userHasTemplate)
                            {
                                if (deviceAccountIdValid)
                                    AddMember(device.SerialNumber, (database) => new string[] { deviceAccountId + "$" });
                                if (devicePreviousAccountIdValid)
                                    RemoveMember(device.SerialNumber, (database) => new string[] { devicePreviousAccountId + "$" });
                            }
                            break;
                    }
                });
            }
        }

        private void ProcessJobCloseRepositoryEvent(RepositoryMonitorEvent e)
        {
            var job = (Job)e.Entity;

            if (job.DeviceSerialNumber != null)
            {
                var jobId = job.Id;

                var relevantJob = e.Database.Jobs
                    .Where(j => j.Id == jobId && j.JobAttachments.Any(ja => ja.DocumentTemplateId == this.DocumentTemplateId))
                    .Any();

                if (relevantJob)
                {
                    string deviceAccountId;
                    string deviceSerialNumber;
                    if (JobsContainAttachment(e.Database, jobId, out deviceAccountId, out deviceSerialNumber))
                        AddMember(deviceSerialNumber, (database) => new string[] { deviceAccountId });
                    else
                        RemoveMember(deviceSerialNumber, (database) => new string[] { deviceAccountId });
                }
            }
        }

        private void ProcessDeviceAssignmentRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var device = (Device)Event.Entity;
            var deviceSerialNumber = device.SerialNumber;
            var deviceAccountId = device.DeviceDomainId;

            if (ActiveDirectory.IsValidDomainAccountId(deviceAccountId))
            {
                var deviceCurrentAssignedUserId = device.AssignedUserId;
                var devicePreviousAssignedUserId = Event.GetPreviousPropertyValue<string>("AssignedUserId");

                Event.ExecuteAfterCommit(e =>
                {
                    bool previousUserHasTemplate = false;
                    bool currentUserHasTemplate = false;

                    if (devicePreviousAssignedUserId != null)
                        previousUserHasTemplate = e.Database.Users
                            .Where(u => u.UserId == devicePreviousAssignedUserId && u.UserAttachments.Any(ua => ua.DocumentTemplateId == this.DocumentTemplateId))
                            .Any();

                    if (deviceCurrentAssignedUserId != null)
                        currentUserHasTemplate = e.Database.Users
                            .Where(u => u.UserId == deviceCurrentAssignedUserId && u.UserAttachments.Any(ua => ua.DocumentTemplateId == this.DocumentTemplateId))
                            .Any();

                    if (!previousUserHasTemplate && currentUserHasTemplate)
                        AddMember(deviceSerialNumber, (database) => new string[] { deviceAccountId + "$" });
                    else if (previousUserHasTemplate && !currentUserHasTemplate)
                        RemoveMember(deviceSerialNumber, (database) => new string[] { deviceAccountId + "$" });
                });
            }
        }

        public override void Dispose()
        {
            if (repositoryAddSubscription != null)
                repositoryAddSubscription.Dispose();

            if (repositoryRemoveSubscription != null)
                repositoryRemoveSubscription.Dispose();

            if (deviceRenameRepositorySubscription != null)
                deviceRenameRepositorySubscription.Dispose();

            if (jobCloseRepositorySubscription != null)
                jobCloseRepositorySubscription.Dispose();

            if (deviceAssignmentRepositorySubscription != null)
                deviceAssignmentRepositorySubscription.Dispose();
        }
    }
}
