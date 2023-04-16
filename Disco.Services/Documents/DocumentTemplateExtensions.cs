using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Disco.Services
{
    public static class DocumentTemplateExtensions
    {

        public static Bitmap GenerateTemplatePreview(this DocumentTemplate DocumentTemplate, DiscoDataContext Database, int Width, int PageGapHeight, bool DrawPageBorder)
        {
            string filename = DocumentTemplate.RepositoryFilename(Database);

            if (File.Exists(filename))
            {

                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var pdfDocument = PdfiumViewer.PdfDocument.Load(fileStream))
                    {
                        var pageMaxWidth = (int)pdfDocument.PageSizes.Max(s => s.Width);
                        var pageScale = (float)(Width + (DrawPageBorder ? -2 : 0)) / pageMaxWidth;

                        var previewTotalHeight = pdfDocument.PageSizes
                            .Take(40)
                            .Select(s => (int)(pageScale * s.Height))
                            .Sum() +
                                (DrawPageBorder ? (Math.Min(40, pdfDocument.PageCount) * 2) : 0) +
                                ((Math.Min(40, pdfDocument.PageCount) - 1) * PageGapHeight);

                        var result = new Bitmap(Width, previewTotalHeight);
                        result.SetResolution(72, 72);

                        using (var graphics = Graphics.FromImage(result))
                        {
                            var yPosition = 0;

                            for (int pageIndex = 0; pageIndex < Math.Min(40, pdfDocument.PageCount); pageIndex++)
                            {
                                var pageSize = pdfDocument.PageSizes[pageIndex];
                                var previewWidth = Math.Floor(pageScale * pageSize.Width);
                                var previewHeight = Math.Floor(pageScale * pageSize.Height);

                                // Calculate box
                                var destination = new Rectangle(
                                    x: (int)((Width - previewWidth) / 2),
                                    y: yPosition + (DrawPageBorder ? 1 : 0),
                                    width: (int)previewWidth,
                                    height: (int)previewHeight
                                    );

                                // Fill white background
                                graphics.FillRectangle(Brushes.White, destination);

                                using (var image = pdfDocument.Render(pageIndex, (int)previewWidth, (int)previewHeight, 72F, 72F, true))
                                {
                                    graphics.DrawImage(image, destination.X, destination.Y);
                                }

                                if (DrawPageBorder)
                                {
                                    destination.X -= 1;
                                    destination.Y -= 1;
                                    destination.Width += 1;
                                    destination.Height += 1;
                                    graphics.DrawRectangle(Pens.LightGray, destination);
                                }

                                yPosition += destination.Height + PageGapHeight;
                            }
                        }

                        return result;
                    }
                }

            }

            return null;
        }

        public static IAttachmentTarget ResolveScopeTarget(this AttachmentTypes scope, DiscoDataContext database, string targetId)
            => ResolveScopeTarget(scope, database, targetId, out _);

        public static IAttachmentTarget ResolveScopeTarget(this AttachmentTypes scope, DiscoDataContext database, string targetId, out User targetUser)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrWhiteSpace(targetId))
                throw new ArgumentNullException(nameof(targetId));

            switch (scope)
            {
                case AttachmentTypes.Device:
                    var device = database.Devices.Include(d => d.AssignedUser).First(d => d.SerialNumber == targetId);
                    targetUser = device?.AssignedUser;
                    return device;
                case AttachmentTypes.Job:
                    if (!int.TryParse(targetId, out var targetIdInt))
                        throw new ArgumentOutOfRangeException(nameof(targetId));
                    var job = database.Jobs.Include(j => j.User).First(j => j.Id == targetIdInt);
                    targetUser = job?.User;
                    return job;
                case AttachmentTypes.User:
                    // special usecase in resolving users (they may not exist in the database yet)
                    targetId = ActiveDirectory.ParseDomainAccountId(targetId);
                    var user = database.Users.First(u => u.UserId == targetId);
                    if (user == null)
                    {
                        // try importing user
                        user = UserService.GetUser(targetId, database, true);
                    }
                    targetUser = user;
                    return user;
                default:
                    throw new InvalidOperationException("Unexpected DocumentType Scope");
            }
        }

        public static IAttachmentTarget ResolveScopeTarget(this DocumentTemplate template, DiscoDataContext database, string targetId)
            => ResolveScopeTarget(template, database, targetId, out _);

        public static IAttachmentTarget ResolveScopeTarget(this DocumentTemplate template, DiscoDataContext database, string targetId, out User targetUser)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            return ResolveScopeTarget(template.AttachmentType, database, targetId, out targetUser);
        }

        public static void GetTemplateAndTarget(DiscoDataContext database, AuthorizationToken authorization, string templateId, string targetId, out DocumentTemplate template, out IAttachmentTarget target, out User targetUser)
        {
            if (string.IsNullOrWhiteSpace(templateId))
                throw new ArgumentNullException(nameof(templateId));
            if (string.IsNullOrWhiteSpace(targetId))
                throw new ArgumentNullException(nameof(targetId));

            // get template
            template = database.DocumentTemplates.Find(templateId);
            if (template == null)
                throw new ArgumentException("Invalid document template id", nameof(templateId));

            // validate authorization
            switch (template.Scope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    authorization.Require(Claims.Device.Actions.GenerateDocuments);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    authorization.Require(Claims.Job.Actions.GenerateDocuments);
                    break;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    authorization.Require(Claims.User.Actions.GenerateDocuments);
                    break;
                default:
                    throw new InvalidOperationException("Unknown DocumentType Scope");
            }

            // resolve target
            target = template.ResolveScopeTarget(database, targetId, out targetUser);
            if (target == null)
                throw new ArgumentException("Target not found", nameof(targetId));
        }

        public static IEnumerable<OnImportUserFlagRule> GetOnImportUserFlagRuleDetails(this DocumentTemplate template, DiscoDataContext database)
        {
            foreach (var rule in GetOnImportUserFlagRules(template))
            {
                var detail = rule.AddDetails(database);

                if (detail == null)
                    continue;

                yield return detail;
            }
        }

        public static IEnumerable<OnImportUserFlagRule> GetOnImportUserFlagRules(this DocumentTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.OnImportUserFlagRules))
                return Enumerable.Empty<OnImportUserFlagRule>();
            else
                return JsonConvert.DeserializeObject<List<OnImportUserFlagRule>>(template.OnImportUserFlagRules);
        }

        public static OnImportUserFlagRule AddDetails(this OnImportUserFlagRule rule, DiscoDataContext database)
        {
            rule.UserFlag = database.UserFlags.FirstOrDefault(f => f.Id == rule.FlagId);

            if (rule.UserFlag == null)
                return null;
            else
                return rule;
        }

        public static OnImportUserFlagRule AddOnImportUserFlagRule(this DocumentTemplate template, DiscoDataContext database, OnImportUserFlagRule rule)
        {
            List<OnImportUserFlagRule> rules;

            if (string.IsNullOrWhiteSpace(template.OnImportUserFlagRules))
                rules = new List<OnImportUserFlagRule>();
            else
                rules = JsonConvert.DeserializeObject<List<OnImportUserFlagRule>>(template.OnImportUserFlagRules);

            // validate user flag
            rule.UserFlag = database.UserFlags.FirstOrDefault(f => f.Id == rule.FlagId);
            if (rule.UserFlag == null)
                throw new ArgumentException("Unknown rule user flag", nameof(rule));

            // validate no existing matching rule
            if (rules.Any(r => r.FlagId == rule.FlagId))
                throw new ArgumentException("This document template already has a rule for this user flag", nameof(rule));

            rule.Id = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(rule.Comments))
                rule.Comments = null;

            rules.Add(rule);
            template.OnImportUserFlagRules = JsonConvert.SerializeObject(rules);

            database.SaveChanges();

            return rule;
        }

        public static bool RemoveOnImportUserFlagRule(this DocumentTemplate template, DiscoDataContext database, Guid id)
        {
            if (string.IsNullOrWhiteSpace(template.OnImportUserFlagRules))
                return false;

            var rules = JsonConvert.DeserializeObject<List<OnImportUserFlagRule>>(template.OnImportUserFlagRules);

            if (rules.RemoveAll(r => r.Id == id) == 0)
                return false;

            template.OnImportUserFlagRules = JsonConvert.SerializeObject(rules);
            database.SaveChanges();

            return true;
        }

        public static void Apply(this OnImportUserFlagRule rule, DiscoDataContext database, IAttachmentTarget target, User techUser)
        {
            string userId;
            if (target is User targetUser)
                userId = targetUser.UserId;
            else if (target is Device targetDevice && targetDevice.AssignedUserId != null)
                userId = targetDevice.AssignedUserId;
            else if (target is Job targetJob && targetJob.UserId != null)
                userId = targetJob.UserId;
            else
                return;

            if (userId == null)
                return;

            var user = database.Users.Include(u => u.UserFlagAssignments).FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return;

            if (techUser == null)
            {
                techUser = database.Users.FirstOrDefault(u => u.UserId == rule.UserId);
                if (techUser == null)
                    return;
            }

            // remove flag
            if (!rule.AddFlag)
            {
                var flagAssignment = user.UserFlagAssignments.FirstOrDefault(a => a.RemovedDate == null && a.UserFlagId == rule.FlagId);
                if (flagAssignment != null)
                {
                    if (!string.IsNullOrWhiteSpace(rule.Comments))
                    {
                        if (!string.IsNullOrWhiteSpace(flagAssignment.Comments))
                            flagAssignment.Comments = string.Concat(flagAssignment.Comments, Environment.NewLine, rule.Comments);
                        else
                            flagAssignment.Comments = rule.Comments;
                    }
                    flagAssignment.OnRemoveUnsafe(database, techUser);
                }
                database.SaveChanges();
            }
            else
            {
                // already has flag?
                if (rule.AddFlag && user.UserFlagAssignments.Any(a => a.RemovedDate == null && a.UserFlagId == rule.FlagId))
                    return;

                // add flag
                var userFlag = database.UserFlags.FirstOrDefault(f => f.Id == rule.FlagId);
                if (userFlag == null)
                    return;

                user.OnAddUserFlagUnsafe(database, userFlag, techUser, rule.Comments);

                database.SaveChanges();
            }

        }
    }
}
