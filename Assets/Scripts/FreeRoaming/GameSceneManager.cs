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

        #endregion

        public static void Initialise()
        {
            
            sceneRecordStack.Clear();
            battleSceneInUse = false;

        }

        private static GameObject playerGameObject = null;
        public static void SetPlayerGameObject(GameObject go) => playerGameObject = go;

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

        #region Scene Changing

        #region Scene Fading

        private delegate void OnComplete();
        private static event OnComplete FadeComplete;

        private static void StartFadeOut()
        {
            FadeComplete = null;
            BlackFadeController goController = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Black Fade")).GetComponent<BlackFadeController>();
            goController.FadeOut();
            goController.FadeCompleted += () => FadeComplete?.Invoke();
            goController.FadeCompleted += () => UnityEngine.Object.Destroy(goController.gameObject);
        }

        private static void StartFadeIn()
        {
            FadeComplete = null;
            BlackFadeController goController = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Black Fade")).GetComponent<BlackFadeController>();
            goController.FadeIn();
            goController.FadeCompleted += () => FadeComplete?.Invoke();
            goController.FadeCompleted += () => UnityEngine.Object.Destroy(goController.gameObject);
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

            playerGameObject.GetComponent<PlayerController>().CompleteMovement();
            playerGameObject.GetComponent<PlayerController>().SetPosition(newPosition);

            if (playerObject != null)
                SceneManager.MoveGameObjectToScene(playerObject, newScene);
            else
                Debug.LogWarning("No player object set. Couldn't move player to new scene");

            if (autoEnablePlayerRenderers)
                foreach (Renderer r in playerGameObject.GetComponentsInChildren<Renderer>())
                    r.enabled = true;

        }

        private static void LoadParallelLevelScene(SceneDoorDetails doorDetails)
        {

            if (battleSceneInUse)
            {
                Debug.LogError("Attempting to load scene whilst in battle");
                return;
            }

            playerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            Scene oldScene = sceneRecordStack.Pop().scene;

            GetFreeRoamSceneController(oldScene).SetDoorsEnabledState(false);
            GetFreeRoamSceneController(oldScene).SetSceneRunningState(false);

            StartFadeOut();

            FadeComplete += () =>
            {

                GetFreeRoamSceneController(oldScene).SetSceneRunningState(true);
                GetFreeRoamSceneController(oldScene).SetEnabledState(false);

                //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
                int newSceneIndex = SceneManager.sceneCount;

                AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(doorDetails.sceneName, LoadSceneMode.Additive);

                loadSceneOperation.completed += (ao) =>
                {

                    Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

                    sceneRecordStack.Push(new SceneRecord(newScene));

                    MovePlayerToNewScene(playerGameObject, newScene, doorDetails.newSceneTargetPosition);

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

            FadeComplete += () =>
            {

                GetFreeRoamSceneController(oldScene).SetSceneRunningState(true);
                GetFreeRoamSceneController(oldScene).SetEnabledState(false);

                playerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

                sceneRecordStack.Push(new SceneRecord(oldScene, doorDetails.returnPosition));

                //https://low-scope.com/unity-quick-get-a-reference-to-a-newly-loaded-scene/
                int newSceneIndex = SceneManager.sceneCount;

                AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(doorDetails.sceneName, LoadSceneMode.Additive);

                loadSceneOperation.completed += (ao) =>
                {

                    Scene newScene = SceneManager.GetSceneAt(newSceneIndex);

                    sceneRecordStack.Push(new SceneRecord(newScene));

                    MovePlayerToNewScene(playerGameObject, newScene, doorDetails.newSceneTargetPosition);
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

            playerGameObject.GetComponent<PlayerController>().SetMoveDelay(sceneChangePlayerMoveDelay);

            GetFreeRoamSceneController(oldSceneRecord.scene).SetDoorsEnabledState(false);
            GetFreeRoamSceneController(oldSceneRecord.scene).SetSceneRunningState(false);

            StartFadeOut();

            FadeComplete += () => {

                GetFreeRoamSceneController(oldSceneRecord.scene).SetSceneRunningState(true);
                GetFreeRoamSceneController(oldSceneRecord.scene).SetEnabledState(false);

                MovePlayerToNewScene(playerGameObject, newSceneRecord.scene, newSceneRecord.gridPosition);

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

        #endregion

    }
}
