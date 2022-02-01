using System;
using System.Collections.Generic;
using UnityEngine;

namespace Testing
{
    public class TextSpeedSetting : MonoBehaviour
    {

        [Range(0,0.5F)]
        public float characterDelay = 0.1F;

        private void Update()
        {
            GameSettings.singleton.textSpeed = new GameSettings.TextSpeed("Testing Speed", characterDelay);
        }

    }
}
