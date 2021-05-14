using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FreeRoaming
{
    [RequireComponent(typeof(Collider2D))]
    public class OccupiedAreaController : MonoBehaviour, IOccupyPositions
    {

        public List<Vector2Int> gridPositions { get; protected set; }

        /// <summary>
        /// The offset from the root of every occupied position of this game object
        /// </summary>
        public Vector2Int[] occupiedPositionOffsets = new Vector2Int[0];

        public virtual Vector2Int[] GetPositions()
        {
            return gridPositions.ToArray();
        }

        protected void Awake()
        {

            gridPositions = new List<Vector2Int>();
            Vector2Int rootGridPosition = Vector2Int.RoundToInt(transform.position);

            if (occupiedPositionOffsets != null)
                gridPositions.AddRange(occupiedPositionOffsets.Select(x => rootGridPosition + x));

        }

    }
}
