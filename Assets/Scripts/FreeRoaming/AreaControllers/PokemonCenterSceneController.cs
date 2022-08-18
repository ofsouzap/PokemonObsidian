using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FreeRoaming.AreaControllers
{
    public class PokemonCenterSceneController : FreeRoamSceneController
    {

        [Serializable]
        public struct InstanceSpecificPrefabEntry
        {
            public int pokemonCenterId;
            public GameObject prefab;
        }

        public Vector2Int respawnPosition = Vector2Int.zero;

        public Transform instanceSpecificRoot;

        public InstanceSpecificPrefabEntry[] instanceSpecificPrefabs;

        protected override void Start()
        {

            base.Start();

            //If pokemon center entrance arguments aren't set or the id is 0, no special objects will be placed in the pokemon center
            if (GameSceneManager.CurrentSceneInstanceId != 0)
            {

                if (instanceSpecificPrefabs.Any(x => x.pokemonCenterId == GameSceneManager.CurrentSceneInstanceId))
                {
                    Instantiate(instanceSpecificPrefabs
                        .Where(x => x.pokemonCenterId == GameSceneManager.CurrentSceneInstanceId)
                        .ToArray()
                        [0]
                        .prefab, instanceSpecificRoot);
                }
                else
                {
                    Debug.LogError("No pokemon center instance-specific prefab set for id - " + GameSceneManager.CurrentSceneInstanceId);
                }

            }

            //Set the player's respawn point as this pokemon center
            PlayerController.singleton.RefreshRespawnSceneStackFromCurrent(respawnPosition);

        }

    }
}