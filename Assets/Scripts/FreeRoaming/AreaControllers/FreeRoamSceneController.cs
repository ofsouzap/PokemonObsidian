using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace FreeRoaming.AreaControllers
{
    [RequireComponent(typeof(GridManager))]
    public class FreeRoamSceneController : GeneralSceneManager
    {

        protected virtual void Start() { }
        protected virtual void Update() { }

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
        public bool SceneIsRunning => sceneRunning;

        public void SetSceneRunningState(bool state)
            => sceneRunning = state;

        #endregion

        #region Enabling

        protected override void EnableScene()
        {

            base.EnableScene();

            SetSceneRunningState(sceneShouldBeRunningOnSceneEnable);

        }

        protected override void DisableScene()
        {

            base.DisableScene();

            sceneShouldBeRunningOnSceneEnable = sceneRunning;
            SetSceneRunningState(false);

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
