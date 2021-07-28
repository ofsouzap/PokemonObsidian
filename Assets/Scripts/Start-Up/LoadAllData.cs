using Pokemon;
using Items;
using FreeRoaming.PokeMart;

namespace StartUp
{
    public static class LoadAllData
    {

        public static void Load()
        {

            Gym.TryLoadRegistry();
            Weather.CreateWeathers();
            PokemonSpeciesData.LoadData();
            PokemonMoveData.LoadData();
            Nature.LoadRegistry();
            SpriteStorage.TryLoadAll();

            Item.TrySetRegistry(); //TMs must be loaded after moves

            PokeMartData.LoadData(); //This must be loaded after all items

        }

    }
}
