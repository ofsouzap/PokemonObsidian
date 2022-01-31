using System.Collections;
using UnityEngine;

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
