using UnityEngine;
using FreeRoaming;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadAllSprites : MonoBehaviour
    {

        public static LoadAllSprites singleton = null;
        [HideInInspector]
        public bool loaded = false;

        public void Awake()
        {
            singleton = this;
            SpriteStorage.TryLoadAll();
            loaded = true;
        }

    }
}

#endif