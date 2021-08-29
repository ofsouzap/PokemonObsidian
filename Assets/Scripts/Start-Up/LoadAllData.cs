using Pokemon;
using Items;
using FreeRoaming;
using FreeRoaming.PokeMart;

namespace StartUp
{
    public static class LoadAllData
    {

        public static void Load()
        {

            SceneArea.TryLoadRegistry();
            Gym.TryLoadRegistry();
            Weather.CreateWeathers();
            PokemonSpeciesData.LoadData();
            PokemonMoveData.LoadData();
            Nature.LoadRegistry();
            SpriteStorage.TryLoadAll();

            Item.TrySetRegistry(); //TMs must be loaded after moves

            DroppedItem.TryLoadRegistry(); //Dropped items must be loaded after items

            PokeMartData.LoadData(); //This must be loaded after all items

        }

    }
}
