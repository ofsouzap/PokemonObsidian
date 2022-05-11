using UnityEngine;
using FreeRoaming;

namespace Testing
{
    public class SceneStackLoadingNPC : GameCharacterController, IInteractable
    {

        public string loadableScenesString;

        public void Interact(GameCharacterController interacter)
        {

            if (GameSceneManager.SceneStack.TryParse(loadableScenesString, out GameSceneManager.SceneStack loadableScenes, out string errMsg))
            {
                GameSceneManager.LoadSceneStack(loadableScenes);
            }
            else
            {
                print("Unable to parse loadableScenesString:\n" + errMsg);
            }

        }

    }
}
