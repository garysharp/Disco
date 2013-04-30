using Disco.Models.UI.Config.Enrolment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.Enrolment
{
    public class IndexModel : ConfigEnrolmentIndexModel
    {
        public string MacSshUsername { get; set; }
    }
}