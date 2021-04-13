using System;
using System.Collections.Generic;
using System.Linq;

public static class TrainerClass
{

    public enum Class
    {
        Barry,
        Youngster,
        Lass,
        Picnicer_m,
        Picnicer_f,
        Bugcatcher,
        AromaLady,
        Hiker,
        BattleGirl,
        Fisherman,
        Blackbelt,
        Cowgirl,
        PokeFan_m,
        PokeFan_f,
        AceTrainer_m,
        AceTrainer_f,
        Veteran,
        Ninjaboy,
        Birdkeeper,
        Lady,
        Gentleman,
        Socialite,
        Beauty,
        Collector,
        Officer,
        Scientist,
        Sailor,
        RuinManiac,
        Psychic_m,
        Psychic_f,
        Guitarist,
        Clown,
        Worker,
        Schoolboy,
        Schoolgirl,
        Oli,
        Roxi,
        Idol,
        Maid
    }

    /// <summary>
    /// The base payouts for each class
    /// </summary>
    public static readonly Dictionary<Class, byte> classBasePayouts = new Dictionary<Class, byte>()
    {
        { Class.Barry, 64 },
        { Class.Youngster, 16 },
        { Class.Lass, 16 },
        { Class.Picnicer_m, 16 },
        { Class.Picnicer_f, 16 },
        { Class.Bugcatcher, 16 },
        { Class.AromaLady, 32 },
        { Class.Hiker, 32 },
        { Class.BattleGirl, 16 },
        { Class.Fisherman, 32 },
        { Class.Blackbelt, 24 },
        { Class.Cowgirl, 16 },
        { Class.PokeFan_m, 64 },
        { Class.PokeFan_f, 64 },
        { Class.AceTrainer_m, 60 },
        { Class.AceTrainer_f, 60 },
        { Class.Veteran, 80 },
        { Class.Ninjaboy, 8 },
        { Class.Birdkeeper, 32 },
        { Class.Lady, 160 },
        { Class.Gentleman, 200 },
        { Class.Socialite, 200 },
        { Class.Beauty, 56 },
        { Class.Collector, 64 },
        { Class.Officer, 40 },
        { Class.Scientist, 32 },
        { Class.Sailor, 32 },
        { Class.RuinManiac, 48 },
        { Class.Psychic_m, 32 },
        { Class.Psychic_f, 32 },
        { Class.Guitarist, 24 },
        { Class.Clown, 24 },
        { Class.Worker, 40 },
        { Class.Schoolboy, 20 },
        { Class.Schoolgirl, 20 },
        { Class.Oli, 32 }, //Made this values up
        { Class.Roxi, 32 }, //Made this values up
        { Class.Idol, 72 },
        { Class.Maid, 40 }
    };

    /// <summary>
    /// The names of the battle sprites for the classes
    /// </summary>
    public static readonly Dictionary<Class, string> classBattleSpriteNames = new Dictionary<Class, string>()
    {
        { Class.Barry, "barry" },
        { Class.Youngster, "youngster" },
        { Class.Lass, "lass" },
        { Class.Picnicer_m, "picnicerm" },
        { Class.Picnicer_f, "picnicerf" },
        { Class.Bugcatcher, "bugcatcher" },
        { Class.AromaLady, "aromalady" },
        { Class.Hiker, "hiker" },
        { Class.BattleGirl, "battlegirl" },
        { Class.Fisherman, "fisherman" },
        { Class.Blackbelt, "blackbelt" },
        { Class.Cowgirl, "cowgirl" },
        { Class.PokeFan_m, "pokefanm" },
        { Class.PokeFan_f, "pokefanf" },
        { Class.AceTrainer_m, "acetrainerm" },
        { Class.AceTrainer_f, "acetrainerf" },
        { Class.Veteran, "veteran" },
        { Class.Ninjaboy, "ninjaboy" },
        { Class.Birdkeeper, "birdkeeper" },
        { Class.Lady, "lady" },
        { Class.Gentleman, "gentleman" },
        { Class.Socialite, "socialite" },
        { Class.Beauty, "beauty" },
        { Class.Collector, "collector" },
        { Class.Officer, "officer" },
        { Class.Scientist, "scientist" },
        { Class.Sailor, "sailor" },
        { Class.RuinManiac, "ruinmaniac" },
        { Class.Psychic_m, "psychicm" },
        { Class.Psychic_f, "psychicf" },
        { Class.Guitarist, "guitarist" },
        { Class.Clown, "clown" },
        { Class.Worker, "worker" },
        { Class.Schoolboy, "schoolboy" },
        { Class.Schoolgirl, "schoolgirl" },
        { Class.Oli, "oli" },
        { Class.Roxi, "roxi" },
        { Class.Idol, "idol" },
        { Class.Maid, "maid" }
    };

    /// <summary>
    /// Names for the classes to use in battle (eg. Ace Trainer, Lass, Bug Catcher)
    /// </summary>
    public static readonly Dictionary<Class, string> classNamesPrefixes = new Dictionary<Class, string>()
    {

        { Class.Barry, "" },
        { Class.Youngster, "Youngster" },
        { Class.Lass, "Lass" },
        { Class.Picnicer_m, "Picnicer" },
        { Class.Picnicer_f, "Picnicer" },
        { Class.Bugcatcher, "Bug Catcher" },
        { Class.AromaLady, "Aroma Lady" },
        { Class.Hiker, "Hiker" },
        { Class.BattleGirl, "Battle Girl" },
        { Class.Fisherman, "Fisherman" },
        { Class.Blackbelt, "Blackbelt" },
        { Class.Cowgirl, "Cowgirl" },
        { Class.PokeFan_m, "Poke Fan" },
        { Class.PokeFan_f, "Poke Fan" },
        { Class.AceTrainer_m, "Ace Trainer" },
        { Class.AceTrainer_f, "Ace Trainer" },
        { Class.Veteran, "Veteran" },
        { Class.Ninjaboy, "Ninja Boy" },
        { Class.Birdkeeper, "Bird Keeper" },
        { Class.Lady, "Lady" },
        { Class.Gentleman, "Gentleman" },
        { Class.Socialite, "Socialite" },
        { Class.Beauty, "Beauty" },
        { Class.Collector, "Collector" },
        { Class.Officer, "Officer" },
        { Class.Scientist, "Scientist" },
        { Class.Sailor, "Sailor" },
        { Class.RuinManiac, "Ruin Maniac" },
        { Class.Psychic_m, "Psychic" },
        { Class.Psychic_f, "Psychic" },
        { Class.Guitarist, "Guitarist" },
        { Class.Clown, "Clown" },
        { Class.Worker, "Worker" },
        { Class.Schoolboy, "Schoolboy" },
        { Class.Schoolgirl, "Schoolgirl" },
        { Class.Oli, "" },
        { Class.Roxi, "" },
        { Class.Idol, "Idol" },
        { Class.Maid, "Maid" }

    };

}
