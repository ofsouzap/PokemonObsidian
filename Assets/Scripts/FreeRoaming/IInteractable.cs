using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
{

    public interface IInteractable : IOccupyPositions
    {

        //Allows the object to be interacted with knowing which game character interacted with it
        public void Interact(GameCharacterController interacter);

    }
}