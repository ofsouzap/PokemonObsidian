using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Items;

namespace Battle.NPCBattleParticipantModes
{
    public class RandomBattleItem : Generic
    {

        public RandomBattleItem(TrainersData.TrainerDetails details)
        {
            npcName = details.GetFullName();
            pokemon = details.GenerateParty();
            basePayout = details.GetBasePayout();
            defeatMessages = details.defeatMessages;
        }

        public override void StartChoosingAction(BattleData battleData)
        {

            base.StartChoosingAction(battleData);

            if (actionHasBeenChosen)
                return;

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
                actionItem = validItems[battleData.RandomRange(0, validItems.Count)];
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
