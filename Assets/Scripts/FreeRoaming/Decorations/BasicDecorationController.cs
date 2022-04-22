using System.Collections;
using UnityEngine;

namespace FreeRoaming.Decorations
{
    public sealed class BasicDecorationController : MessageDecorationController
    {

        public string[] messages;

        protected override string[] GetMessages()
            => messages;

    }
}