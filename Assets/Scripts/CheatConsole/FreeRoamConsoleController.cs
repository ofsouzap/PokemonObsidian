using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;
using FreeRoaming.AreaControllers;
using Battle;
using Pokemon;
using Audio;

namespace CheatConsole
{
    public class FreeRoamConsoleController : CheatConsoleController
    {

        private FreeRoamSceneController sceneController;

        protected static readonly Dictionary<Regex, Func<Match, string>> cheatCodes = new Dictionary<Regex, Func<Match, string>>()
        {

            /* Cheat Code Template:
             
            {
               new Regex("^REGEX$"),
               (m) =>
               {
                   return "";
               }
            },

             */

            {
               new Regex("^wildencounter multiplier set (?<value>[0-9]+(\\.[0-9]+)?)$"),
               (m) =>
               {

                   float value = float.Parse(m.Groups["value"].Value);

                   PlayerController.singleton.SetWildEncounterCheatMultiplier(value);

                   return "Set wild encounter cheat multiplier to " + value.ToString();

               }
            },

            {
               new Regex("^trainerencounter (?<value>enable|disable)$"),
               (m) =>
               {

                   bool state = m.Groups["value"].Value == "enable";

                   PlayerController.singleton.SetTrainerEncounterCheatEnabled(state);

                   return (state ? "Enabled" : "Disabled") + " trainer encounters";

               }
            },

            {
               new Regex("^fullheal"),
               (m) =>
               {
                   PlayerData.singleton.HealPartyPokemon();
                   return "Fully healed party pokemon";
               }
            },

            {
               new Regex("^load scenestack (?<pattern>([A-z0-9 ]+,-?[0-9]+,-?[0-9]+(,-?[0-9]+)?;)*([A-z0-9 ]+,-?[0-9]+,-?[0-9]+(,-?[0-9]+)?))$"),
               (m) =>
               {

                   if (!GameSceneManager.SceneStack.TryParse(m.Groups["pattern"].Value, out GameSceneManager.SceneStack sceneStack, out string parseErrMsg))
                   {
                       return "Unable to parse scene stack string provided:\n" + parseErrMsg;
                   }
                   else
                   {
                       GameSceneManager.LoadSceneStack(sceneStack);
                       return "Loading scene stack";
                   }

               }
            },

            {
               new Regex("^wildpokemonarea current get$"),
               (m) =>
               {

                   var area = PlayerController.singleton.GetCurrentWildPokemonArea();

                   if (area == null)
                       return "No current wild pokemon area";
                   else
                       return "Current wild pokemon area name - " + area.name;

               }
            },

            {
               new Regex("^network timeout (?<value>enable|disable)"),
               (m) =>
               {

                   bool state = m.Groups["value"].Value == "enable";

                   GameSettings.singleton.networkTimeoutDisabled = !state;

                   return (state ? "Enabled" : "Disabled") + " network timeout";

               }
            },

            {
               new Regex("^network log (?<value>enable|disable)"),
               (m) =>
               {

                   bool state = m.Groups["value"].Value == "enable";

                   GameSettings.singleton.networkLogEnabled = state;

                   return (state ? "Enabled" : "Disabled") + " network log";

               }
            },

            {
                new Regex("^money add (?<amount>[0-9]+)"),
                (m) =>
                {

                    int amount = int.Parse(m.Groups["amount"].Value);

                    PlayerData.singleton.profile.money += amount;

                    return "Added " + PlayerData.currencySymbol + amount + " to player";

                }
            },

            {
                new Regex("^wildencounter create species (?<speciesId>[0-9]+) level (?<level>[0-2]?[0-9]?[0-9])"),
                (m) =>
                {

                    int speciesId = int.Parse(m.Groups["speciesId"].Value);

                    byte level = byte.Parse(m.Groups["level"].Value);

                    PokemonInstance opp = PokemonFactory.GenerateWild(
                        speciesId,
                        level,
                        level);

                    BattleEntranceArguments.argumentsSet = true;

                    BattleEntranceArguments.battleBackgroundResourceName = BattleEntranceArguments.defaultBackgroundName;

                    BattleEntranceArguments.battleType = BattleType.WildPokemon;
                    BattleEntranceArguments.wildPokemonBattleArguments = new BattleEntranceArguments.WildPokemonBattleArguments()
                    {
                        opponentInstance = opp
                    };

                    MusicSourceController.singleton.SetTrack(BattleEntranceArguments.defaultPokemonBattleMusicName, true);

                    GameSceneManager.LaunchBattleScene();

                    return "Launching battle with specified wild opponent";

                }
            },

            {
                new Regex("^item get (?<id>[0-9]+) (?<quantity>[0-9]+)"),
                (m) =>
                {

                    int itemId = int.Parse(m.Groups["id"].Value);
                    Items.Item item = Items.Item.GetItemById(itemId);

                    uint quantity = uint.Parse(m.Groups["quantity"].Value);

                    if (item == null)
                    {
                        return "Unknown item id provided";
                    }
                    else
                    {

                        PlayerData.singleton.inventory.AddItem(item, quantity);
                        return "Given player " + quantity.ToString() + ' ' + item.itemName;

                    }

                }
            },

            {
                new Regex("^gymbadge get (?<id>[0-9]+)"),
                (m) =>
                {

                    int gymId = int.Parse(m.Groups["id"].Value);

                    PlayerData.singleton.SetGymDefeated(gymId);

                    return "Added gym badge for gym id " + gymId.ToString();

                }
            },

            {
                new Regex("^pokemon get species (?<speciesId>[0-9]+) level (?<level>[0-2]?[0-9]?[0-9])"),
                (m) =>
                {

                    int speciesId = int.Parse(m.Groups["speciesId"].Value);

                    byte level = byte.Parse(m.Groups["level"].Value);

                    PokemonInstance pokemon = PokemonFactory.GenerateWild(
                        speciesId,
                        level,
                        level);

                    if (!PlayerData.singleton.PartyIsFull)
                    {
                        PlayerData.singleton.AddNewPartyPokemon(pokemon);
                        return $"{pokemon.GetDisplayName()} added to party";
                    }
                    else if (!PlayerData.singleton.boxPokemon.IsFull)
                    {
                        PlayerData.singleton.AddBoxPokemon(pokemon);
                        return $"{pokemon.GetDisplayName()} added to box";
                    }
                    else
                    {
                        return "No space to add pokemon";
                    }

                }
            }

        };

        protected override void Start()
        {

            base.Start();

            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);

        }

        protected override bool CheckAllowedToOpen()
        {

            if (!sceneController.SceneIsActive)
                return false;
            else
                return base.CheckAllowedToOpen();

        }

        protected override void ProcessCommand(string command)
        {

            bool matchFound = false;

            foreach (Regex regex in cheatCodes.Keys)
            {

                Match match = regex.Match(command);
                if (match.Success)
                {
                    Output(cheatCodes[regex](match));
                    matchFound = true;
                }

            }

            if (!matchFound)
                Output("Unknown command or invalid syntax");

            RefreshVisibility();

        }

        protected override void RefreshVisibility()
        {

            base.RefreshVisibility();

            FreeRoamSceneController sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);

            if (consoleActive)
                sceneController.SetSceneRunningState(false);
            else
                sceneController.SetSceneRunningState(true);

        }

    }
}
