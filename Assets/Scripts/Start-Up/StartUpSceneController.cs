using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using FreeRoaming;

namespace StartUp
{
    public class StartUpSceneController : MonoBehaviour
    {

        [Tooltip("The scene to load once the data has been loaded")]
        public string sceneToLoad;

        public GameObject playerGameObject;
        public GameObject freeRoamMenuGameObject;

        /// <summary>
        /// Game objects to put in DontDestroyOnLoad
        /// </summary>
        public GameObject[] dontDestroyOnLoadGameObjects;

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

            foreach (GameObject go in dontDestroyOnLoadGameObjects)
                DontDestroyOnLoad(go);

            LoadAllData.Load();

            //TODO - once data loading done, use loaded player data to choose which scene to open and which scenes to have in stack (and loaded). Use GameSceneManager to help with this
            //TODO - also, use loaded player data to choose player sprite
            //TODO - also, use loaded player data to choose player starting position and scene stack

#if UNITY_EDITOR
            foreach (GameObject go in extrasToActivate)
                go.SetActive(true);
#endif

            GameSceneManager.Initialise();

            FindObjectOfType<EventSystem>().enabled = false; //This should be disabled for when the next scene is loaded

            GameSceneManager.OpenStartingScene(gameObject.scene, sceneToLoad, playerGameObject, freeRoamMenuGameObject, playerSceneStartingPosition);

        }

    }
}
