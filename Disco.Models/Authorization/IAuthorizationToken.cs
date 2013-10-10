using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Authorization
{
    public interface IAuthorizationToken
    {
        User User { get; set; }
        List<string> GroupMembership { get; set; }
        List<IRoleToken> RoleTokens { get; set; }

        bool HasAny(params string[] ClaimKeys);
        bool HasAny(IEnumerable<string> ClaimKeys);
        bool HasAll(params string[] ClaimKeys);
        bool HasAll(IEnumerable<string> ClaimKeys);
        bool Has(string ClaimKey);

        void Require(string ClaimKey);
        void RequireAll(params string[] ClaimKeys);
        void RequireAny(params string[] ClaimKeys);
    }
}
