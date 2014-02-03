using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Models.Services.Authorization
{
    public interface IRoleToken
    {
        AuthorizationRole Role { get; set; }
        List<string> SubjectIds { get; set; }
    }
}
