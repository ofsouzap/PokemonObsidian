using System;
using UnityEngine;

namespace FreeRoaming
{
    public interface IHasPositions
    {

        //Should return any grid positions that the object is in
        public Vector2Int[] GetPositions();

    }
}
