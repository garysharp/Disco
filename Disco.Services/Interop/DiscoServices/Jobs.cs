﻿using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Disco.Services.Interop.DiscoServices
{
    public static class Jobs
    {
        private static string ServiceUrl(string Action)
        {
            return string.Concat(DiscoServiceHelpers.ServicesUrl, "API/Jobs/", Action);
        }

        public static PublishJobResult Publish(DiscoDataContext Database, Job Job, User TechUser, string Recipient, string RecipientReference, string Comments, List<JobAttachment> Attachments, Func<JobAttachment, DiscoDataContext, string> AttachmentFilenameRetriever)
        {
            var url = ServiceUrl("Publish");

            using (var httpClient = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(new StringContent(Database.DiscoConfiguration.DeploymentId), "DeploymentId");
                    formData.Add(new StringContent(Job.Id.ToString()), "JobId");

                    formData.Add(new StringContent(TechUser.UserId), "TechnicianId");
                    formData.Add(new StringContent(TechUser.DisplayName ?? TechUser.UserId), "TechnicianName");
                    if (!string.IsNullOrWhiteSpace(TechUser.PhoneNumber))
                        formData.Add(new StringContent(TechUser.PhoneNumber), "TechnicianPhone");
                    if (!string.IsNullOrWhiteSpace(TechUser.EmailAddress))
                        formData.Add(new StringContent(TechUser.EmailAddress), "TechnicianEmail");

                    formData.Add(new StringContent(Recipient), "Recipient");
                    if (!string.IsNullOrWhiteSpace(RecipientReference))
                        formData.Add(new StringContent(RecipientReference), "RecipientReference");

                    if (!string.IsNullOrWhiteSpace(Comments))
                        formData.Add(new StringContent(Comments), "PublishedComments");

                    if (Attachments != null && Attachments.Count > 0)
                    {
                        Attachments
                            .Select(a => new { Attachment = a, Filename = AttachmentFilenameRetriever(a, Database) })
                            .Where(a => System.IO.File.Exists(a.Filename))
                            .Select((a, i) => new { Attachment = a.Attachment, Filename = a.Filename, Index = i })
                            .ToList()
                            .ForEach(a =>
                            {
                                formData.Add(new StringContent(a.Attachment.Filename), string.Format("Attachments[{0}].Filename", a.Index));
                                formData.Add(new StringContent(a.Attachment.MimeType), string.Format("Attachments[{0}].MimeType", a.Index));
                                formData.Add(new StringContent(a.Attachment.Timestamp.ToISO8601()), string.Format("Attachments[{0}].CreatedDate", a.Index));
                                if (a.Attachment.DocumentTemplateId != null)
                                    formData.Add(new StringContent(a.Attachment.DocumentTemplateId), string.Format("Attachments[{0}].DocumentTemplateId", a.Index));
                                if (a.Attachment.Comments != null)
                                    formData.Add(new StringContent(a.Attachment.Comments), string.Format("Attachments[{0}].Comments", a.Index));

                                formData.Add(new ByteArrayContent(File.ReadAllBytes(a.Filename)), string.Format("Attachments[{0}].File", a.Index), a.Attachment.Filename);
                            });
                    }

                    var response = httpClient.PostAsync(url, formData).Result;

                    response.EnsureSuccessStatusCode();

                    var resultJson = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<PublishJobResult>(resultJson);

                    return result;
                }
            }
        }

        public static PublishJobResult UpdateRecipientReference(DiscoDataContext Database, Job Job, int PublishedJobId, string PublishedJobSecret, string RecipientReference)
        {
            var url = ServiceUrl("UpdateRecipientReference");

            using (var httpClient = new HttpClient())
            {
                using (var formData = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("PublishedJobId", PublishedJobId.ToString()),
                    new KeyValuePair<string, string>("PublishedJobSecret", PublishedJobSecret),
                    new KeyValuePair<string, string>("RecipientReference", RecipientReference)
                }))
                {
                    var response = httpClient.PostAsync(url, formData).Result;

                    response.EnsureSuccessStatusCode();

                    var resultJson = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<PublishJobResult>(resultJson);

                    return result;
                }
            }
        }

    }
}
