using System;
using System.Collections.Generic;
using System.Text;

namespace Authy.AspNetCore
{
    public class AuthyPushNotificationDetails
    {
        public AuthyPushNotificationDetails(bool createDictionaries = false)
        {
            if (createDictionaries)
            {
                Details = new Dictionary<string, string>();
                HiddenDetails = new Dictionary<string, string>();
                Logos = new Dictionary<string, string>();
            }
        }

        public string Message { get; set; }

        public Dictionary<string, string> Details { get; set; }

        public Dictionary<string, string> HiddenDetails { get; set; }

        public Dictionary<string, string> Logos { get; set; }

        public int SecondsToExpire { get; set; }
    }
}
