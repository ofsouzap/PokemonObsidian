using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    [RequireComponent(typeof(Collider2D))]
    public class PCController : MonoBehaviour, IInteractable
    {

        public const string storageSystemScenePath = "Menu_Storage System";

        public Vector2Int[] GetPositions()
            => new Vector2Int[] { Vector2Int.RoundToInt(transform.position) };

        public void Interact(GameCharacterController interacter)
        {

            FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene).SetSceneRunningState(false);

            GameSceneManager.LaunchPlayerMenuScene(storageSystemScenePath);

        }

    }
}
