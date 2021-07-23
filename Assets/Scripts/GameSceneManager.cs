using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    /// The stack of scenes and the player's old positions in them
    /// </summary>
    private static Stack<SceneRecord> sceneRecordStack = new Stack<SceneRecord>();

    public static Scene CurrentFreeRoamScene => sceneRecordStack.Peek().scene;

    /// <summary>
    /// A struct describing the past scenes a player might return to (or the scene they are currently in) and the position that they should return to
    /// </summary>
    public struct SceneRecord
    {

        /// <summary>
        /// Initialises a SceneRecord with a scene and a default gridPosition
        /// </summary>
        public SceneRecord(Scene scene)
        {
            this.scene = scene;
            gridPosition = Vector2Int.zero;
        }

        public SceneRecord(Scene scene,
            Vector2Int gridPosition)
        {
            this.scene = scene;
            this.gridPosition = gridPosition;
        }

        /// <summary>
        /// The scene that might be returned to
        /// </summary>
        public Scene scene;

        /// <summary>
        /// The position to possibly return to. If the player is currently in this scene, this is used
        /// </summary>
        public Vector2Int gridPosition;

    }

    private static bool battleSceneInUse;
    /// <summary>
    /// The battle scene if in use
    /// </summary>
    private static Scene battleScene;

    private static bool evolutionSceneInUse;
    private static Scene evolutionScene;

    private static bool playerMenuSceneInUse;
    private static Scene playerMenuScene;

    #endregion

    public static void Initialise()
    {
            
        sceneRecordStack.Clear();
        battleSceneInUse = false;

    }

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
                Debug.LogWarning("Unable to find FreeRoamSceneController in scene");
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

    private static void RefreshCurrentSceneStack()
    {
        if (sceneRecordStack.Count == 0)
        {
            sceneRecordStack.Push(new SceneRecord(SceneManager.GetActiveScene()));
        }
    }

    public static void UseDoor(SceneDoorDetails doorDetails)
    {

        RefreshCurrentSceneStack();

        if (doorDetails.isLoadingDoor)
            LoadScene(doorDetails);
        else
            ExitDepthLevelScene();

    }

    private static void LoadScene(SceneDoorDetails doorDetails)
    {
        if (doorDetails.isDepthLevel)
            LoadDepthLevelScene(doorDetails);
        else
            LoadParallelLevelScene(doorDetails);
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

        if (freeRoamMenuObject != null)
            SceneManager.MoveGameObjectToScene(freeRoamMenuObject, newScene);

        if (autoEnablePlayerRenderers)
            foreach (Renderer r in playerObject.GetComponentsInChildren<Renderer>())
                r.enabled = true;

        playerObject.SetActive(true);

        if (freeRoamMenuObject != null)
        {
            freeRoamMenuObject.SetActive(true);
            freeRoamMenuObject.GetComponent<FreeRoamMenuController>().Hide();
        }

    }

    private static void LoadParallelLevelScene(SceneDoorDetails doorDetails)
    {

        if (battleSceneInUse)
        {
            Debug.LogError("Attempting to load scene whilst in battle");
            return;
        }

        Scene oldScene = sceneRecordStack.Pop().scene;

        GetFreeRoamSceneController(oldScene).SetDoorsEnabledState(false);
        GetFreeRoamSceneController(oldScene).SetSceneRunningState(false);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            GetFreeRoamSceneController(oldScene).SetSceneRunningState(true);
            GetFreeRoamSceneController(oldScene).SetEnabledState(false);

            PlayerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(doorDetails.sceneName, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

                sceneRecordStack.Push(new SceneRecord(newScene));

                MovePlayerAndMenuToNewScene(PlayerGameObject, FreeRoamMenuGameObject, newScene, doorDetails.newSceneTargetPosition);

                SceneManager.SetActiveScene(newScene);

                GetFreeRoamSceneController(newScene).SetSceneRunningState(false);

                SceneManager.UnloadSceneAsync(oldScene).completed += (ao) =>
                {
                    GetFreeRoamSceneController(newScene).SetSceneRunningState(true);
                    GetFreeRoamSceneController(newScene).SetDoorsEnabledState(true);
                    StartFadeIn();
                };

            };

        };

    }

    private static void LoadDepthLevelScene(SceneDoorDetails doorDetails)
        => LoadDepthLevelScene(doorDetails.sceneName, doorDetails.returnPosition, doorDetails.newSceneTargetPosition);

    private static void LoadDepthLevelScene(string sceneName,
        Vector2Int returnPosition,
        Vector2Int newSceneTargetPosition,
        Action onCompleteAction = null)
    {

        if (battleSceneInUse)
        {
            Debug.LogError("Attempting to load scene whilst in battle");
            return;
        }

        Scene oldScene = sceneRecordStack.Pop().scene;

        GetFreeRoamSceneController(oldScene).SetDoorsEnabledState(false);
        GetFreeRoamSceneController(oldScene).SetSceneRunningState(false);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            GetFreeRoamSceneController(oldScene).SetSceneRunningState(true);
            GetFreeRoamSceneController(oldScene).SetEnabledState(false);

            PlayerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            sceneRecordStack.Push(new SceneRecord(oldScene, returnPosition));

            //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

                sceneRecordStack.Push(new SceneRecord(newScene));

                MovePlayerAndMenuToNewScene(PlayerGameObject, FreeRoamMenuGameObject, newScene, newSceneTargetPosition);
                SceneManager.SetActiveScene(newScene);

                StartFadeIn();
                GetFreeRoamSceneController(newScene).SetDoorsEnabledState(true);

                onCompleteAction?.Invoke();

            };

        };

    }

    private static void ExitDepthLevelScene()
    {

        SceneRecord oldSceneRecord = sceneRecordStack.Pop();
        SceneRecord newSceneRecord = sceneRecordStack.Peek();

        GetFreeRoamSceneController(oldSceneRecord.scene).SetDoorsEnabledState(false);
        GetFreeRoamSceneController(oldSceneRecord.scene).SetSceneRunningState(false);

        StartFadeOut();

        FadeOutComplete += () => {

            GetFreeRoamSceneController(oldSceneRecord.scene).SetSceneRunningState(true);
            GetFreeRoamSceneController(oldSceneRecord.scene).SetEnabledState(false);

            PlayerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            MovePlayerAndMenuToNewScene(PlayerGameObject, FreeRoamMenuGameObject, newSceneRecord.scene, newSceneRecord.gridPosition);

            SceneManager.SetActiveScene(newSceneRecord.scene);

            //Wait until old scene unloaded before starting next scene
            SceneManager.UnloadSceneAsync(oldSceneRecord.scene).completed += (ao) =>
            {
                GetFreeRoamSceneController(newSceneRecord.scene).SetDoorsEnabledState(true);
                GetFreeRoamSceneController(newSceneRecord.scene).SetEnabledState(true);
                StartFadeIn();
            };

        };

    }

    #region Battle Opening/Closing

    public static void LaunchBattleScene()
    {

        RefreshCurrentSceneStack();

        if (battleSceneInUse)
        {
            Debug.LogError("Battle trying to be launched while battle already active");
            return;
        }

        Scene freeRoamScene = sceneRecordStack.Peek().scene;
        FreeRoamSceneController freeRoamSceneController = GetFreeRoamSceneController(freeRoamScene);

        freeRoamSceneController.SetDoorsEnabledState(false);
        freeRoamSceneController.SetSceneRunningState(false);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            freeRoamSceneController.SetSceneRunningState(true);
            freeRoamSceneController.SetEnabledState(false);

            //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(battleSceneIdentifier, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                battleScene = SceneManager.GetSceneAt(newSceneIndex);
                battleSceneInUse = true;

                Battle.BattleManager battleManager = Battle.BattleManager.GetBattleSceneBattleManager(battleScene);

                SceneManager.SetActiveScene(battleScene);

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

        if (!battleSceneInUse)
        {
            Debug.LogError("Battle trying to be closed while battle not already active");
            return;
        }

        Scene freeRoamScene = sceneRecordStack.Peek().scene;
        FreeRoamSceneController freeRoamSceneController = GetFreeRoamSceneController(freeRoamScene);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            PlayerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            battleSceneInUse = false;

            SceneManager.SetActiveScene(freeRoamScene);

            //Wait until old scene unloaded before starting next scene
            SceneManager.UnloadSceneAsync(battleScene).completed += (ao) =>
            {

                freeRoamSceneController.SetDoorsEnabledState(true);
                freeRoamSceneController.SetEnabledState(true);
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

        RefreshCurrentSceneStack();

        if (evolutionSceneInUse)
        {
            Debug.LogError("Evolution scene trying to be launched while evolution scene already active");
            return;
        }

        EvolutionSceneClosed = null;

        Scene oldActiveScene = SceneManager.GetActiveScene();
        EvolutionSceneClosed += () =>
        {
            SceneManager.SetActiveScene(oldActiveScene);
        };

        StartFadeOut();

        FadeOutComplete += () =>
        {

            //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
            int newSceneIndex = SceneManager.sceneCount;

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(evolutionSceneIdentifier, LoadSceneMode.Additive);

            loadSceneOperation.completed += (ao) =>
            {

                evolutionScene = SceneManager.GetSceneAt(newSceneIndex);
                evolutionSceneInUse = true;

                EvolutionScene.EvolutionSceneController evolutionSceneController = EvolutionScene.EvolutionSceneController.GetEvolutionSceneController(evolutionScene);

                SceneManager.SetActiveScene(evolutionScene);

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

        //Instead of having the GameSceneManager re-enable a scene once evolution scene closed, the scene being returned to should add a handler to EvolutionSceneClosed and resume once this is invoked

        if (!evolutionSceneInUse)
        {
            Debug.LogError("Evolution scene trying to be closed while evolution scene not already active");
            return;
        }

        StartFadeOut();

        FadeOutComplete += () =>
        {

            evolutionSceneInUse = false;

            //Wait until old scene unloaded before starting next scene
            SceneManager.UnloadSceneAsync(evolutionScene).completed += (ao) =>
            {

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

        RefreshCurrentSceneStack();

        if (playerMenuSceneInUse)
        {
            Debug.LogError("Player menu scene trying to be launched while player menu scene already active");
            return;
        }

        if (evolutionSceneInUse || battleSceneInUse)
            Debug.LogWarning("Trying to launch player menu scene whlst battle or evolution scene in use");

        Scene freeRoamScene = sceneRecordStack.Peek().scene;
        FreeRoamSceneController freeRoamSceneController = GetFreeRoamSceneController(freeRoamScene);

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
                playerMenuSceneInUse = true;

                SceneManager.SetActiveScene(playerMenuScene);

                StartFadeIn();

            };

        };

    }

    public static void ClosePlayerMenuScene()
    {

        if (!playerMenuSceneInUse)
        {
            Debug.LogError("Player menu scene trying to be closed while player menu scene not already active");
            return;
        }

        Scene freeRoamScene = sceneRecordStack.Peek().scene;
        FreeRoamSceneController freeRoamSceneController = GetFreeRoamSceneController(freeRoamScene);

        StartFadeOut();

        FadeOutComplete += () =>
        {

            playerMenuSceneInUse = false;

            SceneManager.UnloadSceneAsync(playerMenuScene).completed += (ao) =>
            {

                SceneManager.SetActiveScene(freeRoamScene);

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
        GameObject playerGameObject,
        GameObject freeRoamMenuGameObject,
        Vector2Int playerSceneStartPosition,
        Action onCompleteAction = null)
    {

        //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
        int newSceneIndex = SceneManager.sceneCount;

        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        loadSceneOperation.completed += (ao) =>
        {
                
            Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

            sceneRecordStack.Push(new SceneRecord(newScene));

            MovePlayerAndMenuToNewScene(playerGameObject, freeRoamMenuGameObject, newScene, playerSceneStartPosition);

            SceneManager.SetActiveScene(newScene);

            GetFreeRoamSceneController(newScene).SetSceneRunningState(false);

            SceneManager.UnloadSceneAsync(oldScene).completed += (ao) =>
            {

                GetFreeRoamSceneController(newScene).SetSceneRunningState(true);
                GetFreeRoamSceneController(newScene).SetDoorsEnabledState(true);
                StartFadeIn();

                onCompleteAction?.Invoke();

            };

        };

    }

    #endregion

    #region Loadable Scenes Storing and Loading

    public struct LoadableScenes
    {

        //Final element is to be at top of stack

        public struct Scene
        {

            public string sceneIdentifier;
            public Vector2Int position;

            public Scene(string sceneIdentifier, Vector2Int position)
            {
                this.sceneIdentifier = sceneIdentifier;
                this.position = position;
            }

            public static readonly Regex sceneStringRegex = new Regex("^[a-zA-Z0-9/ ]+,-?[0-9]+,-?[0-9]+$");

            public static bool TryParse(string s,
                out Scene result)
            {

                string sceneIdetifier;
                int positionX, positionY;

                if (!sceneStringRegex.IsMatch(s))
                {
                    result = default;
                    return false;
                }

                string[] sParts = s.Split(',');

                sceneIdetifier = sParts[0];

                if (!int.TryParse(sParts[1], out positionX))
                {
                    result = default;
                    return false;
                }

                if (!int.TryParse(sParts[2], out positionY))
                {
                    result = default;
                    return false;
                }

                result = new Scene(sceneIdetifier,
                    new Vector2Int(positionX, positionY));

                return true;

            }

        }

        public Scene[] scenes;

        public int Length => scenes.Length;

        public Scene this[int index]
        {
            get => scenes[index];
            set => scenes[index] = value;
        }

        public LoadableScenes(Scene[] scenes)
        {
            this.scenes = scenes;
        }

        public static readonly Regex loadableScenesStringRegex = new Regex("^([a-zA-Z0-9/ ]+,-?[0-9]+,-?[0-9]+)(;[a-zA-Z0-9/ ]+,-?[0-9]+,-?[0-9]+)*$");

        public static bool TryParse(string s,
            out LoadableScenes result)
        {

            if (!loadableScenesStringRegex.IsMatch(s))
            {
                result = default;
                return false;
            }

            string[] sceneEntries = s.Split(';');

            List<Scene> scenes = new List<Scene>();

            foreach (string entry in sceneEntries)
            {

                if (Scene.TryParse(entry, out Scene scene))
                {
                    scenes.Add(scene);
                }
                else
                {
                    result = default;
                    return false;
                }

            }

            result = new LoadableScenes(scenes.ToArray());
            return true;

        }

    }

    /// <summary>
    /// Clears the current scene stack and loads a new scene stack on top of the current one.
    /// The position of the last loadable scene is used as the position to place the player in. The positions of the other loadable scenes are used as return positions
    /// </summary>
    /// <param name="loadableScenes">The loadable scenes instance to load</param>
    public static void LoadLoadableScenes(LoadableScenes loadableScenes)
    {

        sceneRecordStack.Clear();

        Action onStartingSceneLoadAction = loadableScenes.Length > 1
            ? new Action(() =>
            {

                LoadLoadbleScenes_LoadDepthLevelScene(loadableScenes, 1);

            }) : null;

        //Load first scene
        OpenStartingScene(SceneManager.GetActiveScene(),
            loadableScenes[0].sceneIdentifier,
            PlayerController.singleton.gameObject,
            null,
            loadableScenes[0].position,
            onStartingSceneLoadAction);

    }

    private static void LoadLoadbleScenes_LoadDepthLevelScene(LoadableScenes loadableScenes, int index)
    {

        LoadableScenes.Scene scene = loadableScenes.scenes[index];

        Action onSceneLoadAction = loadableScenes.Length - 1 > index //If there are still more scenes to load
            ? new Action(() =>
            {

                LoadLoadbleScenes_LoadDepthLevelScene(loadableScenes, 1);

            }) : null;

        LoadDepthLevelScene(scene.sceneIdentifier,
            PlayerController.singleton.position,
            scene.position,
            onSceneLoadAction);

    }

    #endregion

}
