using System;

namespace Disco.Services.Plugins.Categories.WarrantyProvider
{
    public class WarrantyProviderSubmitJobException : Exception
    {
        public WarrantyProviderSubmitJobException(string Message)
            : base(Message)
        {
        }
    }
}
