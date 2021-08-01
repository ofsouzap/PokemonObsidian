using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FreeRoaming;
using Pokemon;
using Items;
using Items.PokeBalls;

public static class SpriteStorage
{

    public const string spritesResourcesPrefix = "Sprites/";

    public enum SpriteType
    {
        Character,
        Item,
        Symbol,
        TypeParticle,
        BattleSprite,
        Pokemon,
        PokeBall,
        BattleBackgroundAndCircle
    }

    private static Dictionary<SpriteType, Dictionary<string, Sprite>> sprites = new Dictionary<SpriteType, Dictionary<string, Sprite>>();

    private static bool allSpritesLoaded = false;

    public static void TryLoadAll()
    {

        if (!allSpritesLoaded)
            LoadAllSprites();

    }

    /// <summary>
    /// Load all sprites from a sprite sheet
    /// </summary>
    /// <param name="spriteSheetName">The name of the sprite sheet. The sheet should be in Resources/Sprites</param>
    /// <param name="condition">A condition under which a loaded sprte should be returned (eg. if only some of a sprite sheet is for a certain sprite category)</param>
    /// <returns>The array of sprites loaded from the sheet</returns>
    public static Sprite[] LoadSpriteSheet(string spriteSheetName,
        Predicate<Sprite> condition)
    {
        return Resources.LoadAll<Sprite>(spritesResourcesPrefix + spriteSheetName).Where(sprite => condition(sprite)).ToArray();
    }

    /// <summary>
    /// Load all sprites from a sprite sheet
    /// </summary>
    /// <param name="spriteSheetName">The name of the sprite sheet. The sheet should be in Resources/Sprites</param>
    /// <returns>The array of sprites loaded from the sheet</returns>
    public static Sprite[] LoadSpriteSheet(string spriteSheetName)
        => LoadSpriteSheet(spriteSheetName, (x) => true);

    private static void SetSpriteTypeSprites(SpriteType spriteType,
        Sprite[] sprites)
    {

        SpriteStorage.sprites[spriteType] = new Dictionary<string, Sprite>();

        foreach (Sprite sprite in sprites)
        {
            SpriteStorage.sprites[spriteType].Add(sprite.name, sprite);
        }

    }

    private static void SetSpriteTypeSpritesWithCustomNames(SpriteType spriteType,
        Dictionary<Sprite, string> spritesAndNames)
    {

        sprites[spriteType] = new Dictionary<string, Sprite>();

        foreach (Sprite sprite in spritesAndNames.Keys)
        {
            sprites[spriteType].Add(spritesAndNames[sprite], sprite);
        }

    }

    private static void LoadAllSprites()
    {

        sprites = new Dictionary<SpriteType, Dictionary<string, Sprite>>();

        SetSpriteTypeSprites(SpriteType.Character, LoadCharacterSprites());
        SetSpriteTypeSprites(SpriteType.Item, LoadItemSprites());
        SetSpriteTypeSprites(SpriteType.Symbol, LoadTypeSymbolSprites());
        SetSpriteTypeSprites(SpriteType.TypeParticle, LoadTypeParticleSprites());
        SetSpriteTypeSprites(SpriteType.BattleSprite, LoadBattleSprites());
        SetSpriteTypeSpritesWithCustomNames(SpriteType.Pokemon, LoadPokemonSprites());
        SetSpriteTypeSprites(SpriteType.PokeBall, LoadPokeBallSprites());
        SetSpriteTypeSprites(SpriteType.BattleBackgroundAndCircle, LoadBattleBackgroundAndCircleSprites());

        allSpritesLoaded = true;

    }

