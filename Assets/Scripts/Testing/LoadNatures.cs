using UnityEngine;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadNatures : MonoBehaviour
    {

        private void Awake()
        {
            Nature.LoadRegistry();
        }

    }
}

#endif