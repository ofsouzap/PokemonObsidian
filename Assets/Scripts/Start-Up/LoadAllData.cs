using Pokemon;
using Items;
using Items.PokeBalls;
using Items.MedicineItems;

namespace StartUp
{
    public static class LoadAllData
    {

        public static void Load()
        {

            Weather.CreateWeathers();
            PokemonSpeciesData.LoadData();
            PokemonMoveData.LoadData();
            Nature.LoadRegistry();
            SpriteStorage.TryLoadAll();

            Item.TrySetRegistry(); //TMs must be loaded after moves

        }

    }
}
