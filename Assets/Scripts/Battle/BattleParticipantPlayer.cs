using System.Linq;
using UnityEngine;
using Battle;
using Pokemon;

namespace Battle
{
    public class BattleParticipantPlayer : BattleParticipant
    {

        public override PokemonInstance[] GetPokemon()
        {
            throw new System.NotImplementedException();
        }

        public override void StartChoosingAction(BattleData battleData)
        {
            throw new System.NotImplementedException();
        }

        public override void StartChoosingNextPokemon()
        {
            throw new System.NotImplementedException();
        }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().All((x) => x.health <= 0);
        }

    }
}