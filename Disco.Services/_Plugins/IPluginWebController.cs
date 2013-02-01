using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Disco.Services.Plugins
{
    public interface IPluginWebController
    {
        ActionResult ExecuteAction(string ActionName, Controller HostController);
    }
}
