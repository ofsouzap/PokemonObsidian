using UnityEngine;
using FreeRoaming;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadGameCharacterSprites : MonoBehaviour
    {

        public static LoadGameCharacterSprites singleton = null;
        [HideInInspector]
        public bool loaded = false;

        private void Awake()
        {
            singleton = this;
            GameCharacterSpriteStorage.TryLoad();
            loaded = true;
        }

    }
}

#endif