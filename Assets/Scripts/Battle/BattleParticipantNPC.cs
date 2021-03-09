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

        public PokemonInstance[] pokemon;

        public BattleParticipantNPC(Mode mode,
            PokemonInstance[] pokemon)
        {
            this.mode = mode;
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

        }

        public Action ChooseAction_RandomAttack(BattleData battleData)
        {

            int movesCount = 0;
            foreach (int moveId in pokemon[activePokemonIndex].moveIds)
                if (moveId != 0)
                    movesCount++;

            int chosenMoveIndex = Random.Range(0, movesCount);

            return new Action()
            {
                type = Action.Type.Fight,
                fightMoveIndex = chosenMoveIndex,
                fightMoveUser = ActivePokemon
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