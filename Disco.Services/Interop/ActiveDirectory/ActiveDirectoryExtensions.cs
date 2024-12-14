using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.NetworkInformation;

namespace Disco.Services
{
    public static class ActiveDirectoryExtensions
    {
        #region Domain/Directory Extensions

        public static bool IsReachable(this DirectoryServer ds)
        {
            using (Ping p = new Ping())
            {
                var pr = p.Send(ds.Name, 500);
                return (pr.Status == IPStatus.Success);
            }
        }

        public static IEnumerable<DomainController> WhereReachable(this DomainControllerCollection domainControllers)
        {
            return domainControllers.Cast<DomainController>().Where(dc => dc.IsReachable());
        }

        public static IEnumerable<ADDomainController> WhereReachable(this IEnumerable<ADDomainController> domainControllers)
        {
            return domainControllers.Where(dc => dc.DomainController.IsReachable());
        }

        // Directory Entry Properties (Generic Helpers)
        public static T Value<T>(this PropertyCollection properties, string PropertyName)
        {
            var p = properties.Values<T>(PropertyName);
            return p.FirstOrDefault();
        }
        public static IEnumerable<T> Values<T>(this PropertyCollection properties, string PropertyName)
        {
            var p = properties[PropertyName];
            return p.OfType<T>();
        }

        #endregion

        #region ADObject Builders

        // User Accounts
        public static ADUserAccount AsADUserAccount(this ADSearchResult SearchResult, bool Quick, string[] AdditionalProperties)
        {
            return ADUserAccount.FromSearchResult(SearchResult, Quick, AdditionalProperties);
        }
        public static IEnumerable<ADUserAccount> AsADUserAccounts(this IEnumerable<ADSearchResult> SearchResults, bool Quick, string[] AdditionalProperties)
        {
            return SearchResults.Select(sr => ADUserAccount.FromSearchResult(sr, Quick, AdditionalProperties));
        }

        // Machine Accounts
        public static ADMachineAccount AsADMachineAccount(this ADSearchResult SearchResult, string[] AdditionalProperties)
        {
            return ADMachineAccount.FromSearchResult(SearchResult, AdditionalProperties);
        }
        public static IEnumerable<ADMachineAccount> AsADMachineAccounts(this IEnumerable<ADSearchResult> SearchResults, string[] AdditionalProperties)
        {
            return SearchResults.Select(sr => ADMachineAccount.FromSearchResult(sr, AdditionalProperties));
        }

        // Groups
        public static ADGroup AsADGroup(this ADSearchResult SearchResult, string[] AdditionalProperties)
        {
            return ADGroup.FromSearchResult(SearchResult, AdditionalProperties);
        }
        public static IEnumerable<ADGroup> AsADGroups(this IEnumerable<ADSearchResult> SearchResults, string[] AdditionalProperties)
        {
            return SearchResults.Select(sr => ADGroup.FromSearchResult(sr, AdditionalProperties));
        }
        public static ADGroup AsADGroup(this ADDirectoryEntry DirectoryEntry, string[] AdditionalProperties)
        {
            return ADGroup.FromDirectoryEntry(DirectoryEntry, AdditionalProperties);
        }
        public static IEnumerable<ADGroup> AsADGroups(this IEnumerable<ADDirectoryEntry> DirectoryEntries, string[] AdditionalProperties)
        {
            return DirectoryEntries.Select(de => ADGroup.FromDirectoryEntry(de, AdditionalProperties));
        }

        // Organisational Units
        public static ADOrganisationalUnit AsADOrganisationalUnit(this ADSearchResult SearchResult)
        {
            return ADOrganisationalUnit.FromSearchResult(SearchResult);
        }
        public static IEnumerable<ADOrganisationalUnit> AsADOrganisationalUnit(this IEnumerable<ADSearchResult> SearchResults)
        {
            return SearchResults.Select(sr => ADOrganisationalUnit.FromSearchResult(sr));
        }

        public static IADObject AsADObject(this ADDirectoryEntry directoryEntry, bool quick, string[] additionalProperties)
        {
            var properties = directoryEntry.Entry.Properties;
            var objectCategory = properties.Value<string>("objectCategory");
            objectCategory = objectCategory.Substring(0, objectCategory.IndexOf(',')).ToLower();
            switch (objectCategory)
            {
                case "cn=person":
                    return ADUserAccount.FromDirectoryEntry(directoryEntry, quick, additionalProperties);
                case "cn=computer":
                    return ADMachineAccount.FromDirectoryEntry(directoryEntry, additionalProperties);
                case "cn=group":
                    return ADGroup.FromDirectoryEntry(directoryEntry, additionalProperties);
                default:
                    throw new InvalidOperationException("Unexpected objectCategory");
            }
        }

        #endregion

        public static ADUserAccount ActiveDirectoryAccount(this User User, params string[] AdditionalProperties)
        {
            return ActiveDirectory.RetrieveADUserAccount(User.UserId, AdditionalProperties);
        }

        public static ADMachineAccount ActiveDirectoryAccount(this Device Device, params string[] AdditionalProperties)
        {
            if (ActiveDirectory.IsValidDomainAccountId(Device.DeviceDomainId))
                return ActiveDirectory.RetrieveADMachineAccount(Device.DeviceDomainId, AdditionalProperties: AdditionalProperties);
            else
                return null;
        }
    }
}
