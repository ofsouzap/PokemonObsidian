using System.Collections;
using UnityEngine;

namespace FreeRoaming
{
    public class MultiSceneDoorController : MonoBehaviour
    {

        public SceneDoorDetails doorDetails;

        [SerializeField]
        private SceneDoor[] doors;

        private void Start()
        {

            foreach (SceneDoor door in doors)
                door.doorDetails = doorDetails;

        }

    }
}