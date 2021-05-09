using System.Collections;
using UnityEngine;

#if UNITY_EDITOR

namespace Testing
{
    public class Testing_LoadAllData : MonoBehaviour
    {
        private void Awake()
        {
            StartUp.LoadAllData.Load();
        }
    }
}

#endif
