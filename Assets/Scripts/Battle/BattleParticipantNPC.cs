using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Battle.NPCBattleParticipantModes;

namespace Battle
{
    public abstract class BattleParticipantNPC : BattleParticipant
    {

        #region Modes

        public enum Mode
        {
            RandomAttack,
#if UNITY_EDITOR
            Debug_UseRandomMedicineItem,
            Debug_UseRandomBattleItem
#endif
            //TODO - add more
        }

        public static readonly Dictionary<Mode, Func<string, PokemonInstance[], BattleParticipantNPC>> modeInitialisers = new Dictionary<Mode, Func<string, PokemonInstance[], BattleParticipantNPC>>()
        {
            { Mode.RandomAttack, (n, pmon) => new RandomAttack(n, pmon) },
            { Mode.Debug_UseRandomMedicineItem, (n, pmon) => new RandomMedicineItem(n, pmon) },
            { Mode.Debug_UseRandomBattleItem, (n, pmon) => new RandomBattleItem(n, pmon) }
        };

        #endregion

        protected string npcName;
        public override string GetName() => npcName;

        protected PokemonInstance[] pokemon;
        public override PokemonInstance[] GetPokemon() => pokemon;

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

    }
}