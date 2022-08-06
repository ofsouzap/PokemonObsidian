using System.Collections;
using UnityEngine;

namespace FreeRoaming
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SlidingIceTileController : MonoBehaviour, IHasPositions
    {

        private Vector2Int position;

        public Vector2Int[] GetPositions()
            => new Vector2Int[1] { position };

        public Vector2Int GetPosition()
            => GetPositions()[0];

        private void Start()
        {

            position = Vector2Int.RoundToInt(transform.position);

        }

    }
}