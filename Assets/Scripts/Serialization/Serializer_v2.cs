using Battle;
using Pokemon;
using System;
using System.Collections.Generic;
using System.IO;

namespace Serialization
{
    public class Serializer_v2 : Serializer_v1
    {

        public override ushort GetVersionCode()
            => 0x0002;

        public override void DeserializeData(Stream stream, out long saveTime, out PlayerData playerData, out GameSettings gameSettings, out GameSceneManager.SceneStack sceneStack)
        {

            base.DeserializeData(stream, out saveTime, out playerData, out gameSettings, out sceneStack);

            List<ProgressionEvent> completedProgressionEvents;

            //Completed Progression Events
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int numberOfCompletedProgressionEvents = BitConverter.ToInt32(buffer, 0);

            completedProgressionEvents = new List<ProgressionEvent>();
            for (int i = 0; i < numberOfCompletedProgressionEvents; i++)
            {
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                completedProgressionEvents.Add((ProgressionEvent)BitConverter.ToInt32(buffer, 0));
            }

            playerData.completedProgressionEvents = completedProgressionEvents;

        }

        public override void SerializeData(Stream stream, PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            base.SerializeData(stream, player);

            //Completed progression events
            byte[] buffer = BitConverter.GetBytes(player.CompletedProgressionEventCodesArray.Length);
            stream.Write(buffer, 0, 4);
            foreach (int completedProgressionEvent in player.CompletedProgressionEventCodesArray)
            {
                buffer = BitConverter.GetBytes(completedProgressionEvent);
                stream.Write(buffer, 0, 4);
            }

        }

    }
}
