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
               new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) statmodifier (?<stat>attack|defense|specialAttack|specialDefense|speed|evasion|accuracy) set (?<amount>-?[0-6])$"),
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
               new Regex("^(?<participant>player|opponent) pokemon (?<partyIndex>0|1|2|3|4|5|active) statmodifier (?<stat>attack|defense|specialAttack|specialDefense|speed|evasion|accuracy) change (?<amount>-?[0-6])$"),
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

            },

            {
                new Regex("^(?<participant>player|opponent) inflict bound (?<turns>[0-9]+)"),
                (bm, m) =>
                {

                    int turns = int.Parse(m.Groups["turns"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictBound(turns + 1);
                        return "Player pokemon inflicted with bound for " + turns + " turns";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictBound(turns + 1);
                        return "Opponent pokemon inflicted with bound for " + turns + " turns";
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set curse (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetCurse(state);
                        return "Set player pokemon curse state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetCurse(state);
                        return "Set opponent pokemon curse state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) inflict drowsy"),
                (bm, m) =>
                {
                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictDrowsy();
                        return "Made player's pokemon drowsy";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictDrowsy();
                        return "Made opponent's pokemon drowsy";
                    }
                }
            },

            {
                new Regex("^(?<participant>player|opponent) inflict embargo"),
                (bm, m) =>
                {

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictEmbargo();
                        return "Given player's pokemon embargo";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictEmbargo();
                        return "Given opponent's pokemon embargo";
                    }

                }
            },

            {
                new Regex("(?<participant>player|opponent) inflict encore (?<turns>[0-9]+)"),
                (bm, m) =>
                {

                    int turns = int.Parse(m.Groups["turns"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        if (bm.CheatCommand_TryPlayerInflictEncore(turns))
                            return "Inflicted player pokemon with encore for " + turns + " turns";
                        else
                            return "Couldn't inflict encore on player pokemon";
                    }
                    else
                    {
                        if (bm.CheatCommand_TryOpponentInflictEncore(turns))
                            return "Inflicted opponent pokemon with encore for " + turns + " turns";
                        else
                            return "Couldn't inflict encore on opponent pokemon";
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) inflict healblock"),
                (bm, m) =>
                {

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictHealBlock();
                        return "Inflicted player pokemon with heal block";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictHealBlock();
                        return "Inflicted opponent pokemon with heal block";
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set identified (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetIdentified(state);
                        return "Set player pokemon identified to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetIdentified(state);
                        return "Set opponent pokemon identified to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set infatuated (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetInfatuated(state);
                        return "Set player pokemon infatuated to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetInfatuated(state);
                        return "Set opponent pokemon infatuated to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set leechseed (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetLeechSeed(state);
                        return "Set player pokemon leech seed to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetLeechSeed(state);
                        return "Set opponent pokemon leech seed to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set nightmare (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetNightmare(state);
                        return "Set player pokemon nightmare state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetNightmare(state);
                        return "Set opponent pokemon nightmare state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) inflict perishsong (?<turns>[0-9]+)"),
                (bm, m) =>
                {

                    int turns = int.Parse(m.Groups["turns"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictPerishSong(turns);
                        return "Player pokemon inflicted with perish song for " + turns + " turns";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictPerishSong(turns);
                        return "Opponent pokemon inflicted with perish song for " + turns + " turns";
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) inflict taunt (?<turns>[0-9]+)"),
                (bm, m) =>
                {

                    int turns = int.Parse(m.Groups["turns"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictTaunt(turns);
                        return "Player pokemon inflicted with taunt for " + turns + " turns";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictTaunt(turns);
                        return "Opponent pokemon inflicted with taunt for " + turns + " turns";
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set torment (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetTorment(state);
                        return "Set player pokemon torment state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetTorment(state);
                        return "Set opponent pokemon torment state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set aquaring (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetAquaRing(state);
                        return "Set player pokemon aqua ring state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetAquaRing(state);
                        return "Set opponent pokemon aqua ring state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set bracing (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetBracing(state);
                        return "Set player pokemon bracing state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetBracing(state);
                        return "Set opponent pokemon bracing state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set defensecurl (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetDefenseCurl(state);
                        return "Set player pokemon defense curl state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetDefenseCurl(state);
                        return "Set opponent pokemon defense curl state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set rooting (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetRooting(state);
                        return "Set player pokemon rooting state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetRooting(state);
                        return "Set opponent pokemon rooting state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set protection (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetProtection(state);
                        return "Set player pokemon protection state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetProtection(state);
                        return "Set opponent pokemon protection state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) set takingaim (?<state>true|false)"),
                (bm, m) =>
                {

                    bool state = bool.Parse(m.Groups["state"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerSetTakingAim(state);
                        return "Set player pokemon taking aim state to " + (state ? "true" : "false");
                    }
                    else
                    {
                        bm.CheatCommand_OpponentSetTakingAim(state);
                        return "Set opponent pokemon taking aim state to " + (state ? "true" : "false");
                    }

                }
            },

            {
                new Regex("^(?<participant>player|opponent) inflict thrashing (?<turns>[0-9]+) with (?<moveIndex>[0-3])"),
                (bm, m) =>
                {

                    int turns = int.Parse(m.Groups["turns"].Value);
                    int moveIndex = int.Parse(m.Groups["moveIndex"].Value);

                    if (m.Groups["participant"].Value == "player")
                    {
                        bm.CheatCommand_PlayerInflictThrashing(turns, moveIndex);
                        return "Set player pokemon thrashing for " + turns.ToString() + " turns";
                    }
                    else
                    {
                        bm.CheatCommand_OpponentInflictThrashing(turns, moveIndex);
                        return "Set opponent pokemon thrashing for " + turns.ToString() + " turns";
                    }

                }
            },

            {
                new Regex("^(obedience|ob)([cC]ap)? (?<value>enable|disable)"),
                (bm, m) =>
                {
                    bool state = m.Groups["value"].Value == "enable";

                    GameSettings.singleton.obedienceEnabled = state;

                    return (state ? "Enabled" : "Disabled") + " obedience cap";

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
