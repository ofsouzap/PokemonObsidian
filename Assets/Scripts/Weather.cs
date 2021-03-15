using System.Collections.Generic;

public class Weather : IHasId
{

    #region Registry

    public static Registry<Weather> registry = new Registry<Weather>();

    public static Weather GetWeatherById(int id)
    {
        return registry.StartingIndexSearch(id, id);
    }

    #endregion

    public int id;
    public int GetId() => id;

    public const float damageMaxHealthProportion = 0.0625F;

    /// <summary>
    /// The name of the weather
    /// </summary>
    public string name;

    /// <summary>
    /// The announcement message to show for this weather. If null, won't be used
    /// </summary>
    public string announcement;

    /// <summary>
    /// The path to the texture to show the weather eg. when in battle or in free roam
    /// </summary>
    public string textureResourcePath;

    /// <summary>
    /// The speed at which the texture will move diagonally. This is initially for rain and snow
    /// </summary>
    public int textureMoveSpeed = 0;

    #region Effects

    /// <summary>
    /// The pokemon types that are damaged by this weather. Damage is always 1/16 of a pokemon's maximum health (unless an ability prevents it). For dual-type pokemon, both of its types must be affected to take damage
    /// </summary>
    public Pokemon.Type[] damagedPokemonTypes = new Pokemon.Type[0];

    /// <summary>
    /// The (elemental) types of moves that are boosted by this weather. Boost is applied in modifiers calculation. Boost is +50%
    /// </summary>
    public Pokemon.Type[] boostedMoveTypes = new Pokemon.Type[0];

    /// <summary>
    /// The (elemental) types of moves that are weakened by this weather. Weakening is applied in modifiers calculation. Weakening is -50%
    /// </summary>
    public Pokemon.Type[] weakenedMoveTypes = new Pokemon.Type[0];

    /// <summary>
    /// The stat boosts that are applied to pokemon of each type. Values are multipliers for a stat to have applied
    /// </summary>
    public Dictionary<Pokemon.Type, Pokemon.Stats<float>> typeStatBoosts = new Dictionary<Pokemon.Type, Pokemon.Stats<float>>();

    /// <summary>
    /// How much pokemon should have their accuracy changed. Value is multiplier for the stat. Value can be negative
    /// </summary>
    public float accuracyBoost = 1;

    /// <summary>
    /// Non-volatile status conditions that can't be gained during this weather
    /// </summary>
    public Pokemon.PokemonInstance.NonVolatileStatusCondition[] immuneNonVolatileStatusConditions = new Pokemon.PokemonInstance.NonVolatileStatusCondition[0];

    #endregion

    #region Creating Registry

    public void CreateWeathers()
    {

        registry.SetValues(new Weather[]
        {

            new Weather()
            {
                id = 0,
                name = "Clear Sky",
                announcement = null,
                textureResourcePath = "textures/weather/clear"
            },

            new Weather()
            {
                id = 1,
                name = "Harsh Sunlight",
                announcement = "The sun is harsh",
                textureResourcePath = "textures/weather/harshSunlight",
                immuneNonVolatileStatusConditions = new Pokemon.PokemonInstance.NonVolatileStatusCondition[] { Pokemon.PokemonInstance.NonVolatileStatusCondition.Frozen },
                boostedMoveTypes = new Pokemon.Type[] { Pokemon.Type.Fire },
                weakenedMoveTypes = new Pokemon.Type[] { Pokemon.Type.Water }
            },

            new Weather()
            {
                id = 2,
                name = "Rain",
                announcement = "It is raining",
                textureResourcePath = "textures/weather/rain",
                boostedMoveTypes = new Pokemon.Type[] { Pokemon.Type.Water },
                weakenedMoveTypes = new Pokemon.Type[] { Pokemon.Type.Fire }
            },

            new Weather()
            {
                id = 3,
                name = "Sandstorm",
                announcement = "There is a sandstorm",
                textureResourcePath = "textures/weather/sandstorm",
                damagedPokemonTypes = new Pokemon.Type[]
                {
                    Pokemon.Type.Normal,
                    Pokemon.Type.Fire,
                    Pokemon.Type.Fighting,
                    Pokemon.Type.Water,
                    Pokemon.Type.Flying,
                    Pokemon.Type.Grass,
                    Pokemon.Type.Poison,
                    Pokemon.Type.Electric,
                    Pokemon.Type.Psychic,
                    Pokemon.Type.Ice,
                    Pokemon.Type.Bug,
                    Pokemon.Type.Dragon,
                    Pokemon.Type.Ghost,
                    Pokemon.Type.Dark,
                    Pokemon.Type.Steel
                }, //All types but rock and ground
                typeStatBoosts = new Dictionary<Pokemon.Type, Pokemon.Stats<float>>()
                {
                    { Pokemon.Type.Rock, new Pokemon.Stats<float>()  { specialDefense = 1.5f } }
                }
            },

            new Weather()
            {
                id = 4,
                name = "Hail",
                announcement = "It is hailing",
                textureResourcePath = "textures/weather/hail",
                damagedPokemonTypes = new Pokemon.Type[]
                {
                    Pokemon.Type.Normal,
                    Pokemon.Type.Fire,
                    Pokemon.Type.Fighting,
                    Pokemon.Type.Water,
                    Pokemon.Type.Flying,
                    Pokemon.Type.Grass,
                    Pokemon.Type.Poison,
                    Pokemon.Type.Electric,
                    Pokemon.Type.Ground,
                    Pokemon.Type.Psychic,
                    Pokemon.Type.Rock,
                    Pokemon.Type.Bug,
                    Pokemon.Type.Dragon,
                    Pokemon.Type.Ghost,
                    Pokemon.Type.Dark
                } //All types but ice and steel
            },

            new Weather()
            {
                id = 5,
                name = "Fog",
                announcement = "The fog is thick",
                textureResourcePath = "textures/weather/fog",
                accuracyBoost = 0.9f
            }

        });

    }

    #endregion

}
