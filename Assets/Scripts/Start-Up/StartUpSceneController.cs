using System.Collections;
using UnityEngine;
using FreeRoaming;

namespace StartUp
{
    public class StartUpSceneController : MonoBehaviour
    {

        [Tooltip("The scene to load once the data has been loaded")]
        public string sceneToLoad;

        public GameObject playerGameObject;
        public GameObject freeRoamMenuGameObject;

        [Tooltip("The position to start the player in in the new scene")]
        public Vector2Int playerSceneStartingPosition;

#if UNITY_EDITOR
        public GameObject[] extrasToActivate;
#endif

        private void Awake()
        {
            playerGameObject.SetActive(false);
        }

        private void Start()
        {

            LoadAllData.Load();

            //TODO - once data loading done, use loaded player data to choose which scene to open and which scenes to have in stack (and loaded). Use GameSceneManager to help with this
            //TODO - also, use loaded player data to choose player sprite (male or female)
            //TODO - also, use loaded player data to choose player starting position

#if UNITY_EDITOR
            foreach (GameObject go in extrasToActivate)
                go.SetActive(true);
#endif

            GameSceneManager.Initialise();

            GameSceneManager.OpenStartingScene(gameObject.scene, sceneToLoad, playerGameObject, freeRoamMenuGameObject, playerSceneStartingPosition);

        }

    }
}
