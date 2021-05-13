using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using FreeRoaming.AreaControllers;

namespace FreeRoaming
{
    public class SceneDoor : MonoBehaviour
    {

        [Tooltip("The details for this door")]
        public SceneDoorDetails doorDetails;

        /// <summary>
        /// A delay to prevent a scene trying to be loaded multiple times
        /// </summary>
        public const float triggerDelay = 2;

        private float lastTrigger = -triggerDelay;

        private FreeRoamSceneController sceneController;
        private GridManager gridManager;
        private Vector2Int gridPosition;

        private UnityEvent BeforeDoorUsed = new UnityEvent();

        private void Start()
        {

            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);
            gridManager = GridManager.GetSceneGridManager(gameObject.scene);
            gridPosition = Vector2Int.RoundToInt(transform.position);

        }

        private void Update()
        {

            if (gridManager != null)
            {

                GameObject go = gridManager.GetObjectInPosition(gridPosition);

                if (go != null && go.GetComponent<PlayerController>() != null)
                    TryTrigger();

            }

        }

        private void TryTrigger()
        {
            if (Time.time - lastTrigger >= triggerDelay && sceneController.SceneIsActive)
            {
                
                lastTrigger = Time.time;

                BeforeDoorUsed.Invoke();

                GameSceneManager.UseDoor(doorDetails);

            }
        }

        public void AddBeforeDoorUsedListener(UnityAction call)
        {

            BeforeDoorUsed.AddListener(call);

        }

    }
}
