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
            Debug_UseRandomBattleItem,
#endif
            BasicTrainer,
            WildPokemon
            //TODO - add more
        }

        public static readonly Dictionary<Mode, Func<string, PokemonInstance[], byte, BattleParticipantNPC>> modeInitialisers = new Dictionary<Mode, Func<string, PokemonInstance[], byte, BattleParticipantNPC>>()
        {
            { Mode.RandomAttack, (n, pmon, bp) => new RandomAttack(n, pmon, bp) },
#if UNITY_EDITOR
            { Mode.Debug_UseRandomMedicineItem, (n, pmon, bp) => new RandomMedicineItem(n, pmon, bp) },
            { Mode.Debug_UseRandomBattleItem, (n, pmon, bp) => new RandomBattleItem(n, pmon, bp) },
#endif
            { Mode.BasicTrainer, (n, pmon, bp) => new BasicTrainer(n, pmon, bp) },
            { Mode.WildPokemon, (n, pmon, bp) => new WildPokemon(n, pmon, bp) }
        };

        #endregion

        protected string npcName;
        public override string GetName() => npcName;

        protected PokemonInstance[] pokemon;
        public override PokemonInstance[] GetPokemon() => pokemon;

        public byte basePayout { get; protected set; }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

    }
}