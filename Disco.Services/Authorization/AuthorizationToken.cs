using Disco.Models.Services.Authorization;
using Disco.Models.Repository;
using Disco.Services.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Disco.Services.Authorization
{
    public class AuthorizationToken : IAuthorizationToken
    {
        public User User { get; set; }
        public List<string> GroupMembership { get; set; }
        public List<IRoleToken> RoleTokens { get; set; }

        #region Token Builders

        public static AuthorizationToken BuildToken(User User, IEnumerable<string> GroupMembership)
        {
            return new AuthorizationToken()
            {
                User = User,
                GroupMembership = GroupMembership.ToList(),
                RoleTokens = RoleCache.GetRoleTokens(GroupMembership, User)
            };
        }

        public static AuthorizationToken BuildComputerAccountToken(User User)
        {
            return new AuthorizationToken()
            {
                User = User,
                GroupMembership = new List<string>(),
                RoleTokens = new List<IRoleToken>()
                {
                    (IRoleToken)RoleCache.GetRoleToken(RoleCache.ComputerAccountTokenId)
                }
            };
        }

        #endregion

        #region Token Accessors

        internal const string RequireAuthenticationMessage = "This feature requires authentication.";
        internal const string RequireDiscoAuthorizationMessage = "Your account does not have the required permission to access this feature. This feature requires your account to be included in at least one Disco ICT Authorization Role.";
        internal const string RequireMessageTemplate = "Your account does not have the required permission to access this feature.\r\n";
        internal const string RequireMessageSingleTemplate = RequireMessageTemplate + "This feature requires the following permission:\r\n- {0}";
        internal const string RequireAllMessageTemplate = RequireMessageTemplate + "This feature requires permission for:\r\n- {0}";
        internal const string RequireAnyMessageTemplate = RequireMessageTemplate + "This feature requires at least one of these permissions:\r\n- {0}";

        internal static string BuildRequireMessage(string ClaimKey)
        {
            return string.Format(RequireMessageSingleTemplate, Claims.GetClaimDetails(ClaimKey).Item1);
        }
        internal static string BuildRequireAllMessage(IEnumerable<string> ClaimKeys)
        {
            var claimFriendlyNames = ClaimKeys.Select(ck => Claims.GetClaimDetails(ck).Item1);
            return string.Format(RequireAllMessageTemplate, string.Join("\r\n- ", claimFriendlyNames));
        }
        internal static string BuildRequireAnyMessage(IEnumerable<string> ClaimKeys)
        {
            var claimFriendlyNames = ClaimKeys.Select(ck => Claims.GetClaimDetails(ck).Item1);
            return string.Format(RequireAnyMessageTemplate, string.Join("\r\n- ", claimFriendlyNames));
        }

        /// <summary>
        /// Checks if token contains at least one of the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAny(params string[] ClaimKeys)
        {
            return HasAny((IEnumerable<string>)ClaimKeys);
        }

        /// <summary>
        /// Checks if token contains at least one of the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAny(IEnumerable<string> ClaimKeys)
        {
            return ClaimKeys.Any(ck => Has(Claims.GetClaimAccessor(ck)));
        }

        /// <summary>
        /// Checks if token contains all the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAll(params string[] ClaimKeys)
        {
            return HasAll((IEnumerable<string>)ClaimKeys);
        }
        /// <summary>
        /// Checks if token contains all the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAll(IEnumerable<string> ClaimKeys)
        {
            return ClaimKeys.All(ck => Has(Claims.GetClaimAccessor(ck)));
        }

        /// <summary>
        /// Checks if token contains the claim requested.
        /// </summary>
        /// <param name="ClaimKey">Claim Key from <see cref="Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool Has(string ClaimKey)
        {
            Func<RoleClaims, bool> claimAccessor = Claims.GetClaimAccessor(ClaimKey);

            return Has(claimAccessor);
        }

        /// <summary>
        /// Checks if token contains the claim requested.
        /// </summary>
        /// <param name="ClaimAccessor">A lambda which validates the tokens <see cref="RoleClaims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool Has(Func<RoleClaims, bool> ClaimAccessor)
        {
            return RoleTokens.Any(rt => ClaimAccessor.Invoke(((RoleToken)rt).Claims));
        }

        /// <summary>
        /// Validates the token contains the claim required. An <see cref="AccessDeniedException"/> is thrown if the requirements are not met.
        /// </summary>
        /// <param name="ClaimKey">Claim Key from <see cref="Claims"/></param>
        public void Require(string ClaimKey)
        {
            if (!Has(ClaimKey))
                throw new AccessDeniedException(BuildRequireMessage(ClaimKey), GetRequireResource());
        }

        /// <summary>
        /// Validates the token contains all the claims required. An <see cref="AccessDeniedException"/> is thrown if the requirements are not met.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Claims"/></param>
        public void RequireAll(params string[] ClaimKeys)
        {
            if (!HasAll(ClaimKeys))
                throw new AccessDeniedException(BuildRequireAllMessage(ClaimKeys), GetRequireResource());
        }

        /// <summary>
        /// Validates the token contains at least one of the claims required. An <see cref="AccessDeniedException"/> is thrown if the requirements are not met.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Claims"/></param>
        public void RequireAny(params string[] ClaimKeys)
        {
            if (!HasAny(ClaimKeys))
                throw new AccessDeniedException(BuildRequireAnyMessage(ClaimKeys), GetRequireResource());
        }

        private string GetRequireResource()
        {
            var stackTrace = new StackTrace(2, true);
            if (stackTrace.FrameCount > 1)
            {
                var frame = stackTrace.GetFrame(0);

                // Filename
                var filename = frame.GetFileName();
                if (!string.IsNullOrEmpty(filename) && filename.Contains("\\Disco\\Disco."))
                    filename = filename.Substring(filename.IndexOf("\\Disco\\Disco.") + 7);

                var method = frame.GetMethod();
                var resource = $"{method.DeclaringType.FullName}::{method.Name}";
                if (!string.IsNullOrEmpty(filename))
                    resource = $"{resource} [{filename}]";

                return resource;
            }
            return "[Unknown]";
        }

        #endregion
    }
}
