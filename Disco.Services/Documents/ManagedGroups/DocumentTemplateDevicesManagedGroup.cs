using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Documents.ManagedGroups
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
            DocumentTemplateId = DocumentTemplate.Id;
            DocumentTemplateDescription = DocumentTemplate.Description;
            DocumentTemplateScope = DocumentTemplate.Scope;
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
                    var deviceFilter = Database.DeviceAttachments.Include("Device").Where(a => a.DocumentTemplateId == DocumentTemplateId);

                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        deviceFilter = deviceFilter.Where(a => a.Timestamp >= Configuration.FilterBeginDate);
                    }

                    return deviceFilter.Select(a => a.Device.DeviceDomainId)
                            .Distinct()
                            .ToList()
                            .Where(ActiveDirectory.IsValidDomainAccountId)
                            .Select(id => id + "$");
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    var jobFilter = Database.JobAttachments.Include("Job.Device").Where(a => a.DocumentTemplateId == DocumentTemplateId && a.Job.Device.DeviceDomainId != null);

                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        jobFilter = jobFilter.Where(a => a.Timestamp >= Configuration.FilterBeginDate);
                    }

                    return jobFilter.Select(a => a.Job.Device.DeviceDomainId)
                            .Distinct()
                            .ToList()
                            .Where(ActiveDirectory.IsValidDomainAccountId)
                            .Select(id => id + "$");
                case DocumentTemplate.DocumentTemplateScopes.User:
                    var userFilter = Database.UserAttachments.Include("User.Device.DeviceUserAssignments.Device").Where(a => a.DocumentTemplateId == DocumentTemplateId && a.User.DeviceUserAssignments.Where(ua => ua.UnassignedDate == null && ua.Device.DeviceDomainId != null).Any());

                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        userFilter = userFilter.Where(a => a.Timestamp >= Configuration.FilterBeginDate);
                    }

                    return userFilter.SelectMany(a => a.User.DeviceUserAssignments)
                            .Where(a => a.UnassignedDate == null)
                            .Select(a => a.Device.DeviceDomainId)
                            .Distinct()
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
            var query = Database.DeviceAttachments.Include("Device")
                .Where(a => a.DocumentTemplateId == DocumentTemplateId && a.DeviceSerialNumber == DeviceSerialNumber && a.Device.DeviceDomainId != null);

            if (Configuration.FilterBeginDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= Configuration.FilterBeginDate);
            }

            var deviceDomainId = query.Select(a => a.Device.DeviceDomainId).FirstOrDefault();
            if (ActiveDirectory.IsValidDomainAccountId(deviceDomainId))
            {
                DeviceAccountId = deviceDomainId + "$";
                return true;
            }
            else
            {
                DeviceAccountId = null;
                return false;
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
            var query = Database.JobAttachments.Include("Job.Device")
                .Where(a => a.DocumentTemplateId == DocumentTemplateId && a.JobId == JobId && a.Job.Device.DeviceDomainId != null);

            if (Configuration.FilterBeginDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= Configuration.FilterBeginDate);
            }

            var deviceRecord = query.Select(a => Tuple.Create(a.Job.DeviceSerialNumber, a.Job.Device.DeviceDomainId)).FirstOrDefault();
            if (ActiveDirectory.IsValidDomainAccountId(deviceRecord.Item2))
            {
                DeviceSerialNumber = deviceRecord.Item1;
                DeviceAccountId = deviceRecord.Item2 + "$";
                return true;
            }
            else
            {
                DeviceSerialNumber = null;
                DeviceAccountId = null;
                return false;
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
            var query = Database.UserAttachments.Include("User.DeviceUserAssignments.Device")
                .Where(a => a.DocumentTemplateId == DocumentTemplateId && a.UserId == UserId && a.User.DeviceUserAssignments.Any(d => d.UnassignedDate == null && d.Device.DeviceDomainId != null));

            if (Configuration.FilterBeginDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= Configuration.FilterBeginDate);
            }

            var deviceRecords = query.Take(1).SelectMany(a => a.User.DeviceUserAssignments)
                    .Where(a => a.UnassignedDate == null && a.Device.DeviceDomainId != null)
                    .Select(a => new { a.Device.SerialNumber, a.Device.DeviceDomainId }).ToList()
                    .Where(r => ActiveDirectory.IsValidDomainAccountId(r.DeviceDomainId)).ToList();
            if (deviceRecords.Count > 0)
            {
                Devices = deviceRecords.Select(r => Tuple.Create(r.DeviceDomainId + "$", r.SerialNumber)).ToList();
                return true;
            }
            else
            {
                Devices = null;
                return false;
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
                            bool jobsHaveTemplate;
                            if (Configuration.FilterBeginDate.HasValue)
                            {
                                jobsHaveTemplate = e.Database.Jobs
                                    .Where(j => j.DeviceSerialNumber == deviceSerialNumber)
                                    .Any(j => j.JobAttachments.Any(a => a.DocumentTemplateId == DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate));
                            }
                            else
                            {
                                jobsHaveTemplate = e.Database.Jobs
                                    .Where(j => j.DeviceSerialNumber == deviceSerialNumber)
                                    .Any(j => j.JobAttachments.Any(a => a.DocumentTemplateId == DocumentTemplateId));
                            }

                            if (jobsHaveTemplate)
                            {
                                if (deviceAccountIdValid)
                                    AddMember(device.SerialNumber, (database) => new string[] { deviceAccountId + "$" });
                                if (devicePreviousAccountIdValid)
                                    RemoveMember(device.SerialNumber, (database) => new string[] { devicePreviousAccountId + "$" });
                            }
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.User:
                            bool userHasTemplate;
                            if (Configuration.FilterBeginDate.HasValue)
                            {
                                userHasTemplate = e.Database.Devices
                                    .Where(d => d.SerialNumber == deviceSerialNumber)
                                    .Select(d => d.AssignedUser)
                                    .Any(u => u.UserAttachments.Any(a => a.DocumentTemplateId == DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate));
                            }
                            else
                            {
                                userHasTemplate = e.Database.Devices
                                    .Where(d => d.SerialNumber == deviceSerialNumber)
                                    .Select(d => d.AssignedUser)
                                    .Any(u => u.UserAttachments.Any(a => a.DocumentTemplateId == DocumentTemplateId));
                            }

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
                    {
                        if (Configuration.FilterBeginDate.HasValue)
                        {
                            previousUserHasTemplate = e.Database.Users
                                .Where(u => u.UserId == devicePreviousAssignedUserId && u.UserAttachments.Any(ua => ua.DocumentTemplateId == DocumentTemplateId && ua.Timestamp >= Configuration.FilterBeginDate))
                                .Any();
                        }
                        else
                        {
                            previousUserHasTemplate = e.Database.Users
                                .Where(u => u.UserId == devicePreviousAssignedUserId && u.UserAttachments.Any(ua => ua.DocumentTemplateId == DocumentTemplateId))
                                .Any();
                        }
                    }

                    if (deviceCurrentAssignedUserId != null)
                    {
                        if (Configuration.FilterBeginDate.HasValue)
                        {
                            currentUserHasTemplate = e.Database.Users
                                .Where(u => u.UserId == deviceCurrentAssignedUserId && u.UserAttachments.Any(ua => ua.DocumentTemplateId == DocumentTemplateId && ua.Timestamp >= Configuration.FilterBeginDate))
                                .Any();
                        }
                        else
                        {
                            currentUserHasTemplate = e.Database.Users
                                .Where(u => u.UserId == deviceCurrentAssignedUserId && u.UserAttachments.Any(ua => ua.DocumentTemplateId == DocumentTemplateId))
                                .Any();
                        }
                    }

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

            if (deviceAssignmentRepositorySubscription != null)
                deviceAssignmentRepositorySubscription.Dispose();
        }
    }
}
