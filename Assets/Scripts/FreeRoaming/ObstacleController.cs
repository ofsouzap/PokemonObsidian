using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
{
    public class ObstacleController : MonoBehaviour, IOccupyPositions
    {

        public List<Vector2Int> gridPositions { get; protected set; }

        public virtual Vector2Int[] GetPositions()
        {
            return gridPositions.ToArray();
        }

        protected virtual void Start()
        {

            gridPositions = new List<Vector2Int>();
            gridPositions.Add(Vector2Int.RoundToInt(transform.position));

        }

    }
}