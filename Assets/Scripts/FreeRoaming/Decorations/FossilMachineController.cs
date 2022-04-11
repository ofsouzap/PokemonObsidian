using System.Collections;
using UnityEngine;

namespace FreeRoaming.Decorations
{
    public class FossilMachineController : MessageDecorationController
    {

        protected static readonly string[] messages = new string[]
        {
            "It looks like a very complicated machine.",
            "You should probably leave it alone."
        };

        protected override string[] GetMessages()
            => messages;

    }
}