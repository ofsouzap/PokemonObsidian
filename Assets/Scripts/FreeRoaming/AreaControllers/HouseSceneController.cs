using System;
using System.Linq;
using UnityEngine;
using FreeRoaming.AreaEntranceArguments;

namespace FreeRoaming.AreaControllers
{
    public class HouseSceneController : FreeRoamSceneController
    {

        [Serializable]
        public struct HouseSpecificPrefabEntry
        {
            public int houseId;
            public GameObject prefab;
        }

        public HouseSpecificPrefabEntry[] houseSpecificPrefabs;

        public Transform houseSpecificRoot;

        public Vector2Int respawnPosition;

        protected override void Start()
        {

            base.Start();
            
            if (HouseEntranceArguments.argumentsSet && HouseEntranceArguments.houseId != 0)
            {

                if (houseSpecificPrefabs.Any(x => x.houseId == HouseEntranceArguments.houseId))
                {
                    Instantiate(houseSpecificPrefabs
                        .Where(x => x.houseId == HouseEntranceArguments.houseId)
                        .ToArray()
                        [0]
                        .prefab, houseSpecificRoot);
                }
                else
                {
                    Debug.LogError("No house specific prefab set for id - " + HouseEntranceArguments.houseId);
                }

                HouseEntranceArguments.argumentsSet = false;

            }

        }

    }
}