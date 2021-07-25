using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using FreeRoaming;
using FreeRoaming.Menu;
using FreeRoaming.AreaControllers;

public static class GameSceneManager
{

    /// <summary>
    /// The identifier for the battle scene. This can be in any format that the Unity SceneManager can use
    /// </summary>
    public const string battleSceneIdentifier = "Battle";

    /// <summary>
    /// The identifier for the evolution scene. This can be in any format that the Unity SceneManager can use
    /// </summary>
    public const string evolutionSceneIdentifier = "Evolution Scene";

    public const float sceneChangePlayerMoveDelay = 0.5F;

    #region Scene Records

    /// <summary>
    /// The stack of scene identifiers and the player's old positions in them. This won't hold any details for the current scene
    /// </summary>
    private static Stack<SceneRecord> sceneRecordStack = new Stack<SceneRecord>();

    private struct SceneRecord
    {

        /// <summary>
        /// Initialises a SceneRecord with a scene and a default returnPosition
        /// </summary>
        public SceneRecord(string sceneIdentifier)
        {
            this.sceneIdentifier = sceneIdentifier;
            returnPosition = Vector2Int.zero;
        }

        public SceneRecord(string sceneIdentifier,
            Vector2Int returnPosition)
        {
            this.sceneIdentifier = sceneIdentifier;
            this.returnPosition = returnPosition;
        }

        public string sceneIdentifier;
        public Vector2Int returnPosition;

    }

    private static void AddSceneDoorToRecordStack(string sceneIdentifier, SceneDoorDetails doorDetails)
        => sceneRecordStack.Push(new SceneRecord(sceneIdentifier, doorDetails.returnPosition));

    private static Scene? pausedFreeRoamScene;

    private static Scene? battleScene;

    private static Scene? evolutionScene;

    private static Scene? playerMenuScene;

    #endregion

    public static void Initialise()
    {

        sceneRecordStack.Clear();

        if (pausedFreeRoamScene != null)
        {
            SceneManager.UnloadSceneAsync((Scene)pausedFreeRoamScene);
            pausedFreeRoamScene = null;
        }

        if (battleScene != null)
        {
            SceneManager.UnloadSceneAsync((Scene)battleScene);
            battleScene = null;
        }

        if (evolutionScene != null)
        {
            SceneManager.UnloadSceneAsync((Scene)evolutionScene);
            evolutionScene = null;
        }

        if (playerMenuScene != null)
        {
            SceneManager.UnloadSceneAsync((Scene)playerMenuScene);
            playerMenuScene = null;
        }

    }

    private static Scene CurrentScene => SceneManager.GetActiveScene();

    private static GameObject PlayerGameObject => PlayerController.singleton.gameObject;
    private static GameObject FreeRoamMenuGameObject => FreeRoamMenuController.singleton.gameObject;

    private static FreeRoamSceneController GetFreeRoamSceneController(Scene scene)
    {

        List<FreeRoamSceneController> controllers = new List<FreeRoamSceneController>();

        foreach (GameObject go in scene.GetRootGameObjects())
            controllers.AddRange(go.GetComponentsInChildren<FreeRoamSceneController>());

        switch (controllers.Count)
        {

            case 0:
                return null;

            case 1:
                return controllers[0];

            default:
                Debug.LogWarning("Multiple FreeRoamSceneController found in scene");
                return controllers[0];

        }

    }

    public delegate void OnComplete();

    #region Scene Changing

    #region Scene Fading

    private static event OnComplete FadeOutComplete;
    private static event OnComplete FadeInComplete;

    private static void StartFadeOut()
    {
        FadeOutComplete = null;
        BlackFadeController goController = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Black Fade")).GetComponent<BlackFadeController>();
        goController.FadeOut();
        goController.FadeCompleted += () => FadeOutComplete?.Invoke();
        goController.FadeCompleted += () => UnityEngine.Object.Destroy(goController.gameObject);
    }

    private static void StartFadeIn()
    {
        FadeInComplete = null;
        BlackFadeController goController = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Black Fade")).GetComponent<BlackFadeController>();
        goController.FadeIn();
        goController.FadeCompleted += () => FadeInComplete?.Invoke();
    }

    #endregion

    public static void UseDoor(SceneDoorDetails doorDetails)
    {

        if (doorDetails.isLoadingDoor)
            LoadSceneDoor(doorDetails);
        else
            AscendScene();

    }

    /// <summary>
    /// Loads a scene by its identifier, moves the player and player menu and unloads the old scene
    /// </summary>
    /// <param name="sceneIdentifier">The identifier for the scene to load</param>
    /// <param name="oldScene">The old scene to unload</param>
    /// <param name="targetPlayerPosition">The position to place the player in when the scene is loaded</param>
    /// <param name="onComplete">An action to invoke when the new scene is loaded in and the old scene has been unloaded</param>
    private static void LoadScene(string sceneIdentifier,
        Scene oldScene,
        Vector2Int targetPlayerPosition,
        Action onComplete = null)
    {

        FreeRoamSceneController oldSceneFreeRoamController = GetFreeRoamSceneController(oldScene);

        if (oldSceneFreeRoamController != null)
        {
            oldSceneFreeRoamController.SetSceneRunningState(true);
            oldSceneFreeRoamController.SetEnabledState(false);
        }

        int newSceneIndex = SceneManager.sceneCount;

        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneIdentifier, LoadSceneMode.Additive);

