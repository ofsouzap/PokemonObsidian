using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class PCController : InteractableDecorationController
    {

        public const string storageSystemScenePath = "Menu_Storage System";

        public override void Interact(GameCharacterController interacter)
        {

            sceneController.SetSceneRunningState(false);

            GameSceneManager.LaunchPlayerMenuScene(storageSystemScenePath);

        }

    }
}
