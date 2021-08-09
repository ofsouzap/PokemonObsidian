using System.Collections;
using UnityEngine;

namespace FreeRoaming.BuildingGameObjects
{
    public class HouseController : SceneEntranceBuildingController
    {

        public const string houseSceneIdentifier = "House";

        public override SceneDoorDetails GetBaseSceneDoorDetails()
            => new SceneDoorDetails()
            {
                isDepthLevel = true,
                isLoadingDoor = true,
                newSceneTargetPosition = new Vector2Int(0, 0),
                sceneName = houseSceneIdentifier,
                instanceId = 0
            };

        public override SceneDoorDetails GetInstanceSpecificSceneDoorDetails(SceneDoorDetails baseDoorDetails)
            => new SceneDoorDetails()
            {
                isDepthLevel = baseDoorDetails.isDepthLevel,
                isLoadingDoor = baseDoorDetails.isLoadingDoor,
                newSceneTargetPosition = baseDoorDetails.newSceneTargetPosition,
                returnPosition = Vector2Int.RoundToInt(door.transform.position) + Vector2Int.down,
                sceneName = baseDoorDetails.sceneName,
                instanceId = buildingId
            };

    }
}