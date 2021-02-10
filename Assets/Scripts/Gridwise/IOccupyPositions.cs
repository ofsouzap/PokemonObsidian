using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gridwise;

namespace Gridwise
{
    public interface IOccupyPositions
    {

        //N.B. all implementers should have a Collider2D component so they can be found by 'Gridwise.Manager's

        //Should return any grid positions that the object is occupying and that other objects shouldn't be able to move into
        public Vector2Int[] GetPositions();

    }
}