using System;

namespace Disco.Services.Interop.DiscoServices
{
    public static class DiscoServiceHelpers
    {
        public static string ServicesUrl { get; } = "https://services.discoict.com.au/";
        public static Uri ActivationServiceUrl { get; } = new Uri("https://activate.discoict.com.au");
        public static Uri UploadOnlineUrl { get; } = new Uri("https://upload.discoict.com.au");
    }
}
