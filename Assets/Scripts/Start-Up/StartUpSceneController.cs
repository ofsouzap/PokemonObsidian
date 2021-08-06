﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using FreeRoaming;
using StartUp.ChoosePlayerData;

namespace StartUp
{
    public class StartUpSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        public bool useDefaultPlayerChoices = false;
#else
        public const bool useDefaultPlayerChoices = false;
#endif

        [Tooltip("The scene stack string to load once the data has been loaded")]
        public string startupSceneStack = "Basic Testing,0,0";

        public GameObject playerGameObject;
        public GameObject freeRoamMenuGameObject;

        /// <summary>
        /// Game objects to put in DontDestroyOnLoad
        /// </summary>
        public GameObject[] dontDestroyOnLoadGameObjects;

        public GameObject[] extrasToActivate;

        public GameObject playerPartyPokemonSetUpGameObject = null;

        private void Awake()
        {
            playerGameObject.SetActive(false);
        }

        private void Start()
        {

            StartCoroutine(StartUpCoroutine());

        }

        private IEnumerator StartUpCoroutine()
        {

            HideFreeRoamMenu();

            Initialise();

            yield return StartCoroutine(SetInitialPlayerData());

            foreach (GameObject go in extrasToActivate)
                go.SetActive(true);

            ShowFreeRoamMenu();

            LaunchGame();

            yield break;

        }

        private void Initialise()
        {

            foreach (GameObject go in dontDestroyOnLoadGameObjects)
                DontDestroyOnLoad(go);

            LoadAllData.Load();

        }

        private IEnumerator SetInitialPlayerData()
        {

            //TODO - once data loading done, use loaded player data to choose which scene to open and which scenes to have in stack (and loaded). Use GameSceneManager to help with this
            //TODO - also, use loaded player data to choose player sprite (male or female)

            if (useDefaultPlayerChoices) //Use default player data
            {

                //Game start time
                PlayerData.singleton.SetGameStartTimeAsNow();

                //Guid
                PlayerData.singleton.SetRandomGuid();

                //Name
                PlayerData.singleton.profile.name = "Player";

                //Sprite
                PlayerData.singleton.profile.spriteId = 0;

                //Party Pokemon (optionally box pokemon too)
                //Pokemon must be set after profile details so that OT details can be set
                if (playerPartyPokemonSetUpGameObject != null)
                    playerPartyPokemonSetUpGameObject.SetActive(true);

            }
            else //Have player data chosen in ChoosePlayerData scenes
            {

                yield return StartCoroutine(ChoosePlayerData());

            }
            //TODO - have branch for loading player data from save file

        }

        private void LaunchGame()
        {

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

        private void HideFreeRoamMenu() => freeRoamMenuGameObject.SetActive(false);
        private void ShowFreeRoamMenu() => freeRoamMenuGameObject.SetActive(true);

        #region Choosing Player Data

        private const string choosePlayerDataSpriteSceneIdentifier = "Choose Player Data_Sprite";
        private const string choosePlayerDataStarterPokemonSceneIdentifier = "Choose Player Data_Starter Pokemon";
        private const string choosePlayerDataNameSceneIdentifier = "Choose Player Data_Name";

        private IEnumerator ChoosePlayerData()
        {

            PlayerData.singleton.SetGameStartTimeAsNow();
            PlayerData.singleton.SetRandomGuid();
            yield return StartCoroutine(ChoosePlayerData_Name());
            yield return StartCoroutine(ChoosePlayerData_Sprite());
            yield return StartCoroutine(ChoosePlayerData_StarterPokemon()); //Pokemon must be set after profile details so that OT details can be set

        }

        private void OpenChoosePlayerDataScene(string sceneIdentifier,
            Action OnSceneCompleteAction = null)
        {

            EventSystem mainEventSystem = EventSystem.current;
            mainEventSystem.enabled = false;

            int newSceneIndex = SceneManager.sceneCount;
            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneIdentifier, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

                foreach (GameObject go in newScene.GetRootGameObjects())
                {

                    ChoosePlayerDataController cpdController = go.GetComponentInChildren<ChoosePlayerDataController>();

                    if (cpdController != null)
                    {
                        cpdController.OnSceneClose += () =>
                        {
                            OnSceneCompleteAction?.Invoke();
                            mainEventSystem.enabled = true;
                        };
                        break;
                    }

                }

            };

        }

        private IEnumerator ChoosePlayerData_Sprite()
        {

            bool spriteChosen = false;

            OpenChoosePlayerDataScene(choosePlayerDataSpriteSceneIdentifier,
                () => spriteChosen = true);

            yield return new WaitUntil(() => spriteChosen);

        }

        private IEnumerator ChoosePlayerData_StarterPokemon()
        {

            bool starterPokemonChosen = false;

            OpenChoosePlayerDataScene(choosePlayerDataStarterPokemonSceneIdentifier,
                () => starterPokemonChosen = true);

            yield return new WaitUntil(() => starterPokemonChosen);

        }

        private IEnumerator ChoosePlayerData_Name()
        {

            bool nameChosen = false;

            OpenChoosePlayerDataScene(choosePlayerDataNameSceneIdentifier,
                () => nameChosen = true);

            yield return new WaitUntil(() => nameChosen);

        }

        #endregion

    }
}
