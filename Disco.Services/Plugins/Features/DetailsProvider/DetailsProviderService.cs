using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Plugins.Details;
using Disco.Services.Authorization;
using Disco.Services.Users;
using Exceptionless.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Disco.Services.Plugins.Features.DetailsProvider
{
    public class DetailsProviderService
    {
        private const string DetailsScope = "Details";
        private readonly DiscoDataContext database;

        public DetailsProviderService(DiscoDataContext database)
        {
            this.database = database;
        }

        public bool HasUserPhoto(User user)
        {
            var cachePath = GetUserPhotoCachePath(user);
            if (File.Exists(cachePath))
                return true;

            // slow-path: this should only happen once,
            //  the first time, before we cache
            var photo = GetUserPhoto(user);

            return photo != null;
        }

        public byte[] GetUserPhoto(User user)
        {
            var cachePath = GetUserPhotoCachePath(user);
            var cacheAge = default(DateTime?);
            if (File.Exists(cachePath))
                cacheAge = File.GetLastWriteTime(cachePath);

            var features = Plugins.GetPluginFeatures(typeof(DetailsProviderFeature));

            foreach (var feature in features)
            {
                var instance = feature.CreateInstance<DetailsProviderFeature>();
                var result = instance.GetUserPhoto(database, user, cacheAge);
                if (result != null)
                {
                    // resize image
                    using (var originalStream = new MemoryStream(result))
                    {
                        using (var originalImage = Image.FromStream(originalStream))
                        {
                            using (var resizedImage = originalImage.ResizeImage(192, Brushes.White))
                            {
                                using (var savedResizedImage = (MemoryStream)resizedImage.SaveJpg(85))
                                {
                                    result = savedResizedImage.ToArray();
                                }
                            }
                        }
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                    File.WriteAllBytes(cachePath, result);
                    return result;
                }
            }

            // serve from cache
            if (cacheAge.HasValue)
                return File.ReadAllBytes(cachePath);

            return null;
        }

        private string GetUserPhotoCachePath(User user)
        {
            var hasher = new SHA1Managed();
            var userHash = BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(user.UserId))).Replace("-", string.Empty);
            return Path.Combine(database.DiscoConfiguration.PluginUserPhotosLocation, userHash.Substring(0, 2), $"{userHash}.jpg");
        }

        public DetailsResult GetDetails(User user)
        {
            var result = new DetailsResult();
            var saveChangesRequired = false;

            if (!UserService.CurrentAuthorization.HasAll(Claims.User.Show, Claims.User.ShowDetails))
                return result;

            var features = Plugins.GetPluginFeatures(typeof(DetailsProviderFeature));

            if (features.Count == 0)
                return result;

            var cache = user.UserDetails?.Where(d => d.Scope == DetailsScope).ToDictionary(d => d.Key, d => new { DbDetails = d, Details = JsonConvert.DeserializeObject<DetailsResult>(d.Value) }, StringComparer.OrdinalIgnoreCase);

            foreach (var feature in features)
            {
                var featureResult = default(DetailsResult);
                if (!cache.TryGetValue(feature.Id, out var cacheResult) || cacheResult.Details.ExpiresOn < DateTime.Now || cacheResult.Details.GatheredOn < database.DiscoConfiguration.PluginDetailsCacheExpiration)
                {
                    var timestamp = cacheResult?.Details.GatheredOn;
                    if (timestamp.HasValue && timestamp.Value < database.DiscoConfiguration.PluginDetailsCacheExpiration)
                        timestamp = null;

                    try
                    {
                        var featureInstance = feature.CreateInstance<DetailsProviderFeature>();
                        featureResult = featureInstance.GetDetails(database, user, timestamp);

                        if (featureResult != null)
                        {
                            if (featureResult.ExpiresOn > DateTime.Now)
                            {
                                if (cacheResult == null)
                                    database.UserDetails.Add(new UserDetail() { UserId = user.UserId, Scope = DetailsScope, Key = feature.Id, Value = JsonConvert.SerializeObject(featureResult) });
                                else
                                    cacheResult.DbDetails.Value = JsonConvert.SerializeObject(featureResult);
                                saveChangesRequired = true;
                            }
                            else if (cacheResult != null)
                            {
                                database.UserDetails.Remove(cacheResult.DbDetails);
                                saveChangesRequired = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore exceptions when plugins behave badly
                    }
                }
                else
                {
                    featureResult = cacheResult.Details;
                }

                // apply feature results
                if (featureResult != null)
                {
                    result.SetExpiration(featureResult.ExpiresOn);
                    foreach (var value in featureResult.Details)
                    {
                        result.Details[value.Key] = value.Value;
                    }
                }
            }

            if (saveChangesRequired)
                database.SaveChanges();

            return result;
        }

        public DetailsResult GetDetails(Device device)
        {
            var result = new DetailsResult();
            var saveChangesRequired = false;

            if (!UserService.CurrentAuthorization.HasAll(Claims.Device.Show, Claims.Device.ShowDetails))
                return result;

            var features = Plugins.GetPluginFeatures(typeof(DetailsProviderFeature));

            if (features.Count == 0)
                return result;

            var cache = device.DeviceDetails?.Where(d => d.Scope == DetailsScope).ToDictionary(d => d.Key, d => new { DbDetails = d, Details = JsonConvert.DeserializeObject<DetailsResult>(d.Value) }, StringComparer.OrdinalIgnoreCase);

            foreach (var feature in features)
            {
                var featureResult = default(DetailsResult);
                if (!cache.TryGetValue(feature.Id, out var cacheResult) || cacheResult.Details.ExpiresOn < DateTime.Now || cacheResult.Details.GatheredOn < database.DiscoConfiguration.PluginDetailsCacheExpiration)
                {
                    var timestamp = cacheResult?.Details.GatheredOn;
                    if (timestamp.HasValue && timestamp.Value < database.DiscoConfiguration.PluginDetailsCacheExpiration)
                        timestamp = null;

                    try
                    {
                        var featureInstance = feature.CreateInstance<DetailsProviderFeature>();
                        featureResult = featureInstance.GetDetails(database, device, timestamp);

                        if (featureResult != null)
                        {
                            if (featureResult.ExpiresOn > DateTime.Now)
                            {
                                if (cacheResult == null)
                                    database.DeviceDetails.Add(new DeviceDetail() { DeviceSerialNumber = device.SerialNumber, Scope = DetailsScope, Key = feature.Id, Value = JsonConvert.SerializeObject(featureResult) });
                                else
                                    cacheResult.DbDetails.Value = JsonConvert.SerializeObject(featureResult);
                                saveChangesRequired = true;
                            }
                            else if (cacheResult != null)
                            {
                                database.DeviceDetails.Remove(cacheResult.DbDetails);
                                saveChangesRequired = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore exceptions when plugins behave badly
                    }
                }
                else
                {
                    featureResult = cacheResult.Details;
                }

                // apply feature results
                if (featureResult != null)
                {
                    result.SetExpiration(featureResult.ExpiresOn);
                    foreach (var value in featureResult.Details)
                    {
                        result.Details[value.Key] = value.Value;
                    }
                }
            }

            if (saveChangesRequired)
                database.SaveChanges();

            return result;
        }
    }
}
