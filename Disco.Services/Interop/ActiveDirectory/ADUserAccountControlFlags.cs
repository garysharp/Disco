using System;

namespace Disco.Services.Interop.ActiveDirectory
{
    [Flags]
    public enum ADUserAccountControlFlags : int
    {
        /// <summary>
        /// The logon script is executed.
        /// </summary>
        ADS_UF_SCRIPT = 0x00000001,
        /// <summary>
        /// The user account is disabled.
        /// </summary>
        ADS_UF_ACCOUNTDISABLE = 0x00000002,
        /// <summary>
        /// The home directory is required.
        /// </summary>
        ADS_UF_HOMEDIR_REQUIRED = 0x00000008,
        /// <summary>
        /// The account is currently locked out.
        /// </summary>
        ADS_UF_LOCKOUT = 0x00000010,
        /// <summary>
        /// No password is required.
        /// </summary>
        ADS_UF_PASSWD_NOTREQD = 0x00000020,
        /// <summary>
        /// The user cannot change the password. Note: You cannot assign the permission settings of PASSWD_CANT_CHANGE by directly modifying the UserAccountControl attribute.
        /// </summary>
        ADS_UF_PASSWD_CANT_CHANGE = 0x00000040,
        /// <summary>
        /// The user can send an encrypted password.
        /// </summary>
        ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x00000080,
        /// <summary>
        /// This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not to any domain that trusts this domain. Also known as a local user account.
        /// </summary>
        ADS_UF_TEMP_DUPLICATE_ACCOUNT = 0x00000100,
        /// <summary>
        /// This is a default account type that represents a typical user.
        /// </summary>
        ADS_UF_NORMAL_ACCOUNT = 0x00000200,
        /// <summary>
        /// This is a permit to trust account for a system domain that trusts other domains.
        /// </summary>
        ADS_UF_INTERDOMAIN_TRUST_ACCOUNT = 0x00000800,
        /// <summary>
        /// This is a computer account for a computer that is a member of this domain.
        /// </summary>
        ADS_UF_WORKSTATION_TRUST_ACCOUNT = 0x00001000,
        /// <summary>
        /// This is a computer account for a system backup domain controller that is a member of this domain.
        /// </summary>
        ADS_UF_SERVER_TRUST_ACCOUNT = 0x00002000,
        /// <summary>
        /// The password for this account will never expire.
        /// </summary>
        ADS_UF_DONT_EXPIRE_PASSWD = 0x00010000,
        /// <summary>
        /// This is an MNS logon account.
        /// </summary>
        ADS_UF_MNS_LOGON_ACCOUNT = 0x00020000,
        /// <summary>
        /// The user must log on using a smart card.
        /// </summary>
        ADS_UF_SMARTCARD_REQUIRED = 0x00040000,
        /// <summary>
        /// The service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. Any such service can impersonate a client requesting the service.
        /// </summary>
        ADS_UF_TRUSTED_FOR_DELEGATION = 0x00080000,
        /// <summary>
        /// The security context of the user will not be delegated to a service even if the service account is set as trusted for Kerberos delegation.
        /// </summary>
        ADS_UF_NOT_DELEGATED = 0x00100000,
        /// <summary>
        /// Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys.
        /// </summary>
        ADS_UF_USE_DES_KEY_ONLY = 0x00200000,
        /// <summary>
        /// This account does not require Kerberos pre-authentication for logon.
        /// </summary>
        ADS_UF_DONT_REQUIRE_PREAUTH = 0x00400000,
        /// <summary>
        /// The user password has expired. This flag is created by the system using data from the Pwd-Last-Set attribute and the domain policy.
        /// </summary>
        ADS_UF_PASSWORD_EXPIRED = 0x00800000,
        /// <summary>
        /// The account is enabled for delegation. This is a security-sensitive setting; accounts with this option enabled should be strictly controlled. This setting enables a service running under the account to assume a client identity and authenticate as that user to other remote servers on the network.
        /// </summary>
        ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x01000000
    }
}
