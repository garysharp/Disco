using Disco.Data.Repository;
using Disco.Models.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.DiscoServices.Upload
{
    internal class UploadOnlineClient
    {
        private readonly string organisationName;
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
        public UploadOnlineClient()
        {
            using (var database = new DiscoDataContext())
                organisationName = database.DiscoConfiguration.OrganisationName;

            httpClient = new HttpClient(new OnlineServicesAuthenticatedHandler())
            {
                BaseAddress = new Uri(DiscoServiceHelpers.UploadOnlineUrl, "/api/v1/"),
            };
        }

        public async Task<(Uri sessionUri, DateTime sessionExpiration)> CreateSession(User techUser, IAttachmentTarget attachmentTarget)
        {
            var model = new CreateSessionRequestModel()
            {
                CreatedBy = techUser.UserId,
                OrganisationName = organisationName,
                TargetType = attachmentTarget.HasAttachmentType,
                TargetId = attachmentTarget.AttachmentReferenceId,
                TargetDisplayName = GetAttachmentTargetDisplayName(attachmentTarget),
            };

            var modelJson = JsonConvert.SerializeObject(model, serializerSettings);

            using (var response = await httpClient.PostAsync("session", new StringContent(modelJson, Encoding.UTF8, "application/json")))
            {
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<CreateSessionResponseModel>(responseJson, serializerSettings)
                    ?? throw new InvalidOperationException("Failed to create upload session (empty response)");

                if (!responseModel.Success)
                    throw new InvalidOperationException($"Failed to create upload session ({responseModel.ErrorMessage})");

                var expiration = DateTime.Now.AddSeconds(responseModel.ExpiresInSeconds - 10 ?? 0);
                var sessionUri = new Uri(responseModel.SessionUrl, UriKind.Absolute);

                return (sessionUri, expiration);
            }
        }

        public MemoryStream SyncUploads(string lastFileId, string hintFileId)
        {
            var response = Task.Run(() => httpClient.GetAsync($"sync?last={lastFileId}&hint={hintFileId}")).GetAwaiter().GetResult();
            try
            {
                response.EnsureSuccessStatusCode();

                if (response.StatusCode == HttpStatusCode.NoContent)
                    return null;

                var stream = new MemoryStream();

                Task.Run(() => response.Content.CopyToAsync(stream)).GetAwaiter().GetResult();

                stream.Position = 0;
                return stream;
            }
            finally
            {
                response.Dispose();
            }
        }

        private string GetAttachmentTargetDisplayName(IAttachmentTarget attachmentTarget)
        {
            switch (attachmentTarget.HasAttachmentType)
            {
                case AttachmentTypes.Device:
                    return $"Device: {attachmentTarget.AttachmentReferenceId}";
                case AttachmentTypes.Job:
                    return $"Job #{attachmentTarget.AttachmentReferenceId}";
                case AttachmentTypes.User:
                    if (attachmentTarget is User user)
                        return $"User: {user.DisplayName} ({ActiveDirectory.ActiveDirectory.FriendlyAccountId(user.UserId)})";
                    else
                        return $"User: {attachmentTarget.AttachmentReferenceId}";
                case AttachmentTypes.DeviceBatch:
                    if (attachmentTarget is DeviceBatch deviceBatch)
                        return $"Device Batch: {deviceBatch.Name} ({deviceBatch.Id})";
                    else
                        return $"Device Batch {attachmentTarget.AttachmentReferenceId}";
            }
            return $"{attachmentTarget.HasAttachmentType}: {attachmentTarget.AttachmentReferenceId}";
        }

        private class CreateSessionRequestModel
        {
            public string CreatedBy { get; set; }
            public string OrganisationName { get; set; }
            public AttachmentTypes TargetType { get; set; }
            public string TargetId { get; set; }
            public string TargetDisplayName { get; set; }
        }

        private class CreateSessionResponseModel
        {
            public bool Success { get; set; }
            public string SessionUrl { get; set; }
            public int? ExpiresInSeconds { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
