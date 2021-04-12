using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FreeRoaming
{
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
        private struct SceneRecord
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

        #endregion

        public static void Initialise()
        {
            
            sceneRecordStack.Clear();
            battleSceneInUse = false;

        }

        private static GameObject PlayerGameObject => PlayerController.singleton.gameObject;

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

        private static void MovePlayerToNewScene(GameObject playerObject,
            Scene newScene,
            Vector2Int newPosition,
            bool autoEnablePlayerRenderers = true)
        {

            PlayerGameObject.GetComponent<PlayerController>().CompleteMovement();
            PlayerGameObject.GetComponent<PlayerController>().SetPosition(newPosition);

            if (playerObject != null)
                SceneManager.MoveGameObjectToScene(playerObject, newScene);
            else
                Debug.LogWarning("No player object set. Couldn't move player to new scene");

            if (autoEnablePlayerRenderers)
                foreach (Renderer r in PlayerGameObject.GetComponentsInChildren<Renderer>())
                    r.enabled = true;

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

                    MovePlayerToNewScene(PlayerGameObject, newScene, doorDetails.newSceneTargetPosition);

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

                sceneRecordStack.Push(new SceneRecord(oldScene, doorDetails.returnPosition));

                //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
                int newSceneIndex = SceneManager.sceneCount;

                AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(doorDetails.sceneName, LoadSceneMode.Additive);

                loadSceneOperation.completed += (ao) =>
                {

                    Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

                    sceneRecordStack.Push(new SceneRecord(newScene));

                    MovePlayerToNewScene(PlayerGameObject, newScene, doorDetails.newSceneTargetPosition);
                    SceneManager.SetActiveScene(newScene);

                    StartFadeIn();
                    GetFreeRoamSceneController(newScene).SetDoorsEnabledState(true);

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

                MovePlayerToNewScene(PlayerGameObject, newSceneRecord.scene, newSceneRecord.gridPosition);

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

        #endregion

    }
}
