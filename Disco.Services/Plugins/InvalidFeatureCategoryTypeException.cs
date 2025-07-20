using System;

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
            _categoryType = CategoryType;
            _pluginRequested = PluginRequested;
        }

        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(_pluginRequested))
                    return $"Invalid Category Type [{_categoryType.Name}]";
                else
                    return $"Plugin [{_pluginRequested}] is not of the correct Category Type [{_categoryType.Name}]";
            }
        }

    }
}
