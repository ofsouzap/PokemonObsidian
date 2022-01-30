using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Pokemon;
using Pokemon.Moves;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace CheatConsole
{
    public class BattleConsoleController : CheatConsoleController
    {

        public BattleManager battleManager;

        protected static readonly Dictionary<Regex, Func<BattleManager, Match, string>> cheatCodes = new Dictionary<Regex, Func<BattleManager, Match, string>>()
        {

            /* Cheat Code Template:
             
            {
               new Regex("^REGEX$"),
               (bm, m) =>
               {
                   bm.
                   return "";
               }
            },

             */

            {
               new Regex("^player fullheal"),
               (bm, m) =>
               {
                   bm.CheatCommand_PlayerFullHeal();
                   return "Player pokemon healed";
               }
            },

            {
               new Regex("^opponent fullheal"),
               (bm, m) =>
               {
                   bm.CheatCommand_OpponentFullHeal();
                   return "Opponent pokemon healed";
               }
            },

            {
               new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) statuscondition set (?<nvsc>none|burnt|frozen|paralysed|poisoned|badlypoisoned|asleep)$"),
               (bm, m) =>
               {

                   int partyIndex;
                   if (m.Groups["partyIndex"].Value == "active")
                       partyIndex = -1;
                   else if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out partyIndex, out string message))
                   {
                       return message;
                   }

                   if (!TryParseNonVolatileStatusCondition(m.Groups["nvsc"].Value, out PokemonInstance.NonVolatileStatusCondition nvsc, out string nvscInvMessage))
                   {
                       return nvscInvMessage;
                   }

                   if (m.Groups["participant"].Value == "player")
                   {
                       bm.CheatCommand_PlayerInflictNonVolatileStatusCondition(partyIndex, nvsc);
                       return "Player pokemon non-volatile status condition set";
                   }
                   else
                   {
                       bm.CheatCommand_OpponentInflictNonVolatileStatusCondition(partyIndex, nvsc);
                       return "Opponent pokemon non-volatile status condition set";
                   }

               }
            },

            {
               new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) statuscondition clear$"),
               (bm, m) =>
               {

                   int partyIndex;
                   if (m.Groups["partyIndex"].Value == "active")
                       partyIndex = -1;
                   else if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out partyIndex, out string message))
                   {
                       return message;
                   }

                   if (m.Groups["participant"].Value == "player")
                   {
                       bm.CheatCommand_PlayerInflictNonVolatileStatusCondition(partyIndex, PokemonInstance.NonVolatileStatusCondition.None);
                       return "Player pokemon non-volatile status condition cleared";
                   }
                   else
                   {
                       bm.CheatCommand_OpponentInflictNonVolatileStatusCondition(partyIndex, PokemonInstance.NonVolatileStatusCondition.None);
                       return "Opponent pokemon non-volatile status condition cleared";
                   }

               }
            },

            {
                new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) health set (?<value>(absolute (?<absoluteValue>[0-9]+))|proportional (?<proportionalValue>0|1|0[.][0-9]+))$"),
                (bm, m) =>
                {

                    if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out int partyIndex, out string partyIndexInvMessage))
                    {
                        return partyIndexInvMessage;
                    }

                    bool absoluteMode = m.Groups["value"].Value.Split(' ')[0] == "absolute";

                    float amount = float.Parse(m.Groups[absoluteMode ? "absoluteValue" : "proportionalValue"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerPokemonHealthSet(partyIndex, absoluteMode, amount);
                        return "Player's pokemon health set";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentPokemonHealthSet(partyIndex, absoluteMode, amount);
                        return "Opponent's pokemon health set";
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) health change (?<value>(absolute (?<absoluteValue>-?[0-9]+))|proportional (?<proportionalValue>0|-?1|-?0[.][0-9]+))$"),
                (bm, m) =>
                {

                    if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out int partyIndex, out string partyIndexInvMessage))
                    {
                        return partyIndexInvMessage;
                    }

                    bool absoluteMode = m.Groups["value"].Value.Split(' ')[0] == "absolute";

                    float amount = float.Parse(m.Groups[absoluteMode ? "absoluteValue" : "proportionalValue"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerPokemonHealthChange(partyIndex, absoluteMode, amount);
                        return "Player's pokemon health changed";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentPokemonHealthChange(partyIndex, absoluteMode, amount);
                        return "Opponent's pokemon health changed";
                    }

                }
            },

            {
               new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) statmodifier (?<stat>attack|defense|specialattack|specialdefense|speed|evasion|accuracy) set (?<amount>-?[0-6])$"),
               (bm, m) =>
               {

                   if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out int partyIndex, out string partyIndexInvMessage))
                   {
                       return partyIndexInvMessage;
                   }

                   sbyte amount = sbyte.Parse(m.Groups["amount"].Value);

                   if (!TryParseStat(m.Groups["stat"].Value, out Stats<sbyte>.Stat stat, out bool overrideEvasion, out bool overrideAccuracy, out string statInvMessage))
                   {
                       return statInvMessage;
                   }

                   if (m.Groups["participant"].Value == "player")
                   {
                       bm.CheatCommand_PlayerStatModifierSet(partyIndex, stat, overrideEvasion, overrideAccuracy, amount);
                       return "Player's pokemon's stat modifier set";
                   }
                   else
                   {
                       bm.CheatCommand_OpponentStatModifierSet(partyIndex, stat, overrideEvasion, overrideAccuracy, amount);
                       return "Opponent's pokemon's stat modifier set";
                   }

               }
            },

            {
               new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) statmodifier (?<stat>attack|defense|specialattack|specialdefense|speed|evasion|accuracy) change (?<amount>-?[0-6])$"),
               (bm, m) =>
               {

                   if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out int partyIndex, out string partyIndexInvMessage))
                   {
                       return partyIndexInvMessage;
                   }

                   sbyte amount = sbyte.Parse(m.Groups["amount"].Value);

                   if (!TryParseStat(m.Groups["stat"].Value, out Stats<sbyte>.Stat stat, out bool overrideEvasion, out bool overrideAccuracy, out string statInvMessage))
                   {
                       return statInvMessage;
                   }

                   if (m.Groups["participant"].Value == "player")
                   {
                       bm.CheatCommand_PlayerStatModifierChange(partyIndex, stat, overrideEvasion, overrideAccuracy, amount);
                       return "Player's pokemon's stat modifier changed";
                   }
                   else
                   {
                       bm.CheatCommand_OpponentStatModifierChange(partyIndex, stat, overrideEvasion, overrideAccuracy, amount);
                       return "Opponent's pokemon's stat modifier changed";
                   }

               }
            },

            {
               new Regex("^(?<participant>player|opponent) flinch"),
               (bm, m) =>
               {
                   if (m.Groups["participant"].Value == "player")
                   {
                       bm.CheatCommand_PlayerPokemonFlinch();
                       return "Player's active pokemon set to flinch";
                   }
                   else
                   {
                       bm.CheatCommand_OpponentPokemonFlinch();
                       return "Opponent's active pokemon set to flinch";
                   }
               }
            },

            {
               new Regex("^weather set (?<weatherId>[0-9]+)"),
               (bm, m) =>
               {

                   int weatherId = int.Parse(m.Groups["weatherId"].Value);

                   if (!Weather.registry.EntryWithIdExists(weatherId))
                   {
                       return "Weather id specified has no assigned weather";
                   }

                   if (bm.CheatCommand_SetWeatherPermanent(weatherId))
                       return "Weather changed";
                   else
                       return "Unable to change to the specified weather. Check that a weather exists with that id";

               }
            },

            {
                new Regex("^opponent action set fight move (?<moveIndex>0|1|2|3|struggle)$"),
                (bm, m) =>
                {

                    bool useStruggle = m.Groups["moveIndex"].Value == "struggle";
                    
                    int moveIndex;

                    if (useStruggle)
                        moveIndex = 0;
                    else
                        moveIndex = int.Parse(m.Groups["moveIndex"].Value);

                    if (bm.CheatCommand_SetOpponentAction_Fight(moveIndex, useStruggle))
                        return "Opponent action set to fight";
                    else
                        return "Unable to set this action. Check that opponent has that move set";

                }
            },

            {
                new Regex("^opponent action set switchpokemon (?<partyIndex>0|1|2|3|4|5)$"),
                (bm, m) =>
                {

                    int partyIndex = int.Parse(m.Groups["partyIndex"].Value);

                    if (bm.CheatCommand_SetOpponentAction_SwitchPokemon(partyIndex))
                        return "Opponent action set to switch pokemon";
                    else
                        return "Unable to set this action. Check that opponent has that many pokemon and that that isn't already their active pokemon";

                }
            },

            {
                new Regex("^(?<participant>player|opponent) action set useitem (?<itemType>medicineitem|battleitem) (?<itemId>[0-9]+) pokemon (?<partyIndex>0|1|2|3|4|5|active)(?<move> move (?<moveIndex>0|1|2|3))?$"),
                (bm, m) =>
                {

                    if (!TryParseItem(m.Groups["itemType"].Value,
                        m.Groups["itemId"].Value,
                        out Item itemToUse,
                        out string itemInvMessage))
                    {
                        return itemInvMessage;
                    }

                    if (!TryParsePartyIndex(m.Groups["partyIndex"].Value,
                        out int partyIndex,
                        out string partyIndexInvMessage))
                    {
                        return partyIndexInvMessage;
                    }

                    int moveIndex = -1;

                    if (m.Groups["move"].Value.Length > 0)
                    {
                        moveIndex = int.Parse(m.Groups["moveIndex"].Value);
                    }

                    if (m.Groups["participant"].Value == "player") {
                        if (bm.CheatCommand_SetPlayerAction_UseItem(itemToUse, partyIndex, moveIndex))
                            return "Set player action to use item";
                        else
                            return "Unable to set this action. Check that all used parameters are valid";
                    }
                    else
                    {
                        if (bm.CheatCommand_SetOpponentAction_UseItem(itemToUse, partyIndex, moveIndex))
                            return "Set opponent action to use item";
                        else
                            return "Unable to set this action. Check that all used parameters are valid";
                    }

                }

            },

            {

                new Regex("^player action set useitem pokeball (?<pokeBallId>[0-9]+)$"),
                (bm, m) =>
                {

                    if (!TryParseItem("pokeball", m.Groups["pokeBallId"].Value, out Item pokeBallToUse, out string invMessage))
                        return invMessage;

                    if (bm.CheatCommand_SetPlayerAction_UseItem_PokeBall(pokeBallToUse))
                        return "Set player action to use poke ball";
                    else
                        return "Unable to set layer action to use poke ball";

                }

            },

            {

                new Regex("^opponent pokemon (?<partyIndex>0|1|2|3|4|5|active) moves get$"),
                (bm, m) =>
                {

                    if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out int partyIndex, out string invMessage))
                        return invMessage;

                    PokemonMove[] moves = bm.CheatCommand_GetOpponentPokemonMoves(partyIndex, out byte[] movePPs);

                    string output = "";

                    for (int i = 0; i < moves.Length; i++)
                    {

                        output = output + "[" + i.ToString() + "] " + moves[i].name + " (" + movePPs[i] + "/" + moves[i].maxPP + "), ";

                    }

                    output = output.Substring(0, output.Length - 2); //Remove final ", "

                    return output;

                }

            },

            {

                new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) move (?<moveIndex>0|1|2|3) pp set (?<pp>[0-9]+)$"),
                (bm, m) =>
                {

                    if (!TryParsePartyIndex(m.Groups["partyIndex"].Value, out int partyIndex, out string partyIndexInvMessage))
                        return partyIndexInvMessage;

                    int moveIndex = int.Parse(m.Groups["moveIndex"].Value);

                    if (!byte.TryParse(m.Groups["pp"].Value, out byte newPP))
                        return "Invalid new PP value provided";

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_SetPlayerMovePP(partyIndex, moveIndex, newPP);
                        return "Set player's pokemon's move PP";
                    }
                    else
                    {
                        bm.CheatCommand_SetOpponentMovePP(partyIndex, moveIndex, newPP);
                        return "Set opponent's pokemon's move PP";
                    }

                }

            }

        };

        protected override bool CheckAllowedToOpen()
            => battleManager.battleData.cheatsAllowed;

        protected override void ProcessCommand(string command)
        {

            bool matchFound = false;

            foreach (Regex regex in cheatCodes.Keys)
            {

                Match match = regex.Match(command);
                if (match.Success)
                {
                    Output(cheatCodes[regex](battleManager, match));
                    matchFound = true;
                }

            }

            if (!matchFound)
                Output("Unknown command or invalid syntax");

        }

    }
}
