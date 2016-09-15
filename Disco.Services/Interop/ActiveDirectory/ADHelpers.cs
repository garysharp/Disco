using System;
using System.Security.Principal;
using System.Text;

namespace Disco.Services.Interop.ActiveDirectory
{
    internal static class ADHelpers
    {
        internal static byte[] ToBytes(this SecurityIdentifier Sid)
        {
            var sidBytes = new byte[Sid.BinaryLength];

            Sid.GetBinaryForm(sidBytes, 0);

            return sidBytes;
        }

        internal static SecurityIdentifier BuildPrimaryGroupSid(SecurityIdentifier UserSid, int PrimaryGroupId)
        {
            var groupSid = UserSid.ToBytes();

            int ridOffset = groupSid.Length - 4;
            int groupId = PrimaryGroupId;
            for (int i = 0; i < 4; i++)
            {
                groupSid[ridOffset + i] = (byte)(groupId & 0xFF);
                groupId >>= 8;
            }

            return new SecurityIdentifier(groupSid, 0);
        }

        internal static string ToBinaryString(this SecurityIdentifier Sid)
        {
            StringBuilder escapedSid = new StringBuilder();

            foreach (var sidByte in Sid.ToBytes())
            {
                escapedSid.Append('\\');
                escapedSid.Append(sidByte.ToString("x2"));
            }

            return escapedSid.ToString();
        }

        internal static string EscapeLdapQuery(string query)
        {
            return query.Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\\", "\\5c").Replace("NUL", "\\00").Replace("/", "\\2f");
        }

        internal static string EscapeDistinguishedName(string DistinguishedName)
        {
            if (DistinguishedName.Contains("/"))
            {
                return DistinguishedName.Replace("/", @"\/");
            }
            else
            {
                return DistinguishedName;
            }
        }

        internal static string ToLdapQueryFormat(this Guid g)
        {
            checked
            {
                StringBuilder sb = new StringBuilder();
                byte[] array = g.ToByteArray();
                for (int i = 0; i < array.Length; i++)
                {
                    byte b = array[i];
                    sb.Append("\\");
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
