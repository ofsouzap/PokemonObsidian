using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCRotomRoomTransportController : NPCController
    {

        public const string npcName = "Rue";

        protected override string GetNPCName()
            => npcName;

        public static readonly string[] messages = new string[]
        {
            "Have you heard of escape rooms?",
            "I've heard that some cities in other regions have them and they sound really fun.",
            "How about we play an escape room game?",
            "Heehee..."
        };

        public Vector2Int relativeReturnPosition;
        private Vector2Int GetReturnPosition()
            => position + relativeReturnPosition;

        public SceneDoorDetails GetDoorDetails()
            => new SceneDoorDetails()
            {
                isLoadingDoor = true,
                isDepthLevel = false, //Parallel-level so that player doesn't try to talk to this NPC again
                newSceneTargetPosition = new Vector2Int(0, 0),
                returnPosition = GetReturnPosition(),
                sceneName = AreaControllers.RotomRoomSceneController.rotomRoomSceneIdentifier
            };

        public override void Interact(GameCharacterController interacter)
        {
            TryTurn(GetOppositeDirection(interacter.directionFacing)); //Face player when talking
            StartCoroutine(InteractionCoroutine());
        }

        private IEnumerator InteractionCoroutine()
        {

            sceneController.SetSceneRunningState(false);

            yield return StartCoroutine(Speak(messages));

            GameSceneManager.UseDoor(GetDoorDetails());

            //Scene isn't resumed as new scene has been launched

        }

    }
}