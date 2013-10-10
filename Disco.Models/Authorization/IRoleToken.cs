using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Authorization
{
    public interface IRoleToken
    {
        AuthorizationRole Role { get; set; }
        List<string> SubjectIds { get; set; }
    }
}
