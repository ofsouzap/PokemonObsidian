using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;
using Pokemon;

namespace Battle
{
    public class BattleParticipantNPC : BattleParticipant
    {

        #region Mode

        public enum Mode
        {
            RandomAttack
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

                default:
                    Debug.LogError("Unknown npc participant mode - " + mode);
                    actionHasBeenChosen = false;
                    break;

                //TODO - continue once more added

            }

            if (!chosenAction.fightUsingStruggle
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
            return pokemon.All((x) => x.IsFainted);
        }

    }
}