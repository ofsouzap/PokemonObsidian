using System.Collections;
using UnityEngine;

namespace FreeRoaming.BuildingGameObjects
{
    public class PokeMartController : SceneEntranceBuildingController
    {

        public const string pokeMartSceneIdentifier = "Poke Mart";

        public override SceneDoorDetails GetBaseSceneDoorDetails() => new SceneDoorDetails()
        {
            isDepthLevel = true,
            isLoadingDoor = true,
            newSceneTargetPosition = new Vector2Int(0, 0),
            sceneName = pokeMartSceneIdentifier
        };

        protected override void SetBuildingEntranceArguments()
        {
            AreaEntranceArguments.PokeMartEntranceArguments.SetArguments(buildingId);
        }

        /// <summary>
        /// Gets an instance-specific SceneDoorDetails for this specific pokemon center by setting the instance-specific properties of it (eg. the return position)
        /// </summary>
        /// <param name="baseDetails">The basic door details to base the output on</param>
        public override SceneDoorDetails GetInstanceSpecificSceneDoorDetails(SceneDoorDetails baseDetails)
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