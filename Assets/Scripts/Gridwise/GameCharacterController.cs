using System.Collections.Generic;
using UnityEngine;
using Gridwise;

namespace Gridwise
{
    public abstract class GameCharacterController : MonoBehaviour, IOccupyPosition
    {

        public enum DirectionFacing
        {
            Down,
            Up,
            Left,
            Right
        }

        public DirectionFacing directionFacing { get; protected set; }

        //The position the character is currently in
        public Vector2Int position { get; protected set; }

        //The position the character may be moving into which should also be reserved
        public Vector2Int movementTargettedGridPosition { get; protected set; }
        //Whether the character is moving and therefore whether the movementTargettedPosition should be counted
        public bool isMoving { get; protected set; }

        //If the character is moving, return both position else only current position
        public Vector2Int[] GetPositions() => isMoving ?
            new Vector2Int[] { position, movementTargettedGridPosition } :
            new Vector2Int[] { position };
        
        [Tooltip("Sprites for this character will be fetched as Resources/{spriteGroupName}/{sprite needed}")]
        public string spriteGroupName;

        protected virtual void Start()
        {

            position = Vector2Int.RoundToInt(transform.position);
            LoadSprites();
            
        }

        protected void LoadSprites()
        {

            //TODO

        }

    }
}