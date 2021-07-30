using System;
using UnityEngine;
using Audio;

namespace FreeRoaming
{
    /// <summary>
    /// A controller for areas of scenes e.g. towns, routes
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneAreaController : MonoBehaviour
    {

        private SceneArea? area;

        public int id = 0;

        private void Start()
        {

            if (id != 0)
                area = SceneArea.registry.LinearSearch(id);
            else
                area = null;

        }

        public void OnTriggerEnter2D(Collider2D collision)
        {

            //Only act if it is the player that enters the area
            if (collision.GetComponent<PlayerController>() != null)
            {

                if (area != null)
                {

                    if (TrySetAsPlayerCurrentSceneArea())
                    {

                        area?.TryPlayAreaMusic();

                        area?.TryDisplayAreaNameSign(gameObject.scene);

                    }

                }

            }

        }

        private bool TrySetAsPlayerCurrentSceneArea()
        {

            return PlayerController.singleton.TrySetCurrentSceneArea((SceneArea)area);

        }

    }
}
