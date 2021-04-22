using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
{
    public class ObstacleController : FreeRoamSprite, IOccupyPositions
    {

        public List<Vector2Int> gridPositions { get; protected set; }

        public Vector2Int[] additionalOccupiedPositionOffsets = new Vector2Int[0];

        public virtual Vector2Int[] GetPositions()
        {
            return gridPositions.ToArray();
        }

        protected override void Start()
        {

            base.Start();

            gridPositions = new List<Vector2Int>();
            Vector2Int rootGridPosition = Vector2Int.RoundToInt(transform.position);
            gridPositions.Add(rootGridPosition);

            if (additionalOccupiedPositionOffsets != null)
                gridPositions.AddRange(additionalOccupiedPositionOffsets.Select(x => rootGridPosition + x));

        }

    }
}