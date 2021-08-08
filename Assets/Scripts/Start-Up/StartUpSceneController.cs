using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using FreeRoaming;
using Menus;
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

        [Tooltip("The scene stack to load when starting a new game")]
        public string newGameSceneStack = "Pokshire,0,0";

        public StartUpMenuController startUpMenuController;

        public GameObject playerGameObject;
        public GameObject freeRoamMenuGameObject;

        /// <summary>
        /// Game objects to put in DontDestroyOnLoad
        /// </summary>
        public GameObject[] dontDestroyOnLoadGameObjects;

        public GameObject playerPartyPokemonSetUpGameObject = null;

        private void Awake()
        {
            playerGameObject.SetActive(false);
        }

        private void Start()
        {

            HideFreeRoamMenu();

            Initialise();

            startUpMenuController.SetUp((slotIndex) => LaunchSaveSlot(slotIndex),
                () => LaunchNewGame());

        }

        private void Initialise()
        {

            foreach (GameObject go in dontDestroyOnLoadGameObjects)
                DontDestroyOnLoad(go);

            LoadAllData.Load();

            SetUpPlayer();

            SetUpFreeRoamMenu();

        }

        private void SetUpPlayer()
        {

            if (playerGameObject.GetComponent<PlayerController>() == null)
            {
                Debug.LogError("No PlayerController on playerGameObject");
                return;
            }
            else
            {
                playerGameObject.GetComponent<PlayerController>().TrySetSingleton();
            }

        }

        private void SetUpFreeRoamMenu()
        {

            if (freeRoamMenuGameObject.GetComponent<FreeRoaming.Menu.FreeRoamMenuController>() == null)
            {
                Debug.LogError("No FreeRoamMenuController on freeRoamMenuGameObject");
                return;
            }
            else
            {
                freeRoamMenuGameObject.GetComponent<FreeRoaming.Menu.FreeRoamMenuController>().TrySetSingleton();
            }

        }

        #region Launching Game

        #region Main Launching Methods

        private void LaunchNewGame()
        {

            startUpMenuController.Hide();

            StartCoroutine(NewGameCoroutine());

        }

        private void LaunchSaveSlot(int slotIndex)
        {

            startUpMenuController.Hide();

            PrepareToLaunch();

            Saving.LoadedData data = Saving.LoadData(slotIndex);

            if (data.status != Saving.LoadedData.Status.Success)
                return;

            Saving.LaunchLoadedData(data);

        }

        #endregion

        private void PrepareToLaunch()
        {

            GameSceneManager.Initialise();

            FindObjectOfType<EventSystem>().enabled = false; //This should be disabled for when the next scene is loaded

            ShowFreeRoamMenu();

        }

        private IEnumerator NewGameCoroutine()
        {

            yield return StartCoroutine(SetInitialPlayerData());

            if (!GameSceneManager.SceneStack.TryParse(newGameSceneStack, out GameSceneManager.SceneStack sceneStack, out string stackParseErrMsg))
            {
                Debug.LogError("Unable to parse provided scene stack:\n" + stackParseErrMsg);
                yield break;
            }

            LaunchSceneStack(sceneStack);

        }

        private IEnumerator SetInitialPlayerData()
        {

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

        }

        private void LaunchSceneStack(GameSceneManager.SceneStack sceneStack)
        {

            PrepareToLaunch();

            GameSceneManager.LoadSceneStack(sceneStack);

        }

        #endregion

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
