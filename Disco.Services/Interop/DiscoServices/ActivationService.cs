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
        private static readonly HttpClient httpClient;
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
            httpClient = new HttpClient(new OnlineServicesAuthenticatedHandler())
            {
                BaseAddress = DiscoServiceHelpers.ActivationServiceUrl
            };
        }

        public ActivationService(DiscoDataContext database)
        {
            this.database = database;
        }

        public string GetDataStoreLocation => Path.Combine(database.DiscoConfiguration.DataStoreLocation, "Activations");
        public bool RequiresCleanup => Directory.Exists(GetDataStoreLocation);

        public Uri GetCallbackUrl()
            => new Uri(DiscoServiceHelpers.ActivationServiceUrl, "/api/callback");

        public string CalculateCallbackProof(Guid correlationId, string userId, long timestamp)
        {
            var deploymentId = Guid.Parse(database.DiscoConfiguration.DeploymentId);
            var secret = Guid.Parse(database.DiscoConfiguration.DeploymentSecret);
            using (var hmac = new HMACSHA256(secret.ToByteArray()))
            {
                var data = new MemoryStream();
                data.Write(deploymentId.ToByteArray(), 0, 16);
                data.Write(correlationId.ToByteArray(), 0, 16);
                var userIdBytes = Encoding.UTF8.GetBytes(userId);
                data.Write(BitConverter.GetBytes(userIdBytes.Length), 0, 4);
                data.Write(userIdBytes, 0, userIdBytes.Length);
                data.Write(BitConverter.GetBytes(timestamp), 0, 8);
                var hash = hmac.ComputeHash(data.ToArray());
                return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }

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

        public static async Task<T> Post<T>(string url, object request)
        {
            var stream = new MemoryStream();
            using (var jsonWriter = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, request);
            }
            stream.Position = 0;
            using (var content = new StreamContent(stream))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (var response = await httpClient.PostAsync(url, content))
                {
                    response.EnsureSuccessStatusCode();

                    using (var responseContent = await response.Content.ReadAsStreamAsync())
                    {
                        using (var reader = new StreamReader(responseContent, Encoding.UTF8))
                        {
                            using (var jsonReader = new JsonTextReader(reader))
                            {
                                var serializer = new JsonSerializer();
                                return serializer.Deserialize<T>(jsonReader);
                            }
                        }
                    }
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

            return ValidateOnlineServicesSignature(stream.ToArray(), response.Signature);
        }

        private static bool ValidateSignature(Guid activationId, byte[] challenge, byte[] challengeIv, byte[] signature)
        {
            var stream = new MemoryStream();
            stream.Write(activationId.ToByteArray(), 0, 16);
            stream.Write(challenge, 0, challenge.Length);
            stream.Write(challengeIv, 0, challengeIv.Length);
            return ValidateOnlineServicesSignature(stream.ToArray(), signature);
        }

        public static bool ValidateOnlineServicesSignature(byte[] bytes, byte[] signature)
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

        public static byte[] SignSHA256Hash(byte[] hash)
        {
            return SignHash(OnlineServicesAuthentication.Key, hash);
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

        public static byte[] Encrypt(byte[] data, out byte[] iv, out long timeStamp)
        {
            iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            timeStamp = DateTime.UtcNow.Ticks;

            return Encrypt(OnlineServicesAuthentication.Key, iv, timeStamp, data);
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

        public static byte[] Decrypt(byte[] data, byte[] iv, long timeStamp)
        {
            return Decrypt(OnlineServicesAuthentication.Key, iv, timeStamp, data);
        }

        public static byte[] OneWayDecrypt(byte[] data)
        {
            var span = data.AsSpan();

            if (span.Length < 13)
                throw new ArgumentException("Data is too short", nameof(data));
            Span<byte> magicBytes = new byte[] { 0xF0, 0x9F, 0x94, 0x8F, 0x44, 0x69, 0x73, 0x63, 0x6F, 0x49, 0x43, 0x54 }.AsSpan();
            if (!MemoryExtensions.SequenceEqual(magicBytes, span.Slice(0, 12)))
                throw new InvalidOperationException("Invalid format signature");

            span = span.Slice(12);

            byte[] readChunk(ref Span<byte> source)
            {
                var length = source[0];
                if (source.Length < (length + 1))
                    throw new ArgumentException("Data is too short", nameof(data));
                var buffer = new byte[length];
                source.Slice(1, length).CopyTo(buffer);
                source = source.Slice(length + 1);
                return buffer;
            }

            var publicKey = readChunk(ref span);
            var iv = readChunk(ref span);
            var signature = readChunk(ref span);

            var key = DeriveOneWayDecryptingKey(OnlineServicesAuthentication.Key, publicKey, iv);

            var outputStream = new MemoryStream();
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var ms = new MemoryStream(span.ToArray());
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        cs.CopyTo(outputStream);
                    }
                }
            }
            var output = outputStream.ToArray();
            if (!ValidateOnlineServicesSignature(output, signature))
                throw new InvalidOperationException("Invalid encrypted data signature");
            return output;
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

        private static byte[] DeriveOneWayDecryptingKey(byte[] privateKey, byte[] publicKey, byte[] iv)
        {
            using (var ephemeralKey = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob))
            {
                using (var clientKey = CngKey.Import(privateKey, CngKeyBlobFormat.EccPrivateBlob))
                {
                    using (var ephemeralEcdh = new ECDiffieHellmanCng(ephemeralKey))
                    {
                        using (var ecdh = new ECDiffieHellmanCng(clientKey))
                        {
                            return ecdh.DeriveKeyFromHash(ephemeralEcdh.PublicKey, HashAlgorithmName.SHA256, iv, null);
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
