using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FreeRoaming.BuildingGameObjects
{
    public class PokemonCenterController : MonoBehaviour
    {

        public const string pokemonCenterSceneIdentifier = "Pokemon Center";

        /// <summary>
        /// A basic scene door details for pokemon center doors. Any instance-specific properties will be set in the instance
        /// </summary>
        public static readonly SceneDoorDetails basePokemonCenterDoorDetails = new SceneDoorDetails()
        {
            isDepthLevel = true,
            isLoadingDoor = true,
            newSceneTargetPosition = new Vector2Int(0, 0),
            sceneName = pokemonCenterSceneIdentifier
        };

        [SerializeField]
        private SceneDoor door;

        private void Start()
        {

            door.doorDetails = GetInstanceSpecificDoorDetails(basePokemonCenterDoorDetails);

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
