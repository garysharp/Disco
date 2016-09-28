using System;

namespace Disco.Services.Devices.Enrolment
{
    public class EnrolmentSafeException : Exception
    {
        public EnrolmentSafeException(string Message) : base(Message)
        {
        }
    }
}
