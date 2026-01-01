using Disco.Data.Repository;
using Disco.Models.Repository;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disco.Services.Interop.DiscoServices
{
    public static class OnlineServicesAuthentication
    {
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static readonly Guid deploymentId;
        private static Guid? activationId;
        private static byte[] key;
        private static string token;
        private static DateTime? tokenExpires;

        static OnlineServicesAuthentication()
        {
            using (var database = new DiscoDataContext())
            {
                deploymentId = Guid.Parse(database.DiscoConfiguration.DeploymentId);
                UpdateActivation(database);
            }
        }

        public static bool IsActivated => activationId.HasValue;
        internal static byte[] Key => key.ToArray() ?? throw new InvalidOperationException("Not activated");

        public static string GetToken()
            => GetTokenAsync().Result;

        public static async Task<string> GetTokenAsync()
        {
            var localExpires = tokenExpires;
            var localToken = token;
            if (tokenExpires != null && tokenExpires.Value > DateTime.UtcNow && localToken != null)
                return localToken;

            if (!IsActivated)
                throw new InvalidOperationException("Not activated");

            await semaphore.WaitAsync();
            try
            {
                localExpires = tokenExpires;
                localToken = token;
                if (tokenExpires != null && tokenExpires.Value > DateTime.UtcNow && localToken != null)
                    return localToken;

                if (!IsActivated)
                    throw new InvalidOperationException("Not activated");

                var timeStamp = DateTime.UtcNow.ToUnixEpoc();
                var iv = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(iv);

                var dataStream = new MemoryStream(16 + 16 + 8 + iv.Length);
                dataStream.Write(deploymentId.ToByteArray(), 0, 16);
                dataStream.Write(activationId.Value.ToByteArray(), 0, 16);
                dataStream.Write(BitConverter.GetBytes(timeStamp), 0, 8);
                dataStream.Write(iv, 0, iv.Length);
                byte[] hash;
                using (var hasher = SHA256.Create())
                    hash = hasher.ComputeHash(dataStream.ToArray());

                var signature = ActivationService.SignHash(key, hash);

                var body = new AuthenticationRequest()
                {
                    DeploymentId = deploymentId,
                    ActivationId = activationId.Value,
                    TimeStamp = timeStamp,
                    IV = iv,
                    Signature = signature,
                };
                var requestJson = JsonConvert.SerializeObject(body);

                using (var request = new ByteArrayContent(Encoding.UTF8.GetBytes(requestJson)))
                {
                    request.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.BaseAddress = DiscoServiceHelpers.ActivationServiceUrl;
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await httpClient.PostAsync("/api/authenticate", request);

                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            var authResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(responseJson);

                            if (authResponse == null)
                                throw new InvalidOperationException("Failed to authenticate (empty response)");

                            if (!authResponse.Success)
                                throw new InvalidOperationException($"Failed to authenticate ({authResponse.ErrorMessage})");

                            token = authResponse.Token;
                            tokenExpires = DateTime.UtcNow.AddSeconds(authResponse.ExpiresInSeconds.Value);

                            return token;
                        }
                        else
                            throw new InvalidOperationException($"Failed to authenticate ({response.StatusCode} {response.ReasonPhrase})");
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task<Uri> CreateSession(AuthenticationSessionScope scope, User user, Uri returnUrl)
        {
            if (!IsActivated)
                throw new InvalidOperationException("Not activated");

            var request = new AuthenticationSessionRequest()
            {
                DeploymentId = deploymentId,
                ActivationId = activationId.Value,
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Scope = scope,
                UserId = user.UserId,
                UserName = user.DisplayName ?? string.Empty,
                UserEmail = user.EmailAddress,
                UserPhone = user.PhoneNumber,
                ReturnUrl = returnUrl.ToString(),
            };

            var response = await ActivationService.Post<AuthenticationSessionResponse>($"/api/authenticate/session", request);

            if (response.Success)
                return new Uri(response.Endpoint);
            else
                throw new InvalidOperationException($"Failed to create authentication session: {response.ErrorMessage}");
        }

        internal static void UpdateActivation(DiscoDataContext database)
        {
            semaphore.Wait();
            try
            {
                var config = database.DiscoConfiguration;
                if (config.IsActivated)
                {
                    activationId = config.ActivationId;
                    key = config.ActivationKey;
                    token = null;
                    tokenExpires = null;

                    OnlineServicesConnect.QueueStart();
                }
                else
                {
                    activationId = null;
                    key = null;
                    token = null;
                    tokenExpires = null;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private class AuthenticationRequest
        {
            public Guid DeploymentId { get; set; }
            public Guid ActivationId { get; set; }
            public long TimeStamp { get; set; }
            public byte[] IV { get; set; }
            public byte[] Signature { get; set; }
        }

        private class AuthenticationResponse
        {
            public bool Success { get; set; }
            public string Token { get; set; }
            public int? ExpiresInSeconds { get; set; }
            public string ErrorMessage { get; set; }
        }

        private class AuthenticationSessionRequest
        {
            public Guid DeploymentId { get; set; }
            public Guid ActivationId { get; set; }
            public long TimeStamp { get; set; }
            public AuthenticationSessionScope Scope { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string UserPhone { get; set; }
            public string ReturnUrl { get; set; }
        }

        private class AuthenticationSessionResponse
        {
            public bool Success { get; set; }
            public string Endpoint { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
