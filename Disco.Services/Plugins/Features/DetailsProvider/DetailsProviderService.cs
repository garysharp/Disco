using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
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

        public Dictionary<string, string> GetDetails(User user)
        {
            if (user.UserDetails != null)
            {
                return user.UserDetails
                    .Where(d => string.Equals(d.Scope, DetailsScope, StringComparison.Ordinal))
                    .ToDictionary(d => d.Key, d => d.Value, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                return database.UserDetails
                    .Where(d => d.UserId == user.UserId &&
                            d.Scope == DetailsScope)
                    .ToDictionary(d => d.Key, d => d.Value, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