    public static Sprite GetSprite(SpriteType spriteType,
        string name,
        bool warnOnFail = true)
    {

        if (!allSpritesLoaded)
        {
            Debug.LogWarning("Sprites weren't automatically loaded, loading now");
            LoadAllSprites();
        }

        if (sprites.ContainsKey(spriteType))
        {
            if (sprites[spriteType].ContainsKey(name))
            {
                return sprites[spriteType][name];
            }
            else
            {
                if (warnOnFail)
                    Debug.LogWarning("Couldn't find sprite name - " + name);
                return null;
            }
        }
        else
        {
            if (warnOnFail)
                Debug.LogError("Invalid spriteType - " + spriteType);
            return null;
        }

    }

    #region Characters

    private static readonly string[] characterSpriteSpriteSheets = new string[]
    {
        "sprite_sheet_npcs",
        "sprite_sheet_player_male_0",
        "sprite_sheet_player_male_1",
        "sprite_sheet_player_male_2",
        "sprite_sheet_player_female_0",
        "sprite_sheet_player_female_1",
        "sprite_sheet_player_female_2",
        "sprite_sheet_npcs_battlesprites"
    };

    private static Sprite[] LoadCharacterSprites()
    {

        List<Sprite> sprites = new List<Sprite>();

        sprites.AddRange(LoadCharacterSpritesInDefaultDirectory());
        foreach (string spriteSheetName in characterSpriteSpriteSheets)
            sprites.AddRange(LoadSpriteSheet(spriteSheetName));

        return sprites.ToArray();

    }

    private static Sprite[] LoadCharacterSpritesInDefaultDirectory()
    {

        string resourcePath = spritesResourcesPrefix + "Characters";

        return Resources.LoadAll<Sprite>(resourcePath);

    }

    public static Sprite GetCharacterSprite(string name) => GetSprite(SpriteType.Character, name);

    /// <summary>
    /// Gets the battle sprite for a character
    /// </summary>
    /// <param name="spriteName">The name of the sprite</param>
    /// <returns>The sprite found</returns>
    public static Sprite GetCharacterBattleSprite(string spriteName)
    {
        return GetSprite(SpriteType.Character,
            spriteName + "_battlesprite");
    }

    private static string GenerateCharacterSpriteFullIdentifier(string spriteName,
            string stateIdentifier,
            GameCharacterController.FacingDirection direction,
            int index = -1)
    {

        string directionIdentifier;

        switch (direction)
        {

            case GameCharacterController.FacingDirection.Down:
                directionIdentifier = "d";
                break;

            case GameCharacterController.FacingDirection.Left:
                directionIdentifier = "l";
                break;

            case GameCharacterController.FacingDirection.Up:
                directionIdentifier = "u";
                break;

            default:
                Debug.LogWarning($"Invalid direction facing ({direction})");
                directionIdentifier = "l";
                break;

        }

        return spriteName + '_' + stateIdentifier + '_' + directionIdentifier + (index == -1 ? "" : '_' + index.ToString());

    }

    /// <summary>
    /// Gets a sprite referenced by a state identifier and a FacingDirection direction. The full identifier will then be formed from these parameters
    /// </summary>
    /// /// <param name="flipSprite">Whether the sprite should be flipped once returned</param>
    /// <param name="spriteName">The name of the sprite</param>
    /// <param name="stateIdentifier">The name of the state that is being requested (eg. "neutral")</param>
    /// <param name="direction">The direction of sprite to request</param>
    /// <param name="index">The index of the sprite to request</param>
    /// <returns>The sprite as specified if found, otherwise null</returns>
    public static Sprite GetCharacterSprite(out bool flipSprite,
        string spriteName,
        string stateIdentifier,
        GameCharacterController.FacingDirection direction,
        int index = -1)
    {

        string identifier = GenerateCharacterSpriteFullIdentifier(spriteName,
            stateIdentifier,
            direction == GameCharacterController.FacingDirection.Right ? GameCharacterController.FacingDirection.Left : direction,
            index);

        flipSprite = direction == GameCharacterController.FacingDirection.Right;

        return GetSprite(SpriteType.Character,
            identifier);

    }

    #endregion

    #region Items

