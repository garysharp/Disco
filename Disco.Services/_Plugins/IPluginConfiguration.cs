using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using System.Web.Mvc;

namespace Disco.Services.Plugins
{
    public interface IPluginConfiguration
    {
        Type ConfigurationViewType { get; }
        dynamic ConfigurationViewModel(DiscoDataContext dbContext, Controller controller);
        bool ConfigurationSave(DiscoDataContext dbContext, FormCollection form, Controller controller);
    }
}
