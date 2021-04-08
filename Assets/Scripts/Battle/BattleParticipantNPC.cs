using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Battle
{
    public class BattleParticipantNPC : BattleParticipant
    {

        #region Mode

        public enum Mode
        {
            RandomAttack,
#if UNITY_EDITOR
            Debug_UseRandomMedicineItem,
            Debug_UseRandomBattleItem
#endif
            //TODO - add more
        }

        public Mode mode;

        #endregion

        private string npcName;
        public override string GetName() => npcName;

        public PokemonInstance[] pokemon;

        public BattleParticipantNPC(Mode mode,
            string name,
            PokemonInstance[] pokemon)
        {
            this.mode = mode;
            npcName = name;
            this.pokemon = pokemon;
        }

        public override PokemonInstance[] GetPokemon() => pokemon;

        #region Action Choosing

        public override void StartChoosingAction(BattleData battleData)
        {

            actionHasBeenChosen = true;

            switch (mode)
            {

                case Mode.RandomAttack:
                    chosenAction = ChooseAction_RandomAttack(battleData);
                    break;

#if UNITY_EDITOR

                case Mode.Debug_UseRandomMedicineItem:
                    chosenAction = ChooseAction_Debug_UseRandomMedicineItem(battleData);
                    break;

                case Mode.Debug_UseRandomBattleItem:
                    chosenAction = ChooseAction_Debug_UseRandomBattleItem(battleData);
                    break;

#endif

                default:
                    Debug.LogError("Unknown npc participant mode - " + mode);
                    actionHasBeenChosen = false;
                    break;

                //TODO - continue once more added

            }

            if (chosenAction.type == Action.Type.Fight
                && !chosenAction.fightUsingStruggle
                && ActivePokemon.movePPs[chosenAction.fightMoveIndex] <= 0)
            {
                throw new System.Exception("Chosen fight move doesn't have enough PP");
            }

        }

        public Action ChooseAction_RandomAttack(BattleData battleData)
        {

            if (!ActivePokemon.HasUsableMove)
            {
                return new Action(this)
                {
                    type = Action.Type.Fight,
                    fightUsingStruggle = true,
                    fightMoveTarget = battleData.participantPlayer
                };
            }

            int chosenMoveIndex;
            bool selectingMove = true;

            do
            {
                List<int> validMoveIndexes = new List<int>();

                for (int moveIndex = 0; moveIndex < 4; moveIndex++)
                    if (!Pokemon.Moves.PokemonMove.MoveIdIsUnset(ActivePokemon.moveIds[moveIndex]))
                        validMoveIndexes.Add(moveIndex);

                chosenMoveIndex = validMoveIndexes[Random.Range(0, validMoveIndexes.Count)];

                if (ActivePokemon.movePPs[chosenMoveIndex] > 0)
                    selectingMove = false;

            }
            while (selectingMove);

            return new Action(this)
            {
                type = Action.Type.Fight,
                fightMoveIndex = chosenMoveIndex,
                fightMoveTarget = battleData.participantPlayer
            };

        }

#if UNITY_EDITOR

        public Action ChooseAction_Debug_UseRandomMedicineItem(BattleData battleData)
        {

            int actionItemTargetIndex = activePokemonIndex;
            Item actionItem;

            List<MedicineItem> validItems = new List<MedicineItem>();

            foreach (MedicineItem item in MedicineItem.registry)
            {
                if (item.CheckCompatibility(GetPokemon()[actionItemTargetIndex]))
                {
                    validItems.Add(item);
                }
            }

            if (validItems.Count > 0)
                actionItem = validItems[Random.Range(0, validItems.Count)];
            else
            {
                Debug.LogWarning("No valid item found. Choosing item with id 0");
                actionItem = MedicineItem.GetMedicineItemById(0);
            }

            int moveIndex = -1;

            if (actionItem is PPRestoreMedicineItem)
            {
                PPRestoreMedicineItem ppRestoreActionItem = (PPRestoreMedicineItem)actionItem;
                if (ppRestoreActionItem.isForSingleMove)
                {
                    for (int i = 0; i < GetPokemon()[actionItemTargetIndex].moveIds.Length; i++)
                    {
                        if (!Pokemon.Moves.PokemonMove.MoveIdIsUnset(GetPokemon()[actionItemTargetIndex].moveIds[i]))
                        {
                            Pokemon.Moves.PokemonMove move = Pokemon.Moves.PokemonMove.GetPokemonMoveById(GetPokemon()[actionItemTargetIndex].moveIds[i]);
                            if (GetPokemon()[actionItemTargetIndex].movePPs[i] < move.maxPP)
                            {
                                moveIndex = i;
                                break;
                            }
                        }
                    }
                }
            }

            return new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = actionItem,
                useItemTargetPartyIndex = actionItemTargetIndex,
                useItemTargetMoveIndex = moveIndex
            };

        }

        public Action ChooseAction_Debug_UseRandomBattleItem(BattleData battleData)
        {

            int actionItemTargetIndex = activePokemonIndex;
            Item actionItem;

            List<BattleItem> validItems = new List<BattleItem>();

            foreach (BattleItem item in BattleItem.registry)
            {
                if (item.CheckCompatibility(GetPokemon()[actionItemTargetIndex]))
                {
                    validItems.Add(item);
                }
            }

            if (validItems.Count > 0)
                actionItem = validItems[Random.Range(0, validItems.Count)];
            else
            {
                Debug.LogWarning("No valid item found. Choosing item with id 0");
                actionItem = BattleItem.GetBattleItemById(0);
            }

            return new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = actionItem,
                useItemTargetPartyIndex = actionItemTargetIndex
            };

        }

#endif

        #endregion

        public override void StartChoosingNextPokemon()
        {

            //All opponents use their pokemon in the order they have them

            int selectedIndex = -1;
            bool validIndexFound = false;

            for (int i = 0; i < pokemon.Length; i++)
            {

                if (!pokemon[i].IsFainted)
                {
                    selectedIndex = i;
                    validIndexFound = true;
                    break;
                }

            }

            if (!validIndexFound)
            {
                Debug.LogError("Unable to find valid next pokemon index");
                nextPokemonHasBeenChosen = false;
                return;
            }

            chosenNextPokemonIndex = selectedIndex;
            nextPokemonHasBeenChosen = true;

        }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

    }
}