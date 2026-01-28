using System;

namespace PSADTree;

[Flags]
public enum UserAccountControl : uint
{
    None                            = 0,
    SCRIPT                          = 0x00000001,   // 1
    ACCOUNTDISABLE                  = 0x00000002,   // 2
    // HOMEDIR_REQUIRED             = 0x00000008,   // Obsolete / ignored since ~2000
    LOCKOUT                         = 0x00000010,   // 16
    PASSWD_NOTREQD                  = 0x00000020,   // 32
    PASSWD_CANT_CHANGE              = 0x00000040,   // 64     (not stored, computed)
    ENCRYPTED_TEXT_PWD_ALLOWED      = 0x00000080,   // 128
    TEMP_DUPLICATE_ACCOUNT          = 0x00000100,   // 256
    NORMAL_ACCOUNT                  = 0x00000200,   // 512
    INTERDOMAIN_TRUST_ACCOUNT       = 0x00000800,   // 2048
    WORKSTATION_TRUST_ACCOUNT       = 0x00001000,   // 4096
    SERVER_TRUST_ACCOUNT            = 0x00002000,   // 8192
    DONT_EXPIRE_PASSWORD            = 0x00010000,   // 65536
    MNS_LOGON_ACCOUNT               = 0x00020000,   // 131072
    SMARTCARD_REQUIRED              = 0x00040000,   // 262144
    TRUSTED_FOR_DELEGATION          = 0x00080000,   // 524288
    NOT_DELEGATED                   = 0x00100000,   // 1048576
    USE_DES_KEY_ONLY                = 0x00200000,   // 2097152
    DONT_REQ_PREAUTH                = 0x00400000,   // 4194304
    PASSWORD_EXPIRED                = 0x00800000,   // 8388608     (computed)
    TRUSTED_TO_AUTH_FOR_DELEGATION  = 0x01000000,   // 16777216
    PARTIAL_SECRETS_ACCOUNT         = 0x04000000,   // 67108864   (RODC)
    USE_AES_KEYS                    = 0x80000000U,  // 2147483648 (AES Kerberos support)
}
