using Pokemon;

namespace Items.MedicineItems
{
    public class HealthMedicineItem : MedicineItem
    {

        public bool fullyHeals = false;
        public int healAmount;

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
            => new ItemUsageEffects()
            {
                healthRecovered = fullyHeals
                    ? pokemon.GetStats().health - pokemon.health
                    : (healAmount + pokemon.health > pokemon.GetStats().health
                        ? pokemon.GetStats().health - pokemon.health
                        : healAmount)
            };

        public override bool CheckCompatibility(PokemonInstance pokemon)
            => pokemon.health < pokemon.GetStats().health && !pokemon.IsFainted;

    }
}
