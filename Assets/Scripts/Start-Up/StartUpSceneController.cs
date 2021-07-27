using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using FreeRoaming;

namespace StartUp
{
    public class StartUpSceneController : MonoBehaviour
    {

        [Tooltip("The scene stack string to load once the data has been loaded")]
        public string startupSceneStack = "Basic Testing,0,0";

        public GameObject playerGameObject;
        public GameObject freeRoamMenuGameObject;

        /// <summary>
        /// Game objects to put in DontDestroyOnLoad
        /// </summary>
        public GameObject[] dontDestroyOnLoadGameObjects;

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
            //TODO - also, use loaded player data to choose player sprite (male or female)
            //TODO - also, use loaded player data to choose player starting position

#if UNITY_EDITOR
            foreach (GameObject go in extrasToActivate)
                go.SetActive(true);
#endif

            GameSceneManager.Initialise();

            FindObjectOfType<EventSystem>().enabled = false; //This should be disabled for when the next scene is loaded

            if (playerGameObject.GetComponent<PlayerController>() == null)
            {
                Debug.LogError("No PlayerController on playerGameObject");
                return;
            }
            else
            {
                playerGameObject.GetComponent<PlayerController>().TrySetSingleton();
            }

            if (freeRoamMenuGameObject.GetComponent<FreeRoaming.Menu.FreeRoamMenuController>() == null)
            {
                Debug.LogError("No FreeRoamMenuController on freeRoamMenuGameObject");
                return;
            }
            else
            {
                freeRoamMenuGameObject.GetComponent<FreeRoaming.Menu.FreeRoamMenuController>().TrySetSingleton();
            }

            if (!GameSceneManager.SceneStack.TryParse(startupSceneStack, out GameSceneManager.SceneStack sceneStack, out string stackParseErrMsg))
            {

                Debug.LogError("Unable to parse provided scene stack:\n" + stackParseErrMsg);
                return;

            }

            GameSceneManager.LoadSceneStack(sceneStack);

        }

    }
}
