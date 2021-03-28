using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

namespace Testing
{
    public class TextSpeedSetting : MonoBehaviour
    {

        [Range(0,0.5F)]
        public float characterDelay = 0.1F;

        private void Update()
        {
            GameSettings.textSpeed = new GameSettings.TextSpeed("Testing Speed", characterDelay);
        }

    }
}

#endif
