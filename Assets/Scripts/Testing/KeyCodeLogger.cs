using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Testing
{
    public class KeyCodeLogger : MonoBehaviour
    {

        private void Update()
        {
            
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {

                if (Input.GetKeyDown(keyCode))
                {
                    print("Key Down: " + keyCode);
                }

            }

        }

    }
}
