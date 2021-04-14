using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

#if UNITY_EDITOR

namespace Testing
{
    public class TurningInteractionNPC : GameCharacterController, IInteractable
    {

        public void Interact(GameCharacterController interacter)
        {
            TryTurn(interacter.directionFacing);
        }

    }
}

#endif