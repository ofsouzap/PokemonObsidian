namespace Pokemon
{

    public enum GrowthType
    {
        Slow,
        MediumSlow,
        MediumFast,
        Fast,
        Erratic,
        Fluctuating
    }

    public static class GrowthTypeFunc
    {
        public static GrowthType Parse(string x)
        {

            switch (x.ToLower())
            {

                case "slow": return GrowthType.Slow;
                case "mediumslow": return GrowthType.MediumSlow;
                case "mediumfast": return GrowthType.MediumFast;
                case "fast": return GrowthType.Fast;
                case "erratic": return GrowthType.Erratic;
                case "fluctuating": return GrowthType.Fluctuating;

                default:
                    throw new System.FormatException("Unknown growth type string passed");

            }

        }
    }

}
