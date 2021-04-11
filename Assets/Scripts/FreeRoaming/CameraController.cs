using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
{
    public class CameraController : MonoBehaviour
    {

        public static PlayerController PlayerController => PlayerController.singleton;

        [Tooltip("The angle the camera should be at in degrees")]
        public float angle = -45;

        /// <summary>
        /// How offset the camera should be on the y-axis due to its angle
        /// </summary>
        protected float yOffset;

        /// <summary>
        /// Whether the camera should follow the player. This could be set to false for special times (eg. cut scenes)
        /// </summary>
        [HideInInspector()]
        public bool followingPlayer = true;

        private void Start()
        {

            followingPlayer = true;
            yOffset = -1 * transform.position.z * Mathf.Tan(angle * (Mathf.PI / 180));

            transform.rotation = Quaternion.Euler(new Vector3(angle, 0, 0));

        }

        private void Update()
        {
            
            if (followingPlayer)
            {

                transform.position = new Vector3()
                {
                    x = PlayerController.transform.position.x,
                    y = PlayerController.transform.position.y + yOffset,
                    z = transform.position.z
                };

            }

        }

    }
}