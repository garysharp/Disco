using System;

namespace Disco.Services.Plugins.Features.InsuranceProvider
{
    public class InsuranceProviderSubmitJobException : Exception
    {
        public InsuranceProviderSubmitJobException(string message)
            : base(message)
        {
        }
    }
}
