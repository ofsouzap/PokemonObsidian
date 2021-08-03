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
            Debug_UseRandomMedicineItem,
            Debug_UseRandomBattleItem,
            BasicTrainer,
            WildPokemon
            //TODO - add more
        }

        public static readonly Dictionary<Mode, Func<string, PokemonInstance[], byte, string[], BattleParticipantNPC>> modeInitialisers = new Dictionary<Mode, Func<string, PokemonInstance[], byte, string[], BattleParticipantNPC>>()
        {
            { Mode.RandomAttack, (n, pmon, bp, dmsgs) => new RandomAttack(n, pmon, bp, dmsgs) },
            { Mode.Debug_UseRandomMedicineItem, (n, pmon, bp, dmsgs) => new RandomMedicineItem(n, pmon, bp, dmsgs) },
            { Mode.Debug_UseRandomBattleItem, (n, pmon, bp, dmsgs) => new RandomBattleItem(n, pmon, bp, dmsgs) },
            { Mode.BasicTrainer, (n, pmon, bp, dmsgs) => new BasicTrainer(n, pmon, bp, dmsgs) },
            { Mode.WildPokemon, (n, pmon, bp, dmsgs) => new WildPokemon(n, pmon, bp, dmsgs) }
        };

        #endregion

        protected string npcName;
        public override string GetName() => npcName;

        protected PokemonInstance[] pokemon;
        public override PokemonInstance[] GetPokemon() => pokemon;

        public byte basePayout { get; protected set; }

        /// <summary>
        /// Any messages the NPC should say if the player wins
        /// </summary>
        public string[] defeatMessages { get; protected set; }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

    }
}