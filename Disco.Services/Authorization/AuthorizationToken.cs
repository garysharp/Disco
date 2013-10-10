using Disco.Models.Authorization;
using Disco.Models.Repository;
using Disco.Services.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization
{
    public class AuthorizationToken : IAuthorizationToken
    {
        public User User { get; set; }
        public List<string> GroupMembership { get; set; }
        public List<IRoleToken> RoleTokens { get; set; }

        #region Token Builders

        public static AuthorizationToken BuildToken(User User, List<string> GroupMembership)
        {
            return new AuthorizationToken()
            {
                User = User,
                GroupMembership = GroupMembership,
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

        internal const string RequireAuthenticationMessage = "This Disco feature requires authentication.";
        internal const string RequireDiscoAuthorizationMessage = "Your account does not have the required permission to access this Disco feature. This feature requires your account to be included in at least one Disco Authorization Role.";
        internal const string RequireMessageTemplate = "Your account does not have the required permission ({0}) to access this Disco feature.";
        internal const string RequireAllMessageTemplate = "Your account does not have the required permission to access this Disco feature. This feature requires permission for: {0}.";
        internal const string RequireAnyMessageTemplate = "Your account does not have the required permission to access this Disco feature. This feature requires at least one of these permissions: {0}.";

        internal static string BuildRequireMessage(string ClaimKey)
        {
            return string.Format(RequireMessageTemplate, Claims.GetClaimDetails(ClaimKey).Item1);
        }
        internal static string BuildRequireAllMessage(IEnumerable<string> ClaimKeys)
        {
            var claimFriendlyNames = ClaimKeys.Select(ck => Claims.GetClaimDetails(ck).Item1);
            return string.Format(RequireAllMessageTemplate, string.Join("; ", claimFriendlyNames));
        }
        internal static string BuildRequireAnyMessage(IEnumerable<string> ClaimKeys)
        {
            var claimFriendlyNames = ClaimKeys.Select(ck => Claims.GetClaimDetails(ck).Item1);
            return string.Format(RequireAnyMessageTemplate, string.Join("; ", claimFriendlyNames));
        }

        /// <summary>
        /// Checks if token contains at least one of the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Disco.Services.Authorization.Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAny(params string[] ClaimKeys)
        {
            return HasAny((IEnumerable<string>)ClaimKeys);
        }
        
        /// <summary>
        /// Checks if token contains at least one of the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Disco.Services.Authorization.Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAny(IEnumerable<string> ClaimKeys)
        {
            return ClaimKeys.Any(ck => Has(Claims.GetClaimAccessor(ck)));
        }

        /// <summary>
        /// Checks if token contains all the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Disco.Services.Authorization.Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAll(params string[] ClaimKeys)
        {
            return HasAll((IEnumerable<string>)ClaimKeys);
        }
        /// <summary>
        /// Checks if token contains all the claims requested.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Disco.Services.Authorization.Claims"/></param>
        /// <returns>true if the token satisfies the claim request, otherwise false.</returns>
        public bool HasAll(IEnumerable<string> ClaimKeys)
        {
            return ClaimKeys.All(ck => Has(Claims.GetClaimAccessor(ck)));
        }

        /// <summary>
        /// Checks if token contains the claim requested.
        /// </summary>
        /// <param name="ClaimKey">Claim Key from <see cref="Disco.Services.Authorization.Claims"/></param>
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
        /// Validates the token contains the claim required. An <see cref="Disco.Services.Authorization.AccessDeniedException"/> is thrown if the requirements are not met.
        /// </summary>
        /// <param name="ClaimKey">Claim Key from <see cref="Disco.Services.Authorization.Claims"/></param>
        public void Require(string ClaimKey)
        {
            if (!Has(ClaimKey))
                throw new AccessDeniedException(BuildRequireMessage(ClaimKey));
        }

        /// <summary>
        /// Validates the token contains all the claims required. An <see cref="Disco.Services.Authorization.AccessDeniedException"/> is thrown if the requirements are not met.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Disco.Services.Authorization.Claims"/></param>
        public void RequireAll(params string[] ClaimKeys)
        {
            if (!HasAll(ClaimKeys))
                throw new AccessDeniedException(BuildRequireAllMessage(ClaimKeys));
        }
        
        /// <summary>
        /// Validates the token contains at least one of the claims required. An <see cref="Disco.Services.Authorization.AccessDeniedException"/> is thrown if the requirements are not met.
        /// </summary>
        /// <param name="ClaimKeys">Claim Keys from <see cref="Disco.Services.Authorization.Claims"/></param>
        public void RequireAny(params string[] ClaimKeys)
        {
            if (!HasAny(ClaimKeys))
                throw new AccessDeniedException(BuildRequireAnyMessage(ClaimKeys));
        }

        #endregion
    }
}
