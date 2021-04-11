using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using FreeRoaming;

namespace FreeRoaming
{
    [RequireComponent(typeof(FreeRoamSceneController))]
    public class GridManager : MonoBehaviour
    {

        //Should be applied to game object with name

        public const float availabilityCheckRange = 10;
        public const float occupierCheckRange = 1;

        public Scene Scene => gameObject.scene;

        public static GridManager GetSceneGridManager(Scene scene)
        {

            GridManager[] managers = FindObjectsOfType<GridManager>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

            switch (managers.Length)
            {

                case 0:
                    return null;

                case 1:
                    return managers[0];

                default:
                    Debug.LogError("Multiple GridManager found");
                    return managers[0];

            }

        }

        /// <summary>
        /// Get the position one step in a direction from another position
        /// </summary>
        public static Vector2Int GetPositionInDirection(Vector2Int startPosition, GameCharacterController.FacingDirection direction)
        {

            Vector2Int offset;

            switch (direction)
            {

                case GameCharacterController.FacingDirection.Up:
                    offset = Vector2Int.up;
                    break;

                case GameCharacterController.FacingDirection.Down:
                    offset = -Vector2Int.up;
                    break;

                case GameCharacterController.FacingDirection.Left:
                    offset = -Vector2Int.right;
                    break;

                case GameCharacterController.FacingDirection.Right:
                    offset = Vector2Int.right;
                    break;

                default:
                    Debug.LogWarning($"Invalid directionFacing was found ({direction})");
                    offset = Vector2Int.up;
                    break;

            }

            return startPosition + offset;

        }

        /// <summary>
        /// Gets the positions in a direction from provided position
        /// </summary>
        public static Vector2Int[] GetPositionsInDirection(Vector2Int startPosition, GameCharacterController.FacingDirection direction, ushort count)
        {

            Vector2Int[] positions = new Vector2Int[count];

            positions[0] = GetPositionInDirection(startPosition, direction);

            for (int i = 1; i < count; i++)
                positions[i] = GetPositionInDirection(positions[i - 1], direction);

            return positions;

        }

        /// <summary>
        /// Finds an object in a position on the grid by finding all IOccupyPositions nearby (requiring that their object has a Collider2D) and checking each of their positions
        /// The range which will be checked is the constant float Manager.availabilityCheckRange
        /// </summary>
        public GameObject GetObjectInPosition(Vector2Int queryPosition)
        {

            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(queryPosition, availabilityCheckRange)
                .Where(x => x.gameObject.scene == Scene)
                .ToArray();

            foreach (Collider2D collider in nearbyColliders)
            {

                if (collider.GetComponent<IOccupyPositions>() != null)
                {

                    if (collider.GetComponent<IOccupyPositions>().GetPositions().Contains(queryPosition))
                    {
                        return collider.gameObject;
                    }

                }

            }

            return null;

        }

        /// <summary>
        /// Checks whether a position is available using GetObjectInPosition
        /// </summary>
        /// <param name="queryPosition">The position to check the availability of</param>
        /// <returns>Whether the position is available</returns>
        public bool CheckPositionAvailability(Vector2Int queryPosition)
        {

            return GetObjectInPosition(queryPosition) == null;

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
                .Where((x) => x.GetComponent<IInteractable>() != null)
                .Where((x) => x.GetComponent<IInteractable>().GetPositions().Contains(queryPosition))
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
