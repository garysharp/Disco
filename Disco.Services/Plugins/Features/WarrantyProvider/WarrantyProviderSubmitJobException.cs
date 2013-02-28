using System;

namespace Disco.Services.Plugins.Features.WarrantyProvider
{
    public class WarrantyProviderSubmitJobException : Exception
    {
        public WarrantyProviderSubmitJobException(string Message)
            : base(Message)
        {
        }
    }
}
