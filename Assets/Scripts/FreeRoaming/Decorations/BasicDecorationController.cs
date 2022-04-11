using System.Collections;
using UnityEngine;

namespace FreeRoaming.Decorations
{
    public class BasicDecorationController : MessageDecorationController
    {

        public string[] messages;

        protected override string[] GetMessages()
            => messages;

    }
}