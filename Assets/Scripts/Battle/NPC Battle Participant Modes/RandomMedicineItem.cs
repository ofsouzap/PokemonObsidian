using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Items;
using Items.MedicineItems;

#if UNITY_EDITOR

namespace Battle.NPCBattleParticipantModes
{
    public class RandomMedicineItem : Generic
    {

        public RandomMedicineItem(string name,
            PokemonInstance[] pokemon,
            byte basePayout)
        {
            npcName = name;
            this.pokemon = pokemon;
            this.basePayout = basePayout;
        }

        public override void StartChoosingAction(BattleData battleData)
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

            chosenAction = new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = actionItem,
                useItemTargetPartyIndex = actionItemTargetIndex,
                useItemTargetMoveIndex = moveIndex
            };
            actionHasBeenChosen = true;

        }

    }
}

#endif
