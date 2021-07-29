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

        [Tooltip("The area's name to display to the player. If blank, won't be used")]
        public string areaName = "";

        [SerializeField]
        [Tooltip("The name of the audio music track to use for the area (should be stored in Resources/Audio/Music). If blank, won't be used")]
        private string musicTrackName = "";

        public bool hasStartingClip = true;

        public void OnTriggerEnter2D(Collider2D collision)
        {

            //Only act if it is the player that enters the area
            if (collision.GetComponent<PlayerController>() != null)
            {

                SetAsPlayerCurrentSceneArea();

                TryPlayAreaMusic();
                
                TryDisplayAreaNameSign();

            }

        }

        private void SetAsPlayerCurrentSceneArea()
        {

            PlayerController.singleton.SetCurrentSceneArea(this);

        }

        public void TryPlayAreaMusic()
        {

            if (musicTrackName != null && musicTrackName != "")
                MusicSourceController.singleton.SetTrack(musicTrackName,
                        hasStartingClip: hasStartingClip,
                        fadeTracks: true);

        }

        public void TryDisplayAreaNameSign()
        {

            if (areaName != null && areaName != "")
                AreaNameSignController.GetAreaNameSignController(gameObject.scene).DisplayAreaName(areaName);

        }

    }
}
