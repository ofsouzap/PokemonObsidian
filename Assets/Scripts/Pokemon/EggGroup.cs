namespace Pokemon
{

    public enum EggGroup
    {
        Monster,
        HumanLike,
        Water1,
        Water2,
        Water3,
        Bug,
        Mineral,
        Flying,
        Amorphous,
        Field,
        Fairy,
        Grass,
        Dragon
    }

    public static class EggGroupFunc
    {
        public static EggGroup Parse(string x)
        {

            EggGroup? output = x switch
            {
                "monster" => EggGroup.Monster,
                "human-like" => EggGroup.HumanLike,
                "water 1" => EggGroup.Water1,
                "water 2" => EggGroup.Water2,
                "water 3" => EggGroup.Water3,
                "bug" => EggGroup.Bug,
                "mineral" => EggGroup.Mineral,
                "flying" => EggGroup.Flying,
                "amorphous" => EggGroup.Amorphous,
                "field" => EggGroup.Field,
                "fairy" => EggGroup.Fairy,
                "grass" => EggGroup.Grass,
                "dragon" => EggGroup.Dragon,
                _ => null
            };

            if (output == null)
                throw new System.FormatException("Unknown growth type string passed");

            return (EggGroup)output;

        }
    }

}
