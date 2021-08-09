using System.Collections;
using UnityEngine;

namespace FreeRoaming.BuildingGameObjects
{
    public abstract class SceneEntranceBuildingController : MonoBehaviour
    {

        /// <summary>
        /// Gets a basic scene door details for doors for this building. Any instance-specific properties will be set in the instance
        /// </summary>
        public abstract SceneDoorDetails GetBaseSceneDoorDetails();

        /// <summary>
        /// Gets an instance-specific SceneDoorDetails for a specific building instance by setting the instance-specific properties of it (eg. the return position)
        /// </summary>
        /// <param name="baseDetails">The basic door details to base the output on</param>
        public abstract SceneDoorDetails GetInstanceSpecificSceneDoorDetails(SceneDoorDetails baseDoorDetails);

        [SerializeField]
        protected SceneDoor door;

        [SerializeField]
        protected int buildingId;

        protected virtual void Start()
        {

            door.doorDetails = GetInstanceSpecificSceneDoorDetails(GetBaseSceneDoorDetails());

        }

    }
}