using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Battle.NPCBattleParticipantModes
{
    public class RandomAttack : Generic
    {

        public RandomAttack(string name,
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

            base.StartChoosingAction(battleData);

            if (actionHasBeenChosen)
                return;

            if (!ActivePokemon.HasUsableMove)
            {
                chosenAction = new Action(this)
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

                chosenMoveIndex = validMoveIndexes[battleData.RandomRange(0, validMoveIndexes.Count)];

                if (ActivePokemon.movePPs[chosenMoveIndex] > 0)
                    selectingMove = false;

            }
            while (selectingMove);

            chosenAction = new Action(this)
            {
                type = Action.Type.Fight,
                fightMoveIndex = chosenMoveIndex,
                fightMoveTarget = battleData.participantPlayer
            };

            actionHasBeenChosen = true;

        }

    }
}