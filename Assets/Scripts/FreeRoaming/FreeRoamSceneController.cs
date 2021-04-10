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

        #region Running

        protected bool sceneRunning = true;

        public bool SceneIsRunning => sceneRunning;

        public void SetSceneRunningState(bool state)
            => sceneRunning = state;

        #endregion

    }
}
