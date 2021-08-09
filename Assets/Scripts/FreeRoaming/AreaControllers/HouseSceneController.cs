using System;
using System.Linq;
using UnityEngine;

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
            
            if (GameSceneManager.CurrentSceneInstanceId != 0)
            {

                if (houseSpecificPrefabs.Any(x => x.houseId == GameSceneManager.CurrentSceneInstanceId))
                {
                    Instantiate(houseSpecificPrefabs
                        .Where(x => x.houseId == GameSceneManager.CurrentSceneInstanceId)
                        .ToArray()
                        [0]
                        .prefab, houseSpecificRoot);
                }
                else
                {
                    Debug.LogError("No house specific prefab set for id - " + GameSceneManager.CurrentSceneInstanceId);
                }

            }

        }

    }
}