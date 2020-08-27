using System;
using System.Collections.Generic;
using System.Text;

namespace Authy.AspNetCore
{
    public enum VerificationType
    {
        TOTP,
        SMS,
        VOICE,
        PUSH,
        UNKNOWN
    }
}
