using System.Collections;
using UnityEngine;
using FreeRoaming.AreaEntranceArguments;

namespace FreeRoaming.AreaControllers
{
    public class PokemonCenterSceneController : FreeRoamSceneController
    {

        public Vector2Int respawnPosition = Vector2Int.zero;

        protected override void Start()
        {

            base.Start();

            //If pokemon center entrance arguments aren't set or the id is 0, no special objects will be placed in the pokemon center
            if (PokemonCenterEntranceArguments.argumentsSet && PokemonCenterEntranceArguments.pokemonCenterId != 0)
            {

                //TODO - place objects in scene depending on id in PokemonCenterEntranceArguments

            }

            //Set the player's respawn point as this pokemon center
            PlayerController.singleton.RefreshRespawnSceneStackFromCurrent(respawnPosition);

        }

    }
}