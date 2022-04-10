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
            Obsolete, //This shouldn't be used anymore
            GymLeader
        }

        public const Mode defaultTrainerMode = Mode.BasicTrainer;
        public const Mode defaultGymLeaderMode = Mode.GymLeader;

        public static bool TryParse(string s,
            out Mode m)
        {

            switch (s.ToLower().Replace(" ", ""))
            {

                case "":
                    m = defaultTrainerMode;
                    break;

                case "randomattack":
                    m = Mode.RandomAttack;
                    break;

                case "basictrainer":
                    m = Mode.BasicTrainer;
                    break;

                default:
                    m = default;
                    return false;

            }

            return true;

        }

        public static readonly Dictionary<Mode, Func<TrainersData.TrainerDetails, BattleParticipantNPC>> modeInitialisers = new Dictionary<Mode, Func<TrainersData.TrainerDetails, BattleParticipantNPC>>()
        {
            { Mode.RandomAttack, (details) => new RandomAttack(details) },
            { Mode.Debug_UseRandomMedicineItem, (details) => new RandomMedicineItem(details) },
            { Mode.Debug_UseRandomBattleItem, (details) => new RandomBattleItem(details) },
            { Mode.BasicTrainer, (details) => new BasicTrainer(details) },
            { Mode.GymLeader, (details) => new GymLeader(details) }
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

        public override void ChooseActionFight(BattleData battleData, bool useStruggle, int moveIndex)
        {

            SetChosenAction(new Action(this)
            {
                type = Action.Type.Fight,
                fightUsingStruggle = useStruggle,
                fightMoveTarget = battleData.participantPlayer,
                fightMoveIndex = moveIndex
            });

        }

    }
}