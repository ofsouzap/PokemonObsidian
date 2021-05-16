using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableDecorationController : MonoBehaviour, IInteractable
    {

        public Vector2Int[] GetPositions()
            => new Vector2Int[] { Vector2Int.RoundToInt(transform.position) };

        protected FreeRoamSceneController sceneController;
        protected TextBoxController textBoxController;

        protected virtual void Start()
        {
            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);
            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);
        }

        public abstract void Interact(GameCharacterController interactee);

    }
}