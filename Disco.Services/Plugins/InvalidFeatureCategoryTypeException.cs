using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Services.Plugins
{
    public class InvalidFeatureCategoryTypeException : Exception
    {
        private string _pluginRequested;
        private Type _categoryType;

        public string PluginRequested
        {
            get
            {
                return _pluginRequested;
            }
        }
        public Type CategoryType
        {
            get
            {
                return _categoryType;
            }
        }

        public InvalidFeatureCategoryTypeException(Type CategoryType)
            : this(CategoryType, null)
        {
        }
        public InvalidFeatureCategoryTypeException(Type CategoryType, string PluginRequested)
        {
            this._categoryType = CategoryType;
            this._pluginRequested = PluginRequested;
        }

        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(_pluginRequested))
                    return string.Format("Invalid Category Type [{0}]", _categoryType.Name);
                else
                    return string.Format("Plugin [{1}] is not of the correct Category Type [{0}]", _categoryType.Name, _pluginRequested);
            }
        }

    }
}
