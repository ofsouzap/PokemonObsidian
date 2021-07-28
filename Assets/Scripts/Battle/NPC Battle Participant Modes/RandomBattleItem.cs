using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Items;

#if UNITY_EDITOR

namespace Battle.NPCBattleParticipantModes
{
    public class RandomBattleItem : Generic
    {

        public RandomBattleItem(string name,
            PokemonInstance[] pokemon,
            byte basePayout,
            string[] defeatMessages)
        {
            npcName = name;
            this.pokemon = pokemon;
            this.basePayout = basePayout;
            this.defeatMessages = defeatMessages;
        }

        public override void StartChoosingAction(BattleData battleData)
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

            chosenAction = new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = actionItem,
                useItemTargetPartyIndex = actionItemTargetIndex
            };

            actionHasBeenChosen = true;

        }

    }
}

#endif
