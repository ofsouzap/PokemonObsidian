using UnityEngine;
using FreeRoaming;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadableScenesLoadingNPC : GameCharacterController, IInteractable
    {

        public string loadableScenesString;

        public void Interact(GameCharacterController interacter)
        {
            
            if (GameSceneManager.LoadableScenes.TryParse(loadableScenesString, out GameSceneManager.LoadableScenes loadableScenes))
            {
                GameSceneManager.LoadLoadableScenes(loadableScenes);
            }
            else
            {
                print("Unable to parse loadableScenesString");
            }

        }

    }
}

#endif
