using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FreeRoaming.BuildingGameObjects
{
    public class PokemonCenterController : SceneEntranceBuildingController
    {

        public const string pokemonCenterSceneIdentifier = "Pokemon Center";

        public override SceneDoorDetails GetBaseSceneDoorDetails() => new SceneDoorDetails()
        {
            isDepthLevel = true,
            isLoadingDoor = true,
            newSceneTargetPosition = new Vector2Int(0, 0),
            sceneName = pokemonCenterSceneIdentifier,
            instanceId = 0
        };

        public override SceneDoorDetails GetInstanceSpecificSceneDoorDetails(SceneDoorDetails baseDetails)
        {
            return new SceneDoorDetails()
            {
                isDepthLevel = baseDetails.isDepthLevel,
                isLoadingDoor = baseDetails.isLoadingDoor,
                newSceneTargetPosition = baseDetails.newSceneTargetPosition,
                returnPosition = Vector2Int.RoundToInt(door.transform.position) + Vector2Int.down,
                sceneName = baseDetails.sceneName,
                instanceId = buildingId
            };
        }

    }
}
