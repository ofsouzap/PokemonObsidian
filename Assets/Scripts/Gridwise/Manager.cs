using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gridwise;

namespace Gridwise
{
    public class Manager : MonoBehaviour
    {

        //Should be applied to game object with name

        public const float availabilityCheckRange = 10;
        public const float occupierCheckRange = 1;

        /// <summary>
        /// Check if a position on the grid is available by finding all IOccupyPositions nearby (requiring that their object has a Collider2D) and checking each of their positions
        /// The range which will be checked is that constant float Manager.availabilityCheckRange
        /// </summary>
        /// <param name="queryPosition">The position to check the availability of</param>
        /// <returns>Whether the position is available</returns>
        public bool CheckPositionAvailability(Vector2Int queryPosition)
        {

            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(queryPosition, availabilityCheckRange);

            IOccupyPositions[] positionOccupiers = nearbyColliders
                .Where((x) => x.GetComponent<IOccupyPositions>() != null)
                .Select((x) => x.GetComponent<IOccupyPositions>())
                .ToArray();

            foreach (IOccupyPositions occupier in positionOccupiers)
            {

                if (occupier.GetPositions().Contains(queryPosition))
                {

                    return false;

                }

            }

            return true;

        }

        /// <summary>
        /// Attempts to find an IInteractable in the queried position using Manager.occupierCheckRange to search objects nearby
        /// </summary>
        /// <param name="queryPosition">The position to check</param>
        /// <returns>IInteractable if found, otherwise null</returns>
        public IInteractable GetInteractableInPosition(Vector2Int queryPosition)
        {

            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(queryPosition, occupierCheckRange);

            IInteractable[] validCandidates = nearbyColliders
                .Where((x) => x.GetComponent<IInteractable>() != null && x.GetComponent<IOccupyPositions>() != null)
                .Where((x) => x.GetComponent<IOccupyPositions>().GetPositions().Contains(queryPosition))
                .Select((x) => x.GetComponent<IInteractable>())
                .ToArray();

            switch (validCandidates.Length)
            {

                case 0:
                    return null;

                case 1:
                    return validCandidates[0];

                default:
                    Debug.LogWarning($"Multiple interactables were found to occupy position ({queryPosition.x},{queryPosition.y})");
                    return validCandidates[0];

            }

        }

    }
}