    private const string itemSpritePrefix = "itemsprite_";
    private const string itemSpriteSheetName = "sprite_sheet_itemsprites";
    private const string battleItemSpriteSheetName = "sprite_sheet_battleitemsprites";
    private const string tmItemsSpriteSheetName = "sprite_sheet_itemsprites_tmitems";

    private const string tmItemSpritePrefix = "itemsprite_tm_";

    private static Sprite[] LoadItemSprites()
    {
        List<Sprite> sprites = new List<Sprite>();
        sprites.AddRange(LoadSpriteSheet(itemSpriteSheetName));
        sprites.AddRange(LoadSpriteSheet(battleItemSpriteSheetName));
        sprites.AddRange(LoadSpriteSheet(tmItemsSpriteSheetName));
        return sprites.ToArray();
    }

    public static Sprite GetItemSprite(string itemResourceName)
    {

        string spriteName = itemSpritePrefix + itemResourceName;
        return GetSprite(SpriteType.Item, spriteName);

    }

    public static Sprite GetTMSprite(Pokemon.Type moveType)
    {

        string resourceName = tmItemSpritePrefix + TypeFunc.GetTypeResourceName(moveType);
        return GetSprite(SpriteType.Item, resourceName);

    }

    #endregion

    #region Symbols

    private const string typeSymbolsSpriteSheetName = "sprite_sheet_symbols";
    private const string typeSymbolSpritePrefix = "type_";
    private const string moveCategorySymbolSpritePrefix = "movecategory_";

    private static Sprite[] LoadTypeSymbolSprites()
    {
        return LoadSpriteSheet(typeSymbolsSpriteSheetName);
    }

    public static Sprite GetTypeSymbolSprite(Pokemon.Type type)
    {

        return GetSprite(SpriteType.Symbol,
            typeSymbolSpritePrefix + TypeFunc.GetTypeResourceName(type));

    }

    public static Sprite GetMoveTypeSprite(Pokemon.Moves.PokemonMove.MoveType moveType)
    {

        return GetSprite(SpriteType.Symbol,
            moveCategorySymbolSpritePrefix + Pokemon.Moves.PokemonMove.GetMoveTypeResourceName(moveType));

    }

    #endregion

    #region Type Particles

    private const string typeParticlesPath = "Effects/Generic Pokemon Move/";
    private const string typeParticlePrefix = "particle_";

    private static Sprite[] LoadTypeParticleSprites()
    {

        List<Sprite> sprites = new List<Sprite>();

        foreach (Pokemon.Type type in Enum.GetValues(typeof(Pokemon.Type)))
        {

            if (type == Pokemon.Type.Normal)
                continue;

            string resourceName = spritesResourcesPrefix + typeParticlesPath + typeParticlePrefix + TypeFunc.GetTypeResourceName(type);

            Sprite typeParticleSprite = Resources.Load<Sprite>(resourceName);

            if (typeParticleSprite == null)
            {
                Debug.LogWarning("Unable to find sprite for type " + type);
                continue;
            }

            sprites.Add(typeParticleSprite);

        }

        return sprites.ToArray();

    }

    public static Sprite GetTypeParticleSprite(Pokemon.Type type)
    {
        return GetSprite(SpriteType.TypeParticle,
            typeParticlePrefix + TypeFunc.GetTypeResourceName(type));
    }

    #endregion

    #region Battle Sprites

    public const string battleSpritesSheetName = "sprite_sheet_battlesprites";
    public const string statusConditionSpritePrefix = "statuscondition_";
    public const string genderSpritePrefix = "gender_";
    public const string pokeBallLineBallSpritePrefix = "battle_pokeball_";

    private static Sprite[] LoadBattleSprites()
    {
        return LoadSpriteSheet(battleSpritesSheetName);
    }

