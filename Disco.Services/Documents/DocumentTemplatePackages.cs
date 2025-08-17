using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Documents
{
    public static class DocumentTemplatePackages
    {
        private static ConcurrentDictionary<string, DocumentTemplatePackage> cache;

        static DocumentTemplatePackages()
        {
            using (var database = new DiscoDataContext())
            {
                var packages = database.DiscoConfiguration.Documents.Packages;
                if (packages == null)
                {
                    cache = new ConcurrentDictionary<string, DocumentTemplatePackage>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    cache = new ConcurrentDictionary<string, DocumentTemplatePackage>(
                        packages.Select(p => new KeyValuePair<string, DocumentTemplatePackage>(p.Id, p)),
                        StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        public static List<DocumentTemplatePackage> GetPackages()
            => cache.Values.ToList();

        public static DocumentTemplatePackage GetPackage(string Id)
        {
            if (cache.TryGetValue(Id, out var package))
                return package;
            else
                return null;
        }

        public static IEnumerable<DocumentTemplatePackage> AvailablePackages(DiscoDataContext Database, AttachmentTypes Scope)
        {
            var packages = cache.Values.Where(p => p.Scope == Scope).ToList();
            if (packages.Count > 0)
            {
                var dbScope = Scope.ToString();
                var validTemplateIds = Database.DocumentTemplates
                    .Where(dt => dt.Scope == dbScope)
                    .Select(dt => dt.Id).ToList();

                return packages.Where(p =>
                    !p.IsHidden &&
                    p.DocumentTemplateIds != null && p.DocumentTemplateIds.Count > 0 &&
                    p.DocumentTemplateIds.Count(id => validTemplateIds.Contains(id)) > 0);
            }
            return Enumerable.Empty<DocumentTemplatePackage>();
        }

        public static List<DocumentTemplatePackage> AvailablePackages(this Device device, DiscoDataContext Database, User TechnicianUser)
        {
            var packages = new List<DocumentTemplatePackage>();

            foreach (var package in AvailablePackages(Database, AttachmentTypes.Device))
            {
                if (package.FilterExpressionMatches(device, Database, TechnicianUser, DateTime.Now, DocumentState.DefaultState()))
                {
                    packages.Add(package);
                }
            }

            return packages;
        }

        public static List<DocumentTemplatePackage> AvailablePackages(this Job job, DiscoDataContext Database, User TechnicianUser)
        {
            var packages = new List<DocumentTemplatePackage>();

            foreach (var package in AvailablePackages(Database, AttachmentTypes.Job))
            {
                bool subTypeMatch = true; // default match
                if (package.JobSubTypes != null && package.JobSubTypes.Count > 0)
                {
                    subTypeMatch = false; // enforce match
                    foreach (var subType in job.JobSubTypes)
                    {
                        if (package.JobSubTypes.Contains($"{subType.JobTypeId}_{subType.Id}", StringComparer.OrdinalIgnoreCase))
                        {
                            subTypeMatch = true;
                            break;
                        }
                    }
                }

                if (subTypeMatch)
                {
                    if (package.FilterExpressionMatches(job, Database, TechnicianUser, DateTime.Now, DocumentState.DefaultState()))
                    {
                        packages.Add(package);
                    }
                }
            }

            return packages;
        }

        public static List<DocumentTemplatePackage> AvailablePackages(this User user, DiscoDataContext Database, User TechnicianUser)
        {
            var packages = new List<DocumentTemplatePackage>();

            foreach (var package in AvailablePackages(Database, AttachmentTypes.User))
            {
                if (package.FilterExpressionMatches(user, Database, TechnicianUser, DateTime.Now, DocumentState.DefaultState()))
                {
                    packages.Add(package);
                }
            }

            return packages;
        }

        public static DocumentTemplatePackage CreatePackage(string id, string description, AttachmentTypes scope)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id), "The Package Id is required");
            if (cache.ContainsKey(id)) // Name Unique
                throw new ArgumentException("Another Package already exists with that Id", nameof(id));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description), "The Package Description is required");

            var package = new DocumentTemplatePackage()
            {
                Id = id,
                Description = description,
                Scope = scope,
            };

            if (cache.TryAdd(id, package))
            {
                PersistCache();
                return package;
            }
            else
                throw new Exception("Unable to add the Package to the Cache, check the Package Id and try again");
        }

        public static DocumentTemplatePackage UpdatePackage(DocumentTemplatePackage Package)
        {

            if (string.IsNullOrWhiteSpace(Package.Id))
                throw new ArgumentNullException(nameof(Package), "The Package Id is required");
            if (!cache.TryGetValue(Package.Id, out var existingPackage)) // Name Unique
                throw new ArgumentException("The Package Id does not exist", nameof(Package));
            if (string.IsNullOrWhiteSpace(Package.Description))
                throw new ArgumentNullException(nameof(Package), "The Package Description is required");

            if (cache.TryUpdate(Package.Id, Package, existingPackage))
            {
                PersistCache();
                return Package;
            }
            else
                throw new Exception("Unable to update the Package in the Cache, there were concurrent updates to the same package");
        }

        public static void RemovePackage(string Id)
        {
            if (cache.TryRemove(Id, out _))
            {
                PersistCache();
            }
        }

        private static void PersistCache()
        {
            var packages = cache.Values.ToList();
            if (packages.Count == 0)
                packages = null;

            using (var database = new DiscoDataContext())
            {
                database.DiscoConfiguration.Documents.Packages = packages;

                database.SaveChanges();
            }
        }

    }
}
