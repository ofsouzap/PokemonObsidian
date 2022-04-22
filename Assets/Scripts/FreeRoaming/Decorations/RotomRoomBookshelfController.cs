using System;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class RotomRoomBookshelfController : InteractableDecorationController
    {

        public Action InteractAction;

        public override Vector2Int[] GetPositions()
        {
            //Have to refresh positions every time since the bookshelf can move
            return CalculateGridPositions();
        }

        public override void Interact(GameCharacterController interactee)
        {

            InteractAction?.Invoke();

        }

    }
}