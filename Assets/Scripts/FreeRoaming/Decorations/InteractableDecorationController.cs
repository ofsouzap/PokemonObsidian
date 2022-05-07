using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableDecorationController : MonoBehaviour, IInteractable
    {

        protected Vector2Int[] GridPositions { get; private set; }

        public Vector2Int[] additionalOccupiedPositionOffsets = new Vector2Int[0];

        public virtual Vector2Int[] GetPositions()
        {

            if (GridPositions == null)
                RefreshGridPositions();

            return GridPositions;

        }

        protected FreeRoamSceneController sceneController;
        protected TextBoxController textBoxController;

        protected virtual void Start()
        {

            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);
            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

            RefreshGridPositions();

        }

        protected void RefreshGridPositions()
            => GridPositions = CalculateGridPositions();

        protected Vector2Int[] CalculateGridPositions()
        {

            Vector2Int[] positions = new Vector2Int[additionalOccupiedPositionOffsets.Length + 1];
            Vector2Int rootGridPosition = Vector2Int.RoundToInt(transform.position);
            positions[0] = rootGridPosition;

            if (additionalOccupiedPositionOffsets != null)
            {
                for (int i = 0; i < additionalOccupiedPositionOffsets.Length; i++)
                {
                    positions[i + 1] = additionalOccupiedPositionOffsets[i] + rootGridPosition;
                }
            }

            return positions;

        }

        public abstract void Interact(GameCharacterController interactee);

    }
}