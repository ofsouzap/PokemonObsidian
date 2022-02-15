using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Pokemon.Moves;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Battle
{
    public partial class BattleManager : GeneralSceneManager
    {

        private void FullUpdatePlayerOverviewPane() => battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(battleData.participantPlayer.ActivePokemon);
        private void FullUpdateOpponentOverviewPane() => battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(battleData.participantOpponent.ActivePokemon);

        /// <summary>
        /// Cheat command for fully healing all the player's pokemon including removing any non-volatile status conditions
        /// </summary>
        public void CheatCommand_PlayerFullHeal()
        {
            
            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon())
                pokemon?.RestoreFully();

            FullUpdatePlayerOverviewPane();

        }

        /// <summary>
        /// Cheat command for fully healing all the opponent's pokemon including removing any non-volatile status conditions
        /// </summary>
        public void CheatCommand_OpponentFullHeal()
        {

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon())
                pokemon?.RestoreFully();

            FullUpdateOpponentOverviewPane();

        }

        /// <summary>
        /// Cheat command to inflict NVSC on a player pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_PlayerInflictNonVolatileStatusCondition(int partyIndex, PokemonInstance.NonVolatileStatusCondition nvsc)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
            if (pokemon == null) return;
            pokemon.nonVolatileStatusCondition = nvsc;
            FullUpdatePlayerOverviewPane();

        }

        /// <summary>
        /// Cheat command to inflict NVSC on an opponent pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_OpponentInflictNonVolatileStatusCondition(int partyIndex, PokemonInstance.NonVolatileStatusCondition nvsc)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            if (pokemon == null) return;
            pokemon.nonVolatileStatusCondition = nvsc;
            FullUpdateOpponentOverviewPane();

        }

        /// <summary>
        /// Cheat command to set the health on a player pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_PlayerPokemonHealthSet(int partyIndex,
            bool absoluteMode,
            float amount)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
            if (pokemon == null) return;
            pokemon.health = absoluteMode ? Mathf.FloorToInt(amount) : Mathf.FloorToInt(pokemon.GetStats().health * amount);
            FullUpdatePlayerOverviewPane();
        }

        /// <summary>
        /// Cheat command to set the health on an opponent pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_OpponentPokemonHealthSet(int partyIndex,
            bool absoluteMode,
            float amount)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            if (pokemon == null) return;
            pokemon.health = absoluteMode ? Mathf.FloorToInt(amount) : Mathf.FloorToInt(pokemon.GetStats().health * amount);
            FullUpdateOpponentOverviewPane();
        }

        /// <summary>
        /// Cheat command to change the health on a player pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_PlayerPokemonHealthChange(int partyIndex,
            bool absoluteMode,
            float amount)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
            if (pokemon == null) return;
            pokemon.health += absoluteMode ? Mathf.FloorToInt(amount) : Mathf.FloorToInt(pokemon.GetStats().health * amount);
            FullUpdatePlayerOverviewPane();
        }

        /// <summary>
        /// Cheat command to change the health on an opponent pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_OpponentPokemonHealthChange(int partyIndex,
            bool absoluteMode,
            float amount)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            if (pokemon == null) return;
            pokemon.health += absoluteMode ? Mathf.FloorToInt(amount) : Mathf.FloorToInt(pokemon.GetStats().health * amount);
            FullUpdateOpponentOverviewPane();
        }

        /// <summary>
        /// Cheat command to set a stat modifier on an player pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_PlayerStatModifierSet(int partyIndex,
            Stats<sbyte>.Stat stat,
            bool overrideEvasion,
            bool overrideAccuracy,
            sbyte amount)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
            if (pokemon == null) return;

            if (overrideEvasion)
                pokemon.battleProperties.evasionModifier = amount;
            else if (overrideAccuracy)
                pokemon.battleProperties.accuracyModifier = amount;
            else
                switch (stat)
                {
                    case Stats<sbyte>.Stat.attack:
                        pokemon.battleProperties.statModifiers.attack = amount;
                        break;
                    case Stats<sbyte>.Stat.defense:
                        pokemon.battleProperties.statModifiers.defense = amount;
                        break;
                    case Stats<sbyte>.Stat.specialAttack:
                        pokemon.battleProperties.statModifiers.specialAttack = amount;
                        break;
                    case Stats<sbyte>.Stat.specialDefense:
                        pokemon.battleProperties.statModifiers.specialDefense = amount;
                        break;
                    case Stats<sbyte>.Stat.speed:
                        pokemon.battleProperties.statModifiers.speed = amount;
                        break;
                }

            FullUpdatePlayerOverviewPane();

        }

        /// <summary>
        /// Cheat command to set a stat modifier on an opponent pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_OpponentStatModifierSet(int partyIndex,
            Stats<sbyte>.Stat stat,
            bool overrideEvasion,
            bool overrideAccuracy,
            sbyte amount)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            if (pokemon == null) return;

            if (overrideEvasion)
                pokemon.battleProperties.evasionModifier = amount;
            else if (overrideAccuracy)
                pokemon.battleProperties.accuracyModifier = amount;
            else
                switch (stat)
                {
                    case Stats<sbyte>.Stat.attack:
                        pokemon.battleProperties.statModifiers.attack = amount;
                        break;
                    case Stats<sbyte>.Stat.defense:
                        pokemon.battleProperties.statModifiers.defense = amount;
                        break;
                    case Stats<sbyte>.Stat.specialAttack:
                        pokemon.battleProperties.statModifiers.specialAttack = amount;
                        break;
                    case Stats<sbyte>.Stat.specialDefense:
                        pokemon.battleProperties.statModifiers.specialDefense = amount;
                        break;
                    case Stats<sbyte>.Stat.speed:
                        pokemon.battleProperties.statModifiers.speed = amount;
                        break;
                }

            FullUpdateOpponentOverviewPane();

        }

        /// <summary>
        /// Cheat command to change a stat modifier on an player pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_PlayerStatModifierChange(int partyIndex,
            Stats<sbyte>.Stat stat,
            bool overrideEvasion,
            bool overrideAccuracy,
            sbyte amount)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
            if (pokemon == null) return;

            if (overrideEvasion)
                pokemon.battleProperties.evasionModifier += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.evasionModifier);
            else if (overrideAccuracy)
                pokemon.battleProperties.accuracyModifier += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.accuracyModifier);
            else
                switch (stat)
                {
                    case Stats<sbyte>.Stat.attack:
                        pokemon.battleProperties.statModifiers.attack += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.attack);
                        break;
                    case Stats<sbyte>.Stat.defense:
                        pokemon.battleProperties.statModifiers.defense += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.defense);
                        break;
                    case Stats<sbyte>.Stat.specialAttack:
                        pokemon.battleProperties.statModifiers.specialAttack += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.specialAttack);
                        break;
                    case Stats<sbyte>.Stat.specialDefense:
                        pokemon.battleProperties.statModifiers.specialDefense += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.specialDefense);
                        break;
                    case Stats<sbyte>.Stat.speed:
                        pokemon.battleProperties.statModifiers.speed += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.speed);
                        break;
                }

            FullUpdatePlayerOverviewPane();

        }

        /// <summary>
        /// Cheat command to set a change modifier on an opponent pokemon. Negative party index means active pokemon
        /// </summary>
        public void CheatCommand_OpponentStatModifierChange(int partyIndex,
            Stats<sbyte>.Stat stat,
            bool overrideEvasion,
            bool overrideAccuracy,
            sbyte amount)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            if (pokemon == null) return;

            if (overrideEvasion)
                pokemon.battleProperties.evasionModifier += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.evasionModifier);
            else if (overrideAccuracy)
                pokemon.battleProperties.accuracyModifier += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.accuracyModifier);
            else
                switch (stat)
                {
                    case Stats<sbyte>.Stat.attack:
                        pokemon.battleProperties.statModifiers.attack += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.attack);
                        break;
                    case Stats<sbyte>.Stat.defense:
                        pokemon.battleProperties.statModifiers.defense += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.defense);
                        break;
                    case Stats<sbyte>.Stat.specialAttack:
                        pokemon.battleProperties.statModifiers.specialAttack += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.specialAttack);
                        break;
                    case Stats<sbyte>.Stat.specialDefense:
                        pokemon.battleProperties.statModifiers.specialDefense += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.specialDefense);
                        break;
                    case Stats<sbyte>.Stat.speed:
                        pokemon.battleProperties.statModifiers.speed += Stats<sbyte>.LimitStatModifierChange(amount, pokemon.battleProperties.statModifiers.speed);
                        break;
                }

            FullUpdateOpponentOverviewPane();

        }

        /// <summary>
        /// Cheat command to make the player's active pokemon flinch
        /// </summary>
        public void CheatCommand_PlayerPokemonFlinch()
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.flinch = true;
        }

        /// <summary>
        /// Cheat command to make the opponent's active pokemon flinch
        /// </summary>
        public void CheatCommand_OpponentPokemonFlinch()
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.flinch = true;
        }

        /// <summary>
        /// Cheat command to set the weather. This overwrites the initial weather of the battle
        /// </summary>
        /// <returns>Whether the weather change was successful</returns>
        public bool CheatCommand_SetWeatherPermanent(int weatherId)
        {

            if (!Weather.registry.EntryWithIdExists(weatherId)) return false;

            battleData.weatherHasBeenChanged = false;
            battleData.currentWeatherId = weatherId;
            battleData.initialWeatherId = weatherId;

            return true;

        }

        /// <summary>
        /// Cheat command to set the opponent's action to use a specified move
        /// </summary>
        /// <param name="moveIndex">The index of the move for the opponent to use</param>
        /// <param name="useStruggle">Whether the opponent should use struggle instead of an indexed move (moveIndex is ignored if useStruggle is true)</param>
        /// <returns>Whether the action-setting was successful</returns>
        public bool CheatCommand_SetOpponentAction_Fight(int moveIndex,
            bool useStruggle = false)
        {

            if (!useStruggle && PokemonMove.MoveIdIsUnset(battleData.participantOpponent.ActivePokemon.moveIds[moveIndex]))
                return false;

            battleData.participantOpponent.ForceSetChosenAction(new BattleParticipant.Action(battleData.participantOpponent)
            {
                type = BattleParticipant.Action.Type.Fight,
                fightMoveTarget = battleData.participantPlayer,
                fightMoveIndex = useStruggle ? 0 : moveIndex,
                fightUsingStruggle = useStruggle
            });

            return true;

        }

        /// <summary>
        /// Cheat command to set the opponent's action to switch pokemon to a pokemon specified by party index
        /// </summary>
        /// <returns>Whether the action-setting was successful</returns>
        public bool CheatCommand_SetOpponentAction_SwitchPokemon(int partyIndex)
        {

            if (battleData.participantOpponent.activePokemonIndex == partyIndex)
                return false;

            if (battleData.participantOpponent.GetPokemon().Length <= partyIndex)
                return false;

            if (battleData.participantOpponent.GetPokemon()[partyIndex] == null)
                return false;

            battleData.participantOpponent.ForceSetChosenAction(new BattleParticipant.Action(battleData.participantOpponent)
            {
                type = BattleParticipant.Action.Type.SwitchPokemon,
                switchPokemonIndex = partyIndex
            });

            return true;

        }

        /// <summary>
        /// Cheat command to set the player's action to use a specified item on a specified party pokemon without taking the item from the player's inventory
        /// </summary>
        /// <param name="moveIndex">An optional parameter for the index of the move to use the item on (eg. for PP-restoring items)</param>
        public bool CheatCommand_SetPlayerAction_UseItem(Item item, int partyIndex, int moveIndex = -1)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;

            if (moveIndex >= 0 && PokemonMove.MoveIdIsUnset(pokemon.moveIds[moveIndex]))
                return false;

            if (moveIndex < 0 && item is PPRestoreMedicineItem)
                return false;

            battleData.participantPlayer.ForceSetChosenAction(new BattleParticipant.Action(battleData.participantPlayer)
            {
                type = BattleParticipant.Action.Type.UseItem,
                useItemItemToUse = item,
                useItemTargetPartyIndex = partyIndex >= 0 ? partyIndex : battleData.participantPlayer.activePokemonIndex,
                useItemTargetMoveIndex = moveIndex,
                useItemDontConsumeItem = true
            });

            playerBattleUIController.DisableAllMenus();

            return true;

        }

        /// <summary>
        /// Cheat command to set the opponent's action to use a specified item on a specified party pokemon
        /// </summary>
        /// <param name="moveIndex">An optional parameter for the index of the move to use the item on (eg. for PP-restoring items)</param>
        public bool CheatCommand_SetOpponentAction_UseItem(Item item, int partyIndex, int moveIndex = -1)
        {

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;

            if (moveIndex >= 0 && PokemonMove.MoveIdIsUnset(pokemon.moveIds[moveIndex]))
                return false;

            if (moveIndex < 0 && item is PPRestoreMedicineItem)
                return false;

            battleData.participantOpponent.ForceSetChosenAction(new BattleParticipant.Action(battleData.participantOpponent)
            {
                type = BattleParticipant.Action.Type.UseItem,
                useItemItemToUse = item,
                useItemTargetPartyIndex = partyIndex >= 0 ? partyIndex : battleData.participantOpponent.activePokemonIndex,
                useItemTargetMoveIndex = moveIndex
            });
            return true;

        }

        /// <summary>
        /// Cheat command to set the player's action to use a specified poke ball without taking the poke ball from the player's inventory
        /// </summary>
        public bool CheatCommand_SetPlayerAction_UseItem_PokeBall(Item pokeBall)
        {

            battleData.participantPlayer.ForceSetChosenAction(new BattleParticipant.Action(battleData.participantPlayer)
            {
                type = BattleParticipant.Action.Type.UseItem,
                useItemItemToUse = pokeBall,
                useItemPokeBallTarget = battleData.participantOpponent,
                useItemDontConsumeItem = true
            });

            playerBattleUIController.DisableAllMenus();

            return true;

        }

        /// <summary>
        /// Cheat command to get the moves of one of the opponent's pokemon's moves
        /// </summary>
        public PokemonMove[] CheatCommand_GetOpponentPokemonMoves(int partyIndex, out byte[] movePPs)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            movePPs = pokemon.movePPs;
            return pokemon
                .moveIds
                .Where(x => !PokemonMove.MoveIdIsUnset(x))
                .Select(x => PokemonMove.GetPokemonMoveById(x))
                .ToArray();
        }

        /// <summary>
        /// Cheat command to set the PP of one of the player's pokemon's moves
        /// </summary>
        public void CheatCommand_SetPlayerMovePP(int partyIndex, int moveIndex, byte newPP)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
            pokemon.movePPs[moveIndex] = newPP;
        }

        /// <summary>
        /// Cheat command to set the PP of one of the opponent's pokemon's moves
        /// </summary>
        public void CheatCommand_SetOpponentMovePP(int partyIndex, int moveIndex, byte newPP)
        {
            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantOpponent.GetPokemon()[partyIndex] : battleData.participantOpponent.ActivePokemon;
            pokemon.movePPs[moveIndex] = newPP;
        }

        #region Volatile Status Conditions

        /// <summary>
        /// Makes the player's active pokemon bound for the number of turns provided
        /// </summary>
        public void CheatCommand_PlayerInflictBound(int turns)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.bound = turns;
        }

        /// <summary>
        /// Makes the opponent's active pokemon bound for the number of turns provided
        /// </summary>
        public void CheatCommand_OpponentInflictBound(int turns)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.bound = turns;
        }

        /// <summary>
        /// Makes the player's active pokemon cursed or removes it
        /// </summary>
        public void CheatCommand_PlayerSetCurse(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.curse = state;
        }

        /// <summary>
        /// Makes the opponent's active pokemon cursed or removes it
        /// </summary>
        public void CheatCommand_OpponentSetCurse(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.curse = state;
        }

        /// <summary>
        /// Makes the player's active pokemon drowsy
        /// </summary>
        public void CheatCommand_PlayerInflictDrowsy()
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.drowsyStage = 2;
        }

        /// <summary>
        /// Makes the opponent's active pokemon drowsy
        /// </summary>
        public void CheatCommand_OpponentInflictDrowsy()
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.drowsyStage = 2;
        }

        /// <summary>
        /// Gives the player's active pokemon an embargo
        /// </summary>
        public void CheatCommand_PlayerInflictEmbargo()
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.embargo = 6;
        }

        /// <summary>
        /// Gives the opponent's active pokemon an embargo
        /// </summary>
        public void CheatCommand_OpponentInflictEmbargo()
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.embargo = 6;
        }

        /// <summary>
        /// Makes the player's active pokemon have encore for the number of turns provided
        /// </summary>
        public bool CheatCommand_TryPlayerInflictEncore(int turns)
        {

            PokemonInstance pokemon = battleData.participantPlayer.ActivePokemon;

            if (pokemon.battleProperties.lastMoveId < 0)
                return false;

            pokemon.battleProperties.volatileStatusConditions.encoreTurns = turns;
            pokemon.battleProperties.volatileStatusConditions.encoreMoveId = pokemon.battleProperties.lastMoveId;

            return true;

        }

        /// <summary>
        /// Makes the opponent's active pokemon have encore for the number of turns provided
        /// </summary>
        public bool CheatCommand_TryOpponentInflictEncore(int turns)
        {

            PokemonInstance pokemon = battleData.participantOpponent.ActivePokemon;

            if (pokemon.battleProperties.lastMoveId < 0)
                return false;

            pokemon.battleProperties.volatileStatusConditions.encoreTurns = turns;
            pokemon.battleProperties.volatileStatusConditions.encoreMoveId = pokemon.battleProperties.lastMoveId;

            return true;

        }

        /// <summary>
        /// Gives the player participant a heal block
        /// </summary>
        public void CheatCommand_PlayerInflictHealBlock()
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.healBlock = 5;
        }

        /// <summary>
        /// Gives the opponent participant a heal block
        /// </summary>
        public void CheatCommand_OpponentInflictHealBlock()
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.healBlock = 5;
        }

        /// <summary>
        /// Identifies the player's pokemon
        /// </summary>
        public void CheatCommand_PlayerSetIdentified(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.identified = state;
        }

        /// <summary>
        /// Identifies the opponent's pokemon
        /// </summary>
        public void CheatCommand_OpponentSetIdentified(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.identified = state;
        }

        /// <summary>
        /// Infatuates the player's pokemon
        /// </summary>
        public void CheatCommand_PlayerSetInfatuated(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.infatuated = state;
        }

        /// <summary>
        /// Infatuates the opponent's pokemon
        /// </summary>
        public void CheatCommand_OpponentSetInfatuated(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.infatuated = state;
        }

        /// <summary>
        /// Inflicts the player's pokemon with leech seed
        /// </summary>
        public void CheatCommand_PlayerSetLeechSeed(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.leechSeed = state;
        }

        /// <summary>
        /// Inflicts the opponent's pokemon with leech seed
        /// </summary>
        public void CheatCommand_OpponentSetLeechSeed(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.leechSeed = state;
        }

        /// <summary>
        /// Makes the player's active pokemon have nightmares or removes them
        /// </summary>
        public void CheatCommand_PlayerSetNightmare(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.nightmare = state;
        }

        /// <summary>
        /// Makes the opponent's active pokemon have nightmares or removes them
        /// </summary>
        public void CheatCommand_OpponentSetNightmare(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.nightmare = state;
        }

        /// <summary>
        /// Gives the player's active pokemon perish song for the number of turns provided
        /// </summary>
        public void CheatCommand_PlayerInflictPerishSong(int turns)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.perishSong = turns;
        }

        /// <summary>
        /// Gives the opponent's active pokemon perish song for the number of turns provided
        /// </summary>
        public void CheatCommand_OpponentInflictPerishSong(int turns)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.perishSong = turns;
        }

        /// <summary>
        /// Gives the player's active pokemon taunt for the number of turns provided
        /// </summary>
        public void CheatCommand_PlayerInflictTaunt(int turns)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.tauntTurns = turns;
        }

        /// <summary>
        /// Gives the opponent's active pokemon taunt for the number of turns provided
        /// </summary>
        public void CheatCommand_OpponentInflictTaunt(int turns)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.tauntTurns = turns;
        }

        /// <summary>
        /// Makes the player's active pokemon have a torment or removes it
        /// </summary>
        public void CheatCommand_PlayerSetTorment(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.torment = state;
        }

        /// <summary>
        /// Makes the opponent's active pokemon have a torment or removes it
        /// </summary>
        public void CheatCommand_OpponentSetTorment(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.torment = state;
        }

        #endregion

        #region Volatile Battle Statuses

        /// <summary>
        /// Makes the player's active pokemon have a aqua ring or removes it
        /// </summary>
        public void CheatCommand_PlayerSetAquaRing(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileBattleStatus.aquaRing = state;
        }

        /// <summary>
        /// Makes the opponent's active pokemon have a aqua ring or removes it
        /// </summary>
        public void CheatCommand_OpponentSetAquaRing(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileBattleStatus.aquaRing = state;
        }

        /// <summary>
        /// Makes the player's active pokemon brace or stop bracing
        /// </summary>
        public void CheatCommand_PlayerSetBracing(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileBattleStatus.bracing = state;
        }

        /// <summary>
        /// Makes the opponent's active pokemon brace or stop bracing
        /// </summary>
        public void CheatCommand_OpponentSetBracing(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileBattleStatus.bracing = state;
        }

        /// <summary>
        /// Sets the player's pokemon's defense curl state
        /// </summary>
        public void CheatCommand_PlayerSetDefenseCurl(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileBattleStatus.defenseCurl = state;
        }

        /// <summary>
        /// Sets the opponent's pokemon's defense curl state
        /// </summary>
        public void CheatCommand_OpponentSetDefenseCurl(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileBattleStatus.defenseCurl = state;
        }

        /// <summary>
        /// Sets the player's pokemon's rooting state
        /// </summary>
        public void CheatCommand_PlayerSetRooting(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileBattleStatus.rooting = state;
        }

        /// <summary>
        /// Sets the opponent's pokemon's rooting state
        /// </summary>
        public void CheatCommand_OpponentSetRooting(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileBattleStatus.rooting = state;
        }

        /// <summary>
        /// Sets the player's pokemon's protection state
        /// </summary>
        public void CheatCommand_PlayerSetProtection(bool state)
        {
            battleData.participantPlayer.ActivePokemon.battleProperties.volatileBattleStatus.protection = state;
        }

        /// <summary>
        /// Sets the opponent's pokemon's protection state
        /// </summary>
        public void CheatCommand_OpponentSetProtection(bool state)
        {
            battleData.participantOpponent.ActivePokemon.battleProperties.volatileBattleStatus.protection = state;
        }

        #endregion

    }
}
