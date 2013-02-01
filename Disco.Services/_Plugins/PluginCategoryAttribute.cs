using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services.Plugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PluginCategoryAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }
}
