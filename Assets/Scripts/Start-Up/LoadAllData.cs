using Pokemon;
using Items;
using FreeRoaming;
using FreeRoaming.PokeMart;
using FreeRoaming.WildPokemonArea;
using FreeRoaming.NPCs;

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
            WildPokemonAreaData.LoadData();
            TrainersData.LoadData();

            Item.TrySetRegistry(); //TMs must be loaded after moves

            GenericNPCData.LoadData(); //Generic NPCs must be loaded after items

            DroppedItem.TryLoadRegistry(); //Dropped items must be loaded after items

            PokeMartData.LoadData(); //This must be loaded after all items

        }

    }
}