    public static Sprite GetGenderSprite(bool? gender)
    {

        string genderName;

        switch (gender)
        {

            case true:
                genderName = "male";
                break;

            case false:
                genderName = "female";
                break;

            case null:
                genderName = "genderless";
                break;

        }

        string resourceName = genderSpritePrefix + genderName;

        return GetSprite(SpriteType.BattleSprite,
            resourceName);

    }

    public static readonly Dictionary<PokemonInstance.NonVolatileStatusCondition, string> nonVolatileStatusConditionResourceNames = new Dictionary<PokemonInstance.NonVolatileStatusCondition, string>()
    {
        { PokemonInstance.NonVolatileStatusCondition.Burn, "burnt" },
        { PokemonInstance.NonVolatileStatusCondition.Frozen, "frozen" },
        { PokemonInstance.NonVolatileStatusCondition.Paralysed, "paralysed" },
        { PokemonInstance.NonVolatileStatusCondition.Poisoned, "poisoned" },
        { PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned, "poisoned" },
        { PokemonInstance.NonVolatileStatusCondition.Asleep, "asleep" }
    };

    public static Sprite GetNonVolatileStatusConditionSprite(PokemonInstance.NonVolatileStatusCondition nvsc)
    {

        if (nvsc == PokemonInstance.NonVolatileStatusCondition.None)
            return null;

        if (!nonVolatileStatusConditionResourceNames.ContainsKey(nvsc))
        {
            Debug.LogWarning("No resource name found for NVSC " + nvsc);
            return null;
        }

        string resourceName = statusConditionSpritePrefix + nonVolatileStatusConditionResourceNames[nvsc];

        return GetSprite(SpriteType.BattleSprite,
            resourceName);

    }

    public static Sprite GetBattlePokeBallLineBallSprite(Battle.BattleLayout.PokeBallLineController.BallState state)
    {

        string stateResourceName = state switch
        {
            Battle.BattleLayout.PokeBallLineController.BallState.Valid => "valid",
            Battle.BattleLayout.PokeBallLineController.BallState.Invalid => "invalid",
            _ => null
        };

        if (stateResourceName == null)
        {
            Debug.LogError("Invalid poke ball state - " + state);
            return null;
        }

        string fullResourceName = pokeBallLineBallSpritePrefix + stateResourceName;

        return GetSprite(SpriteType.BattleSprite, fullResourceName);

    }

    #endregion

    #region Pokemon

    private const string pokemonSpritePath = "Pokemon/";
    private static readonly string[] pokemonSpriteDirectories = new string[]
    {
        "Front 1 Female/",
        "Front 1 Male/",
        "Front 2 Female/",
        "Front 2 Male/",
        "Back Female/",
        "Back Male/",
        "Icon/"
    };

    private static readonly Dictionary<string, string> pokemonSpriteDirectoryResourceNames = new Dictionary<string, string>
    {
        { "Front 1 Female/", "front1_f" },
        { "Front 1 Male/", "front1_m" },
        { "Front 2 Female/", "front2_f" },
        { "Front 2 Male/", "front2_m" },
        { "Back Female/", "back_f" },
        { "Back Male/", "back_m" },
        { "Icon/", "icon" }
    };

    private static Dictionary<Sprite, string> LoadPokemonSprites()
    {

        //Since the pokemon sprites can't have the defualt names, this method will return a dictionary describing each sprite and the name it should be stored with

        Dictionary<Sprite, string> output = new Dictionary<Sprite, string>();
        
        foreach (string spriteDirectory in pokemonSpriteDirectories)
        {

            string spriteResourcesPath = spritesResourcesPrefix + pokemonSpritePath + spriteDirectory;
            Sprite[] directorySprites = Resources.LoadAll<Sprite>(spriteResourcesPath);

            string spriteDirectoryResourceName = pokemonSpriteDirectoryResourceNames[spriteDirectory];

            foreach (Sprite sprite in directorySprites)
            {

                string spriteName = sprite.name + '_' + spriteDirectoryResourceName;

                output.Add(sprite, spriteName);

            }

        }

        return output;

    }

