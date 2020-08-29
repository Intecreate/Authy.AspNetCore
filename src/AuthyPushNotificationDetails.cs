using System;
using System.Collections.Generic;
using System.Text;

namespace Authy.AspNetCore
{
    public class AuthyOneTouchDetails
    {
        public AuthyOneTouchDetails(bool createDictionaries = false)
        {
            if (createDictionaries)
            {
                Details = new Dictionary<string, string>();
                HiddenDetails = new Dictionary<string, string>();
                Logos = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// The message displayed to the user
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Any additional details to display to the user
        /// </summary>
        public Dictionary<string, string> Details { get; set; }

        /// <summary>
        /// Any additinal details that should be hidden from the user
        /// </summary>
        public Dictionary<string, string> HiddenDetails { get; set; }

        /// <summary>
        /// Any alternative logos to display
        /// </summary>
        public Dictionary<string, string> Logos { get; set; }

        /// <summary>
        /// The amount of seconds that the user has to interact with the OneTouch notification
        /// </summary>
        public int SecondsToExpire { get; set; }
    }
}
