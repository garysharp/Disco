using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web
{
    [AuthorizeDiscoUsersAttribute(Disco.Models.Repository.User.Types.Admin)]
    public class dbAdminController : dbController
    {
    }
}