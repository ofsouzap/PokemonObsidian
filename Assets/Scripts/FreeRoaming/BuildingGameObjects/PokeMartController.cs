using System.Collections;
using UnityEngine;

namespace FreeRoaming.BuildingGameObjects
{
    public class PokeMartController : MonoBehaviour
    {

        public const string pokeMartSceneIdentifier = "Poke Mart";

        /// <summary>
        /// A basic scene door details for pokemon center doors. Any instance-specific properties will be set in the instance
        /// </summary>
        public static readonly SceneDoorDetails basePokeMartDoorDetails = new SceneDoorDetails()
        {
            isDepthLevel = true,
            isLoadingDoor = true,
            newSceneTargetPosition = new Vector2Int(0, 0),
            sceneName = pokeMartSceneIdentifier
        };

        [SerializeField]
        private SceneDoor door;

        [SerializeField]
        private int pokeMartId;

        private void Start()
        {

            door.AddBeforeDoorUsedListener(() => AreaEntranceArguments.PokeMartEntranceArguments.SetArguments(pokeMartId));
            door.doorDetails = GetInstanceSpecificDoorDetails(basePokeMartDoorDetails);

        }

        /// <summary>
        /// Gets an instance-specific SceneDoorDetails for this specific pokemon center by setting the instance-specific properties of it (eg. the return position)
        /// </summary>
        /// <param name="baseDetails">The basic door details to base the output on</param>
        private SceneDoorDetails GetInstanceSpecificDoorDetails(SceneDoorDetails baseDetails)
        {
            return new SceneDoorDetails()
            {
                isDepthLevel = baseDetails.isDepthLevel,
                isLoadingDoor = baseDetails.isLoadingDoor,
                newSceneTargetPosition = baseDetails.newSceneTargetPosition,
                returnPosition = Vector2Int.RoundToInt(door.transform.position) + Vector2Int.down,
                sceneName = baseDetails.sceneName
            };
        }

    }
}