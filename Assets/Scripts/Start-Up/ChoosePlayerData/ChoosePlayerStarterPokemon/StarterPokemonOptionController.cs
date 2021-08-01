using Pokemon;

namespace StartUp.ChoosePlayerData.ChoosePlayerStarterPokemon
{
    public class StarterPokemonOptionController : GenericImageOptionController<int>
    {

        public override void RefreshImage()
        {

            image.sprite = SpriteStorage.GetPokemonSprite(
                PokemonSpecies.GetPokemonSpeciesById(instanceId).resourceName,
                PokemonSpecies.SpriteType.Front1,
                false);

        }

    }
}