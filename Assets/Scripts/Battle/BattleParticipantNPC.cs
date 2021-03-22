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

            if (ActivePokemon.movePPs[chosenAction.fightMoveIndex] <= 0)
            {
                throw new System.Exception("Chosen fight move doesn't have enough PP");
            }

        }

        public Action ChooseAction_RandomAttack(BattleData battleData)
        {

            int chosenMoveIndex;
            bool selectingMove = true;

            do
            {
                int movesCount = 0;
                foreach (int moveId in pokemon[activePokemonIndex].moveIds)
                    if (moveId != 0)
                        movesCount++;

                chosenMoveIndex = Random.Range(0, movesCount);

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

                if (pokemon[i].health > 0)
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
            return pokemon.All((x) => x.health <= 0);
        }

    }
}