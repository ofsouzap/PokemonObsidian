using System.Collections;
using UnityEngine;

namespace FreeRoaming
{

    [RequireComponent(typeof(Collider2D))]
    public class InteractionProxy : ObstacleController, IInteractable, IOccupyPositions
    {

        public GameObject _interactable;
        [HideInInspector]
        public IInteractable interactable;

        protected override void Start()
        {

            base.Start();

            interactable = _interactable.GetComponent<IInteractable>();

        }

        public void Interact(GameCharacterController interactee)
        {

            interactable.Interact(interactee);

        }

    }

}