using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class MailboxController : MessageDecorationController
    {

        public const string messagePrefix = "";
        public const string messageSuffix = "'s House";

        public static string GenerateMessage(string ownerName)
            => messagePrefix + (ownerName != "" && ownerName != null ? ownerName : "Someone") + messageSuffix;

        public string houseOwnerName;

        protected override string[] GetMessages()
            => new string[1] { GenerateMessage(houseOwnerName) };

    }
}