        loadSceneOperation.completed += (ao) =>
        {

            Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

            MovePlayerAndMenuToNewScene(PlayerGameObject,
                FreeRoamMenuGameObject,
                newScene,
                targetPlayerPosition);

            SceneManager.SetActiveScene(newScene);

            GetFreeRoamSceneController(newScene).SetSceneRunningState(false);

            SceneManager.UnloadSceneAsync(oldScene).completed += (ao) =>
            {

                GetFreeRoamSceneController(newScene).SetSceneRunningState(true);
                GetFreeRoamSceneController(newScene).SetDoorsEnabledState(true);
                StartFadeIn();

                onComplete?.Invoke();

            };

        };

    }

    private static void LoadSceneDoor(SceneDoorDetails doorDetails)
    {

        if (battleScene != null || evolutionScene != null || playerMenuScene != null)
        {
            Debug.LogError("Attempting to load scene whilst in battle/evolution/player menu scene");
            return;
        }

        Scene oldScene = CurrentScene;

        GetFreeRoamSceneController(oldScene).SetDoorsEnabledState(false);
        GetFreeRoamSceneController(oldScene).SetSceneRunningState(false);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            PlayerController.singleton.SetMoveDelay(sceneChangePlayerMoveDelay);

            if (doorDetails.isDepthLevel)
                AddSceneDoorToRecordStack(oldScene.name, doorDetails);

            LoadScene(doorDetails.sceneName, oldScene, doorDetails.newSceneTargetPosition);

        };

    }

    /// <summary>
    /// Unload the current scene and load the next scene in the scene record stack
    /// </summary>
    private static void AscendScene()
    {

        if (sceneRecordStack.Count < 1)
        {
            Debug.LogError("No scene records to load once current scene unloaded");
            return;
        }

        Scene oldScene = SceneManager.GetActiveScene();

        SceneRecord newSceneRecord = sceneRecordStack.Pop();

        LoadScene(newSceneRecord.sceneIdentifier, oldScene, newSceneRecord.returnPosition);

    }

    /// <summary>
    /// Moves the player and the free-roaming menu to a new scene and positions the player
    /// </summary>
    /// <param name="playerObject">The player's root game object</param>
    /// <param name="freeRoamMenuObject">The root game object for the free-roaming menu</param>
    /// <param name="newScene">The scene to move the player to</param>
    /// <param name="newPosition">The position to put the player in within the new scene</param>
    /// <param name="autoEnablePlayerRenderers">Whether to automatically enable any renderer components on the player that may have been disabled when a scene was hidden</param>
    private static void MovePlayerAndMenuToNewScene(GameObject playerObject,
        GameObject freeRoamMenuObject,
        Scene newScene,
        Vector2Int newPosition,
        bool autoEnablePlayerRenderers = true)
    {

        if (playerObject == null)
        {
            Debug.LogWarning("No player object set. Couldn't move player to new scene");
            return;
        }

        playerObject.GetComponent<PlayerController>().CancelMovement();
        playerObject.GetComponent<PlayerController>().SetPosition(newPosition);

        SceneManager.MoveGameObjectToScene(playerObject, newScene);
        SceneManager.MoveGameObjectToScene(freeRoamMenuObject, newScene);

        if (autoEnablePlayerRenderers)
            foreach (Renderer r in playerObject.GetComponentsInChildren<Renderer>())
                r.enabled = true;

        playerObject.SetActive(true);

        freeRoamMenuObject.SetActive(true);
        freeRoamMenuObject.GetComponent<FreeRoamMenuController>().Hide();

    }

    #region Battle Opening/Closing

    public static void LaunchBattleScene()
    {

        if (battleScene != null)
        {
            Debug.LogError("Battle trying to be launched while battle already active");
            return;
        }

        if (pausedFreeRoamScene != null)
        {
            Debug.LogError("Trying to launch battle scene while there is already a paused scene");
            return;
        }

        pausedFreeRoamScene = CurrentScene;
        FreeRoamSceneController pausedSceneController = GetFreeRoamSceneController((Scene)pausedFreeRoamScene);

        pausedSceneController.SetDoorsEnabledState(false);
        pausedSceneController.SetSceneRunningState(false);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            pausedSceneController.SetSceneRunningState(true);
            pausedSceneController.SetEnabledState(false);

            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(battleSceneIdentifier, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                battleScene = SceneManager.GetSceneAt(newSceneIndex);

                Battle.BattleManager battleManager = Battle.BattleManager.GetBattleSceneBattleManager((Scene)battleScene);

                SceneManager.SetActiveScene((Scene)battleScene);

                StartFadeIn();

                FadeInComplete += () =>
                {
                    battleManager.StartBattle();
                };

            };

        };

    }

    public static void CloseBattleScene()
    {

        if (battleScene == null)
        {
            Debug.LogError("Battle trying to be closed while battle not active");
            return;
        }

        if (pausedFreeRoamScene == null)
        {
            Debug.LogError("Trying to close battle scene while there is no paused scene to return to");
            return;
        }

        FreeRoamSceneController pausedSceneController = GetFreeRoamSceneController((Scene)pausedFreeRoamScene);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            PlayerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            SceneManager.SetActiveScene((Scene)pausedFreeRoamScene);

            //Wait until old scene unloaded before starting next scene
            SceneManager.UnloadSceneAsync((Scene)battleScene).completed += (ao) =>
            {

                pausedSceneController.SetDoorsEnabledState(true);
                pausedSceneController.SetEnabledState(true);

                battleScene = null;
                pausedFreeRoamScene = null;

                StartFadeIn();

            };

        };

    }

    #endregion

    #region Evolution Scene Opening/Closing

    public static void LaunchEvolutionScene()
    {

        //Evolution scene controller entrance arguments should have already been set before launching scene
        //Previous scene should have paused itself before launching the evolution scene

        if (evolutionScene != null)
        {
            Debug.LogError("Evolution scene trying to be launched while one already active");
            return;
        }

        EvolutionSceneClosed = null;

        Scene oldScene = CurrentScene;
        EvolutionSceneClosed += () =>
        {
            SceneManager.SetActiveScene(oldScene);
        };

        StartFadeOut();

        FadeOutComplete += () =>
        {

            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(evolutionSceneIdentifier, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                evolutionScene = SceneManager.GetSceneAt(newSceneIndex);

                EvolutionScene.EvolutionSceneController evolutionSceneController = EvolutionScene.EvolutionSceneController.GetEvolutionSceneController((Scene)evolutionScene);

                SceneManager.SetActiveScene((Scene)evolutionScene);

                StartFadeIn();

                FadeInComplete += () =>
                {

                    evolutionSceneController.StartAnimation();

                    evolutionSceneController.EvolutionAnimationComplete += () =>
                    {
                        CloseEvolutionScene();
                    };

                };

            };

        };

    }

    public static event OnComplete EvolutionSceneClosed;

    public static void CloseEvolutionScene()
    {

        if (evolutionScene == null)
        {
            Debug.LogError("Evolution scene trying to be close while isn't already active");
            return;
        }

        StartFadeOut();

        FadeOutComplete += () =>
        {

            //Wait until old scene unloaded before starting next scene
            SceneManager.UnloadSceneAsync((Scene)evolutionScene).completed += (ao) =>
            {

                evolutionScene = null;

                StartFadeIn();

                FadeInComplete += () =>
                {
                    EvolutionSceneClosed?.Invoke();
                };

            };

        };

    }

    #endregion

    #region Player Menu Scenes Opening/Closing

    public static void LaunchPlayerMenuScene(string sceneIdentifier)
    {

        if (playerMenuScene != null)
        {
            Debug.LogError("Player menu scene trying to be launched while player menu scene already active");
            return;
        }

        if (evolutionScene != null || battleScene != null)
            Debug.LogWarning("Trying to launch player menu scene whlst battle or evolution scene in use");

        if (pausedFreeRoamScene != null)
        {
            Debug.LogError("Trying to launch player menu scene while there is already a paused scene");
            return;
        }

        pausedFreeRoamScene = CurrentScene;
        FreeRoamSceneController freeRoamSceneController = GetFreeRoamSceneController((Scene)pausedFreeRoamScene);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            freeRoamSceneController.SetEnabledState(false);

            //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneIdentifier, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                playerMenuScene = SceneManager.GetSceneAt(newSceneIndex);

                SceneManager.SetActiveScene((Scene)playerMenuScene);

                StartFadeIn();

            };

        };

    }

    public static void ClosePlayerMenuScene()
    {

        if (playerMenuScene == null)
        {
            Debug.LogError("Player menu scene trying to be closed while player menu scene not already active");
            return;
        }

        if (pausedFreeRoamScene == null)
        {
            Debug.LogError("Trying to launch player menu scene while there isn't already a paused scene");
            return;
        }

        FreeRoamSceneController freeRoamSceneController = GetFreeRoamSceneController((Scene)pausedFreeRoamScene);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            SceneManager.UnloadSceneAsync((Scene)playerMenuScene).completed += (ao) =>
            {

                SceneManager.SetActiveScene((Scene)pausedFreeRoamScene);

                StartFadeIn();

                FadeInComplete += () =>
                {

                    freeRoamSceneController.SetEnabledState(true);

                };

            };

        };

    }

    #endregion

    public static void OpenStartingScene(Scene oldScene,
        string sceneName,
        Vector2Int playerSceneStartPosition)
    {

        LoadScene(sceneName, oldScene, playerSceneStartPosition);

    }

    #endregion

}