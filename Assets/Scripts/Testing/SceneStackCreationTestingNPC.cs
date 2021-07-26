using UnityEngine;
using FreeRoaming;

#if UNITY_EDITOR

namespace Testing
{
    public class SceneStackCreationTestingNPC : GameCharacterController, IInteractable
    {

        public void Interact(GameCharacterController interacter)
        {

            GameSceneManager.SceneStack ss = GameSceneManager.CurrentSceneStack;
            Debug.Log("Current scene stack string - " + ss.AsString);

        }

    }
}

#endif