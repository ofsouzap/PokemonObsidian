using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FreeRoaming
{
    [RequireComponent(typeof(GridManager))]
    public class FreeRoamSceneController : MonoBehaviour
    {

        public Scene Scene => gameObject.scene;

        public static FreeRoamSceneController GetFreeRoamSceneController(Scene scene)
        {

            FreeRoamSceneController[] controllers = FindObjectsOfType<FreeRoamSceneController>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

            switch (controllers.Length)
            {

                case 0:
                    return null;

                case 1:
                    return controllers[0];

                default:
                    Debug.LogError("Multiple FreeRoamSceneControllers found");
                    return controllers[0];

            }

        }

        public bool SceneIsActive => sceneRunning && sceneEnabled;

        #region Running

        protected bool sceneRunning = true;

        public void SetSceneRunningState(bool state)
            => sceneRunning = state;

        #endregion

        #region Enabling

        protected bool sceneEnabled = true;

        private List<Camera> camerasToReEnableOnSceneEnable = new List<Camera>();
        private List<AudioListener> audioListenersToReEnableOnSceneEnable = new List<AudioListener>();
        private List<Renderer> renderersToReEnableOnSceneEnable = new List<Renderer>();
        private bool sceneShouldBeRunningOnSceneEnable = true;

        private void EnableScene()
        {

            foreach (Camera c in camerasToReEnableOnSceneEnable)
                c.enabled = true;

            camerasToReEnableOnSceneEnable.Clear();

            foreach (AudioListener al in audioListenersToReEnableOnSceneEnable)
                al.enabled = true;

            audioListenersToReEnableOnSceneEnable.Clear();

            foreach (Renderer r in renderersToReEnableOnSceneEnable)
                r.enabled = true;

            renderersToReEnableOnSceneEnable.Clear();

            SetSceneRunningState(sceneShouldBeRunningOnSceneEnable);

        }

        private void DisableScene()
        {

            EnableScene();

            foreach (Camera camera in FindObjectsOfType<Camera>().Where(x => x.gameObject.scene == Scene))
            {
                if (camera.enabled)
                {
                    camerasToReEnableOnSceneEnable.Add(camera);
                    camera.enabled = false;
                }
            }

            foreach (AudioListener al in FindObjectsOfType<AudioListener>().Where(x => x.gameObject.scene == Scene))
            {
                if (al.enabled)
                {
                    audioListenersToReEnableOnSceneEnable.Add(al);
                    al.enabled = false;
                }
            }

            foreach (Renderer renderer in FindObjectsOfType<Renderer>().Where(x => x.gameObject.scene == Scene))
            {
                if (renderer.enabled)
                {
                    renderersToReEnableOnSceneEnable.Add(renderer);
                    renderer.enabled = false;
                }
            }

            sceneShouldBeRunningOnSceneEnable = sceneRunning;
            SetSceneRunningState(false);

            //Just in case this object or component was disabled
            enabled = true;
            gameObject.SetActive(true);

        }

        protected void RefreshEnabledState()
        {
            if (sceneEnabled)
                EnableScene();
            else
                DisableScene();
        }

        public void SetEnabledState(bool state)
        {
            sceneEnabled = state;
            RefreshEnabledState();
        }

        #endregion

        #region Door Activation

        private bool doorsAreEnabled = true;

        private List<SceneDoor> doorsToRenableOnDoorsEnable = new List<SceneDoor>();

        private void EnableDoors()
        {

            foreach (SceneDoor door in doorsToRenableOnDoorsEnable)
                door.enabled = true;

            doorsToRenableOnDoorsEnable.Clear();

        }

        private void DisableDoors()
        {

            EnableDoors();

            foreach (SceneDoor door in FindObjectsOfType<SceneDoor>().Where(x => x.gameObject.scene == Scene))
                if (door.enabled)
                {
                    doorsToRenableOnDoorsEnable.Add(door);
                    door.enabled = false;
                }

        }

        private void RefreshDoorsEnabledState()
        {
            if (doorsAreEnabled)
                EnableDoors();
            else
                DisableDoors();
        }

        public void SetDoorsEnabledState(bool state)
        {
            doorsAreEnabled = state;
            RefreshDoorsEnabledState();
        }

        #endregion

    }
}
