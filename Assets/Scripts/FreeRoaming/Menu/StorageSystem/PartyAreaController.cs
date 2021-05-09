using UnityEngine;
using Pokemon;

namespace FreeRoaming.Menu.StorageSystem
{
    public class PartyAreaController : MonoBehaviour
    {

        private PointerController pointer;
        public void SetPointerGameObject(PointerController pointer)
        {
            this.pointer = pointer;
        }

        [Tooltip("The length of this should be 6")]
        public PartyPokemonPositionController[] partyPokemonPositions;

        private StorageSystemMenuController menuController;

        public void SetUp(StorageSystemMenuController menuController)
        {

            this.menuController = menuController;

            for (byte partyIndex = 0; partyIndex < partyPokemonPositions.Length; partyIndex++)
            {

                byte index = partyIndex;

                PartyPokemonPositionController controller = partyPokemonPositions[partyIndex];

                controller.SetPointerGameObject(pointer);

                controller.SetOnSelectAction(() =>
                {
                    PokemonInstance controllerPokemon = controller.GetCurrentPokemon();
                    if (controllerPokemon != null)
                        this.menuController.SetDetailsPokemon(controllerPokemon);
                });

                controller.OnClick.AddListener(() =>
                {
                    menuController.PartyPokemonClicked(index);
                });

            }

        }

        public void SetPokemon(PokemonInstance[] pokemon)
        {

            if (pokemon.Length > partyPokemonPositions.Length)
            {
                Debug.LogError("Too many pokemon provided for number of party Pokemon positions");
                return;
            }

            foreach (PartyPokemonPositionController controller in partyPokemonPositions)
                controller.SetPokemon(null);

            for (byte i = 0; i < pokemon.Length; i++)
            {
                partyPokemonPositions[i].SetPokemon(pokemon[i]);
            }

        }

    }
}