using System.Linq;
using System.IO;
using System.Threading;
using Pokemon;
using Networking;
using Serialization;
using System;

namespace Battle
{
    public class BattleParticipantNetwork : BattleParticipant
    {

        public Serializer Serializer => battleManager.Serializer;

        protected string name;
        public override string GetName() => name;

        protected PokemonInstance[] pokemon;
        public override PokemonInstance[] GetPokemon() => pokemon;

        protected Stream stream;

        protected BattleParticipant opponent;

        public BattleParticipantNetwork(string name,
            PokemonInstance[] pokemon,
            Stream stream)
        {

            this.name = name;
            this.pokemon = pokemon;
            this.stream = stream;

        }

        public void SetOpponent(BattleParticipant opponent)
            => this.opponent = opponent;

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

        public override void StartChoosingAction(BattleData battleData) { }

        public override void StartChoosingNextPokemon() { }

        public override Action GetChosenAction()
        {

            if (!actionHasBeenChosen)
                TryReceiveAction(stream);

            return base.GetChosenAction();

        }

        public void TryReceiveAction(Stream stream)
        {
            
            if (!stream.CanRead)
            {
                Connection.LogNetworkEvent("Stream not readable");
                return;
            }

            try
            {

                SetChosenAction(Serializer.DeserializeBattleAction(stream, this, opponent));

                Connection.LogNetworkEvent("Action deserialized and set");

            }
            catch (Serializer.DeserializationException)
            {

                Connection.LogNetworkEvent("Failed to deserialize action");

                //TODO - end the battle

            }

        }

    }
}
