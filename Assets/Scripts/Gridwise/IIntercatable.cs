using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gridwise;

namespace Gridwise
{
    public interface IIntercatable
    {

        //Allows the object to be interacted with knowing which game character interacted with it
        public void Interact(GameCharacterController interactee);

    }
}