    public static Sprite GetPokemonSprite(
        string speciesResourceName,
        PokemonSpecies.SpriteType spriteType,
        bool useFemale)
    {

        #region Resource Name

        string resourceName;
        string alternativeResourceName = null; //The resource path to load if the main can't be found. (this is to use male sprites if female ones can't be found)

        string nameSuffix;
        string alternativeNameSuffix = null;

        switch (spriteType)
        {

            case PokemonSpecies.SpriteType.Front1:
                nameSuffix = "front1";
                break;

            case PokemonSpecies.SpriteType.Front2:
                nameSuffix = "front2";
                break;

            case PokemonSpecies.SpriteType.Back:
                nameSuffix = "back";
                break;

            case PokemonSpecies.SpriteType.Icon:
                nameSuffix = "icon";
                break;

            default:
                nameSuffix = "icon";
                Debug.LogError("Unknown sprite type provided - " + spriteType);
                break;

        }

        if (spriteType != PokemonSpecies.SpriteType.Icon)
        {

            if (!useFemale)
            {
                nameSuffix += "_m";
            }
            else
            {
                alternativeNameSuffix = nameSuffix + "_m";
                nameSuffix += "_f";
            }

        }

        resourceName = speciesResourceName + '_' + nameSuffix;

        if (alternativeNameSuffix != null)
        {
            alternativeResourceName = speciesResourceName + '_' + alternativeNameSuffix;
        }

        #endregion

        Sprite sprite = GetSprite(SpriteType.Pokemon,
            resourceName,
            false);

        if (sprite != null)
        {
            return sprite;
        }
        else
        {
            if (alternativeResourceName != null)
            {
                return GetSprite(SpriteType.Pokemon,
                    alternativeResourceName,
                    false);
            }
            else
            {
                return null;
            }
        }

    }

    public static Sprite GetPokemonSprite(
        string resourceName,
        PokemonSpecies.SpriteType spriteType,
        bool? gender
        )
    {
        bool useFemale;

        switch (gender)
        {

            case true:
            case null:
                useFemale = false;
                break;

            case false:
                useFemale = true;
                break;

        }

        return GetPokemonSprite(resourceName, spriteType, useFemale);
    }

    #endregion

    #region Poke Balls

    private const string pokeBallSpriteSheetName = "sprite_sheet_pokeballs";

    private static Sprite[] LoadPokeBallSprites()
    {
        return LoadSpriteSheet(pokeBallSpriteSheetName);
    }

    public static Sprite GetPokeBallSprite(int pokeBallId,
        PokeBall.SpriteType spriteType,
        bool addTypeId = false)
    {

        string resourceName = PokeBall.GetPokeBallById(pokeBallId, addTypeId).resourceName + '_' + PokeBall.spriteTypeResourceNames[spriteType];

        return GetSprite(SpriteType.PokeBall, resourceName);

    }

    #endregion

    #region Battle Backgrounds and Circles

    private const string battleBackgroundSpriteSheetName = "sprite_sheet_battle_backgrounds";
    private const string battleCirclesSpriteSheetName = "sprite_sheet_battle_circles";

    private static Sprite[] LoadBattleBackgroundAndCircleSprites()
    {
        List<Sprite> sprites = new List<Sprite>();
        sprites.AddRange(LoadSpriteSheet(battleBackgroundSpriteSheetName));
        sprites.AddRange(LoadSpriteSheet(battleCirclesSpriteSheetName));
        return sprites.ToArray();
    }

    public static Sprite GetBattleBackgroundSprite(string name)
    {
        string resourceName = "battle_background_" + name;
        return GetSprite(SpriteType.BattleBackgroundAndCircle, resourceName);
    }

    public static Sprite GetBattleCircleSprite(string name)
    {
        string resourceName = "battle_circle_" + name;
        return GetSprite(SpriteType.BattleBackgroundAndCircle, resourceName);
    }

    #endregion

}
