using System;
using UnityEngine;

namespace FreeRoaming
{

    [Serializable]
    public struct SceneDoorDetails
    {

        /// <summary>
        /// Whether the door is a loading door. If it isn't, it is a door for unloading the current scene and ascending in the scene stack (in GameSceneManager). If false, the other attributes aren't used
        /// </summary>
        public bool isLoadingDoor;

        /// <summary>
        /// The name/path of the scene to load
        /// </summary>
        public string sceneName;

        /// <summary>
        /// Whether the scene should be loaded with depth-level. If not, it should be parallel-level
        /// </summary>
        public bool isDepthLevel;

        /// <summary>
        /// The grid position in which to put the player in the new scene when they use this door
        /// </summary>
        public Vector2Int newSceneTargetPosition;

        /// <summary>
        /// The grid position which the player should be returned to when they return to the current scene. This is only used if the door is for depth-level scene changing
        /// </summary>
        public Vector2Int returnPosition;

        /// <summary>
        /// The instance id of the scene to launch. For example, the id of the house to load when loading the house scene
        /// </summary>
        public int instanceId;

    }

}
