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
    public class DocumentTemplateUsersManagedGroup : ADManagedGroup
    {
        private const string KeyFormat = "DocumentTemplate_{0}_Users";
        private const string UserDescriptionFormat = "Users with a {0} attachment will be added to this Active Directory group.";
        private const string DescriptionFormat = "{0}s with a {1} attachment will have any associated users added to this Active Directory group.";
        private const string CategoryDescriptionFormat = "Related Users Linked Group";
        private const string GroupDescriptionFormat = "{0} [Document Template Users]";

        private IDisposable repositoryAddSubscription;
        private IDisposable repositoryRemoveSubscription;
        private IDisposable deviceAssignmentRepositorySubscription;
        private string DocumentTemplateId;
        private string DocumentTemplateDescription;
        private string DocumentTemplateScope;

        public override string Description { get { return GetDescription(DocumentTemplateScope, DocumentTemplateDescription); } }
        public override string CategoryDescription { get { return CategoryDescriptionFormat; } }
        public override string GroupDescription { get { return string.Format(GroupDescriptionFormat, DocumentTemplateDescription); } }
        public override bool IncludeFilterBeginDate { get { return true; } }

        private DocumentTemplateUsersManagedGroup(string Key, ADManagedGroupConfiguration Configuration, DocumentTemplate DocumentTemplate)
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
                    // Observe Device Assignments
                    deviceAssignmentRepositorySubscription = DocumentTemplateManagedGroups.DeviceAssignmentRepositoryEvents.Value
                        .Subscribe(ProcessDeviceAssignmentRepositoryEvent);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    // Observe Job Attachments
                    repositoryAddSubscription = DocumentTemplateManagedGroups.UserAttachmentAddRepositoryEvents.Value
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
                    break;
            }
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
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    return string.Format(DescriptionFormat, DocumentTemplateScope, DocumentTemplateDescription);
                case DocumentTemplate.DocumentTemplateScopes.User:
                    return string.Format(UserDescriptionFormat, DocumentTemplateDescription);
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

        public static bool TryGetManagedGroup(DocumentTemplate DocumentTemplate, out DocumentTemplateUsersManagedGroup ManagedGroup)
        {
            ADManagedGroup managedGroup;
            string key = GetKey(DocumentTemplate);

            if (ActiveDirectory.Context.ManagedGroups.TryGetValue(key, out managedGroup))
            {
                ManagedGroup = (DocumentTemplateUsersManagedGroup)managedGroup;
                return true;
            }
            else
            {
                ManagedGroup = null;
                return false;
            }
        }

        public static DocumentTemplateUsersManagedGroup Initialize(DocumentTemplate Template)
        {
            var key = GetKey(Template);

            if (!string.IsNullOrEmpty(Template.UsersLinkedGroup))
            {
                var config = ADManagedGroup.ConfigurationFromJson(Template.UsersLinkedGroup);

                if (config != null && !string.IsNullOrWhiteSpace(config.GroupId))
                {
                    var group = new DocumentTemplateUsersManagedGroup(
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
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        return Database.Devices
                            .Where(d => d.AssignedUserId != null && d.DeviceAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate))
                            .Select(d => d.AssignedUserId);
                    }
                    else
                    {
                        return Database.Devices
                            .Where(d => d.AssignedUserId != null && d.DeviceAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))
                            .Select(d => d.AssignedUserId);
                    }
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        return Database.Jobs
                            .Where(j => j.UserId != null && j.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate))
                            .Select(j => j.UserId)
                            .Distinct();
                    }
                    else
                    {
                        return Database.Jobs
                            .Where(j => j.UserId != null && j.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))
                            .Select(j => j.UserId)
                            .Distinct();
                    }
                case DocumentTemplate.DocumentTemplateScopes.User:
                    if (Configuration.FilterBeginDate.HasValue)
                    {
                        return Database.Users
                            .Where(u => u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate))
                            .Select(u => u.UserId);
                    }
                    else
                    {
                        return Database.Users
                            .Where(u => u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))
                            .Select(u => u.UserId);
                    }
                default:
                    return Enumerable.Empty<string>();
            }
        }

        #region Device Scope
        private bool DeviceContainsAttachment(DiscoDataContext Database, string DeviceSerialNumber, out string UserId)
        {
            Tuple<string, bool> result;

            if (Configuration.FilterBeginDate.HasValue)
            {
                result = Database.Devices
                    .Where(d => d.SerialNumber == DeviceSerialNumber && d.AssignedUser != null)
                    .Select(d => Tuple.Create(
                        d.AssignedUserId,
                        d.DeviceAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate)))
                    .FirstOrDefault();
            }
            else
            {
                result = Database.Devices
                    .Where(d => d.SerialNumber == DeviceSerialNumber && d.AssignedUser != null)
                    .Select(d => Tuple.Create(
                        d.AssignedUserId,
                        d.DeviceAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId)))
                    .FirstOrDefault();
            }

            if (result == null)
            {
                UserId = null;
                return false;
            }
            else
            {
                UserId = result.Item1;
                return result.Item2;
            }
        }

        private void ProcessDeviceAttachmentAddEvent(RepositoryMonitorEvent e)
        {
            var attachment = (DeviceAttachment)e.Entity;

            string userId;
            if (DeviceContainsAttachment(e.Database, attachment.DeviceSerialNumber, out userId) && userId != null)
                AddMember(userId, (database) => new string[] { userId });
        }
        private void ProcessDeviceAttachmentRemoveEvent(Tuple<DiscoDataContext, int, string, string> e)
        {
            var deviceSerialNumber = e.Item4;
            string userId = e.Item1.Devices.Where(d => d.SerialNumber == deviceSerialNumber && d.AssignedUserId != null).Select(j => j.AssignedUserId).FirstOrDefault();

            if (userId != null)
            {
                RemoveMember(userId, (database) =>
                {
                    if (DeviceContainsAttachment(database, deviceSerialNumber, out userId) && userId != null)
                        return new string[] { userId };
                    else
                        return null;
                });
            }
        }
        #endregion

        #region Job Scope
        private bool JobsContainAttachment(DiscoDataContext Database, int JobId, out string UserId)
        {
            Tuple<string, bool> result;

            if (Configuration.FilterBeginDate.HasValue)
            {
                result = Database.Jobs
                    .Where(j => j.Id == JobId && j.UserId != null)
                    .Select(j => Tuple.Create(
                        j.UserId,
                        j.User.Jobs.Any(uj => uj.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate))))
                    .FirstOrDefault();
            }
            else
            {
                result = Database.Jobs
                    .Where(j => j.Id == JobId && j.UserId != null)
                    .Select(j => Tuple.Create(
                        j.UserId,
                        j.User.Jobs.Any(uj => uj.JobAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId))))
                    .FirstOrDefault();
            }

            if (result == null)
            {
                UserId = null;
                return false;
            }
            else
            {
                UserId = result.Item1;
                return result.Item2;
            }
        }

        private void ProcessJobAttachmentAddEvent(RepositoryMonitorEvent e)
        {
            var attachment = (JobAttachment)e.Entity;

            string userId;
            if (JobsContainAttachment(e.Database, attachment.JobId, out userId) && userId != null)
                AddMember(userId, (database) => new string[] { userId });
        }
        private void ProcessJobAttachmentRemoveEvent(Tuple<DiscoDataContext, int, string, int> e)
        {
            var jobId = e.Item4;
            string userId = e.Item1.Jobs.Where(j => j.Id == jobId && j.UserId != null).Select(j => j.UserId).FirstOrDefault();

            if (userId != null)
            {
                RemoveMember(userId, (database) =>
                {
                    if (JobsContainAttachment(database, jobId, out userId) && userId != null)
                        return new string[] { userId };
                    else
                        return null;
                });
            }
        }
        #endregion

        #region User Scope
        private bool UserContainAttachment(DiscoDataContext Database, string UserId)
        {
            if (Configuration.FilterBeginDate.HasValue)
            {
                return Database.Users
                    .Where(u => u.UserId == UserId)
                    .Any(u => u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId && a.Timestamp >= Configuration.FilterBeginDate));
            }
            else
            {
                return Database.Users
                    .Where(u => u.UserId == UserId)
                    .Any(u => u.UserAttachments.Any(a => a.DocumentTemplateId == this.DocumentTemplateId));
            }
        }

        private void ProcessUserAttachmentAddEvent(RepositoryMonitorEvent e)
        {
            var attachment = (UserAttachment)e.Entity;
            var userId = attachment.UserId;

            if (UserContainAttachment(e.Database, userId) && userId != null)
                AddMember(userId, (database) => new string[] { userId });
        }
        private void ProcessUserAttachmentRemoveEvent(Tuple<DiscoDataContext, int, string, string> e)
        {
            var userId = e.Item4;

            RemoveMember(userId, (database) =>
            {
                if (!UserContainAttachment(database, userId))
                    return new string[] { userId };
                else
                    return null;
            });
        }
        #endregion

        private void ProcessDeviceAssignmentRepositoryEvent(RepositoryMonitorEvent Event)
        {
            var device = (Device)Event.Entity;
            var deviceSerialNumber = device.SerialNumber;
            bool relevantDevice;

            if (Configuration.FilterBeginDate.HasValue)
            {
                relevantDevice = Event.Database.Devices
                    .Where(d => d.SerialNumber == deviceSerialNumber && d.DeviceAttachments.Any(ja => ja.DocumentTemplateId == this.DocumentTemplateId && ja.Timestamp >= Configuration.FilterBeginDate))
                    .Any();
            }
            else
            {
                relevantDevice = Event.Database.Devices
                    .Where(d => d.SerialNumber == deviceSerialNumber && d.DeviceAttachments.Any(ja => ja.DocumentTemplateId == this.DocumentTemplateId))
                    .Any();
            }

            if (relevantDevice)
            {
                var deviceCurrentAssignedUserId = device.AssignedUserId;
                var devicePreviousAssignedUserId = Event.GetPreviousPropertyValue<string>("AssignedUserId");

                Event.ExecuteAfterCommit(e =>
                {
                    if (devicePreviousAssignedUserId != null)
                        RemoveMember(devicePreviousAssignedUserId, (database) => new string[] { devicePreviousAssignedUserId });

                    if (deviceCurrentAssignedUserId != null)
                        AddMember(deviceCurrentAssignedUserId, (database) => new string[] { deviceCurrentAssignedUserId });
                });
            }
        }

        public override void Dispose()
        {
            if (repositoryAddSubscription != null)
                repositoryAddSubscription.Dispose();

            if (repositoryRemoveSubscription != null)
                repositoryRemoveSubscription.Dispose();

            if (deviceAssignmentRepositorySubscription != null)
                deviceAssignmentRepositorySubscription.Dispose();
        }
    }
}
