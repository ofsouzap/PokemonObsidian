using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
{
    public interface IOccupyPositions : IHasPositions
    {

        // N.B. all implementers should have a Collider2D component so they can be found by 'Gridwise.Manager's

        // When implementing GetPositions, should return any grid positions that the object is occupying and that other objects shouldn't be able to move into

    }
}