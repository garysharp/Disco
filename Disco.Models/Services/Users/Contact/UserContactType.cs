using System;

namespace Disco.Models.Services.Users.Contact
{
    [Flags]
    public enum UserContactType
    {
        Email = 1,
        MobilePhone,
        Phone,
        AddressMail,
        AddressHome,
    }
}
