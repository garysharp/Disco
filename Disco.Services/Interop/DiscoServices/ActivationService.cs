using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.DiscoServices.Activation;
using Disco.Services.Interop.VicEduDept;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.DiscoServices
{
    public class ActivationService
    {
        private static readonly byte[] onlineServicesActivationKey;
        private readonly DiscoDataContext database;

        static ActivationService()
        {
            using (var resourceStream = typeof(ActivationService).Assembly.GetManifestResourceStream("DiscoIct.OnlineServices.Activation.key"))
            {
                var key = new byte[resourceStream.Length];
                resourceStream.Read(key, 0, key.Length);
                onlineServicesActivationKey = key;
            }
        }

        public ActivationService(DiscoDataContext database)
        {
            this.database = database;
        }

        public string GetDataStoreLocation => Path.Combine(database.DiscoConfiguration.DataStoreLocation, "Activations");
        public bool RequiresCleanup => Directory.Exists(GetDataStoreLocation);

        public Uri GetCallbackUrl()
            => new Uri(DiscoServiceHelpers.ActivationServiceUrl, "/api/callback");

        /// <summary>
        /// Begin the activation process
        /// </summary>
        /// <returns>A redirect URL where the user can attempt activation</returns>
        public async Task<ChallengeModel> BeginActivation(User user, string completeUrl, string finalUrl)
        {
            if (database.DiscoConfiguration.IsActivated)
                throw new InvalidOperationException("Deployment is already activated");

            // generate activation key
            var (privateKey, publicKey) = GenerateActivationKey();

            // get machine ip addresses
            var networkInterfaces = GetMachineIpAddresses();

            var vicSchool = VicSmart.WhoAmI();

            // get challenge
            ChallengeResponse challenge;
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = DiscoServiceHelpers.ActivationServiceUrl;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var body = new ChallengeRequest()
                {
                    DeploymentId = Guid.Parse(database.DiscoConfiguration.DeploymentId),
                    DeploymentVersion = typeof(ActivationService).Assembly.GetName().Version.ToString(4),
                    OrganisationName = database.DiscoConfiguration.OrganisationName,
                    PublicKey = publicKey,
                    UserId = user.UserId,
                    UserName = user.DisplayName,
                    UserEmail = user.EmailAddress,
                    CompleteUrl = completeUrl,
                    DeploymentIpAddresses = string.Join(";", networkInterfaces),
                    VicGovSchoolId = vicSchool?.Item1,
                    VicGovSchoolName = vicSchool?.Item2,
                };
                var requestJson = JsonConvert.SerializeObject(body);

                using (var request = new ByteArrayContent(Encoding.UTF8.GetBytes(requestJson)))
                {
                    request.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/api/challenge", request);

                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();

                    challenge = JsonConvert.DeserializeObject<ChallengeResponse>(responseJson);
                }
            }

            // validate signature
            if (!ValidateSignature(challenge))
                throw new InvalidOperationException("Invalid challenge signature");

            // decrypt challenge token
            var token = Decrypt(privateKey, challenge.ChallengeIv, challenge.TimeStamp, challenge.Challenge);
            if (token.Length != 32)
                throw new InvalidOperationException("Unexpected challenge length");
            // invert token
            for (int i = 0; i < token.Length; i++)
                token[i] = (byte)~token[i];
            var challengeResponseIv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(challengeResponseIv);
            // encrypt token
            var challengeResponse = Encrypt(privateKey, challengeResponseIv, challenge.TimeStamp, token);
            if (challengeResponse.Length != 48)
                throw new InvalidOperationException("Unexpected challenge response length");

            var result = new ChallengeModel
            {
                Key = privateKey,
                ActivationId = challenge.ActivationId,
                UserId = user.UserId,
                TimeStamp = challenge.TimeStamp,
                ChallengeResponse = challengeResponse,
                ChallengeResponseIv = challengeResponseIv,
                RedirectUrl = new Uri(DiscoServiceHelpers.ActivationServiceUrl, "/").ToString(),
            };

            // store activation
            var datastore = GetDataStoreLocation;
            if (!Directory.Exists(datastore))
                Directory.CreateDirectory(datastore);
            var resultJson = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
            var protectedResult = ProtectedData.Protect(resultJson, challenge.ActivationId.ToByteArray(), DataProtectionScope.LocalMachine);
            var datastoreFile = Path.Combine(datastore, $"{challenge.ActivationId:N}.bin");
            File.WriteAllBytes(datastoreFile, protectedResult);

            return result;
        }

        public async Task CompleteActivation(Guid activationId, byte[] challenge, byte[] challengeIv, byte[] signature)
        {
            if (database.DiscoConfiguration.IsActivated)
                throw new InvalidOperationException("Deployment is already activated");

            // validate signature
            if (!ValidateSignature(activationId, challenge, challengeIv, signature))
                throw new InvalidOperationException("Invalid signature");

            // load activation
            var datastoreFile = Path.Combine(GetDataStoreLocation, $"{activationId:N}.bin");
            if (!File.Exists(datastoreFile))
                throw new InvalidOperationException("Activation not found");
            var protectedActivation = File.ReadAllBytes(datastoreFile);
            var activationJson = ProtectedData.Unprotect(protectedActivation, activationId.ToByteArray(), DataProtectionScope.LocalMachine);
            var activation = JsonConvert.DeserializeObject<ChallengeModel>(Encoding.UTF8.GetString(activationJson));

            // decrypt challenge token
            var token = Decrypt(activation.Key, challengeIv, activation.TimeStamp, challenge);
            if (token.Length != 32)
                throw new InvalidOperationException("Unexpected challenge length");
            // reverse token
            Array.Reverse(token);
            var challengeResponseIv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(challengeResponseIv);
            // encrypt token
            var challengeResponse = Encrypt(activation.Key, challengeResponseIv, activation.TimeStamp, token);
            if (challengeResponse.Length != 48)
                throw new InvalidOperationException("Unexpected challenge response length");

            var responseSignature = Sign(activation.Key, activationId, challengeResponse, challengeResponseIv);

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = DiscoServiceHelpers.ActivationServiceUrl;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var body = new CompleteRequest()
                {
                    DeploymentId = Guid.Parse(database.DiscoConfiguration.DeploymentId),
                    ActivationId = activationId,
                    ChallengeResponse = challengeResponse,
                    ChallengeResponseIv = challengeResponseIv,
                    Signature = responseSignature,
                };
                var requestJson = JsonConvert.SerializeObject(body);

                using (var request = new ByteArrayContent(Encoding.UTF8.GetBytes(requestJson)))
                {
                    request.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync("/api/complete", request);

                    response.EnsureSuccessStatusCode();

                    database.DiscoConfiguration.ActivationId = activationId;
                    database.DiscoConfiguration.ActivatedOn = DateTime.Now;
                    database.DiscoConfiguration.ActivatedBy = activation.UserId;
                    database.DiscoConfiguration.ActivationKey = activation.Key;
                    database.SaveChanges();

                    OnlineServicesAuthentication.UpdateActivation(database);
                }
            }

        }

        public void CleanupExpiredActivations()
        {
            var dataStore = GetDataStoreLocation;
            if (Directory.Exists(dataStore))
            {
                if (database.DiscoConfiguration.IsActivated)
                {
                    Directory.Delete(dataStore, true);
                }
                else
                {
                    var threshold = DateTime.Now.AddDays(-14);
                    foreach (var file in Directory.EnumerateFiles(dataStore, "*.bin"))
                    {
                        if (File.GetCreationTime(file) < threshold)
                            File.Delete(file);
                    }
                    if (!Directory.EnumerateFileSystemEntries(dataStore).Any())
                        Directory.Delete(dataStore);
                }
            }
        }

        private static bool ValidateSignature(ChallengeResponse response)
        {
            var stream = new MemoryStream();
            stream.Write(response.ActivationId.ToByteArray(), 0, 16);
            stream.Write(BitConverter.GetBytes(response.TimeStamp), 0, 8);
            stream.Write(response.Challenge, 0, response.Challenge.Length);
            stream.Write(response.ChallengeIv, 0, response.ChallengeIv.Length);

            return ValidateSignature(stream.ToArray(), response.Signature);
        }

        private static bool ValidateSignature(Guid activationId, byte[] challenge, byte[] challengeIv, byte[] signature)
        {
            var stream = new MemoryStream();
            stream.Write(activationId.ToByteArray(), 0, 16);
            stream.Write(challenge, 0, challenge.Length);
            stream.Write(challengeIv, 0, challengeIv.Length);
            return ValidateSignature(stream.ToArray(), signature);
        }

        private static bool ValidateSignature(byte[] bytes, byte[] signature)
        {
            byte[] hash;
            using (var hasher = SHA256.Create())
            {
                hash = hasher.ComputeHash(bytes);
            }

            using (var serverKey = CngKey.Import(onlineServicesActivationKey, CngKeyBlobFormat.EccPublicBlob))
            {
                using (var ecdsa = new ECDsaCng(serverKey))
                {
                    ecdsa.HashAlgorithm = CngAlgorithm.Sha256;
                    return ecdsa.VerifyHash(hash, signature);
                }
            }
        }

        private static byte[] Sign(byte[] privateKey, Guid activationId, byte[] challengeResponse, byte[] challengeResponseIv)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(activationId.ToByteArray(), 0, 16);
                stream.Write(challengeResponse, 0, challengeResponse.Length);
                stream.Write(challengeResponseIv, 0, challengeResponseIv.Length);
                byte[] hash;
                using (var hasher = SHA256.Create())
                {
                    hash = hasher.ComputeHash(stream.ToArray());
                    return SignHash(privateKey, hash);
                }
            }
        }

        internal static byte[] SignHash(byte[] privateKey, byte[] hash)
        {
            using (var key = CngKey.Import(privateKey, CngKeyBlobFormat.EccPrivateBlob))
            {
                using (var ecdsa = new ECDsaCng(key))
                {
                    ecdsa.HashAlgorithm = CngAlgorithm.Sha256;
                    return ecdsa.SignHash(hash);
                }
            }
        }

        private static byte[] Encrypt(byte[] privateKey, byte[] iv, long timeStamp, byte[] data)
        {
            var key = DeriveEncryptionKey(privateKey, iv, timeStamp);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    var ms = new MemoryStream();
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static byte[] Decrypt(byte[] privateKey, byte[] iv, long timeStamp, byte[] data)
        {
            var key = DeriveEncryptionKey(privateKey, iv, timeStamp);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var ms = new MemoryStream(data);
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        var output = new MemoryStream();
                        cs.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
        }

        private static byte[] DeriveEncryptionKey(byte[] privateKey, byte[] iv, long timeStamp)
        {
            using (var serverKey = CngKey.Import(onlineServicesActivationKey, CngKeyBlobFormat.EccPublicBlob))
            {
                using (var clientKey = CngKey.Import(privateKey, CngKeyBlobFormat.EccPrivateBlob))
                {
                    using (var serverEcdh = new ECDiffieHellmanCng(serverKey))
                    {
                        using (var ecdh = new ECDiffieHellmanCng(clientKey))
                        {
                            return ecdh.DeriveKeyFromHash(serverEcdh.PublicKey, HashAlgorithmName.SHA256, iv, BitConverter.GetBytes(timeStamp));
                        }
                    }
                }
            }
        }

        private static (byte[] privateKey, byte[] publicKey) GenerateActivationKey()
        {
            using (var key = CngKey.Create(CngAlgorithm.ECDiffieHellmanP521, null, new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                KeyUsage = CngKeyUsages.KeyAgreement,
            }))
            {
                var privateKey = key.Export(CngKeyBlobFormat.EccPrivateBlob);
                var publicKey = key.Export(CngKeyBlobFormat.EccPublicBlob);

                return (privateKey, publicKey);
            }
        }

        private static List<string> GetMachineIpAddresses()
        {
            var ipAddresses = new List<string>();
            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var address in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ipAddresses.Add(address.Address.ToString());
                }
            }
            return ipAddresses;
        }

        private class ChallengeRequest
        {
            public Guid DeploymentId { get; set; }
            [MaxLength(20)]
            public string DeploymentVersion { get; set; }
            [MaxLength(200)]
            public string OrganisationName { get; set; }
            [MaxLength(158)]
            public byte[] PublicKey { get; set; }
            [StringLength(50)]
            public string UserId { get; set; }
            [StringLength(150)]
            public string UserName { get; set; }
            [StringLength(150)]
            public string UserEmail { get; set; }
            [StringLength(200)]
            public string CompleteUrl { get; set; }
            [StringLength(200)]
            public string DeploymentIpAddresses { get; set; }
            public string VicGovSchoolId { get; set; }
            [StringLength(150)]
            public string VicGovSchoolName { get; set; }
        }

        private class ChallengeResponse
        {
            public Guid ActivationId { get; set; }
            public long TimeStamp { get; set; }
            public byte[] Challenge { get; set; }
            public byte[] ChallengeIv { get; set; }
            public byte[] Signature { get; set; }
        }

        private class CompleteRequest
        {
            public Guid DeploymentId { get; set; }
            public Guid ActivationId { get; set; }
            public byte[] ChallengeResponse { get; set; }
            public byte[] ChallengeResponseIv { get; set; }
            public byte[] Signature { get; set; }
        }
    }
}
