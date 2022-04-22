using System.Linq;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class RotomRoomClueSignController : MessageDecorationController
    {
        protected override string[] GetMessages()
        {

            bool[] goalStates = RotomRoomSceneController.goalBookshelfStates;

            string[] messages = new string[goalStates.Length];

            for (int i = 0; i < goalStates.Length; i++)
            {
                string posPart = goalStates[i] ? "->" : "<-";
                messages[i] = $"{i+1}: {posPart}";
            }

            return messages;

        }

    }
}