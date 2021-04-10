﻿using System;
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
    public partial class BattleManager : MonoBehaviour
    {

        private void FullUpdatePlayerOverviewPane() => battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(battleData.participantPlayer.ActivePokemon);
        private void FullUpdateOpponentOverviewPane() => battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(battleData.participantOpponent.ActivePokemon);

        /// <summary>
        /// Cheat command for fully healing all the player's pokemon including removing any non-volatile status conditions
        /// </summary>
        public void CheatCommand_PlayerFullHeal()
        {
            
            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon())
                pokemon.RestoreFully();

            FullUpdatePlayerOverviewPane();

        }

        /// <summary>
        /// Cheat command for fully healing all the opponent's pokemon including removing any non-volatile status conditions
        /// </summary>
        public void CheatCommand_OpponentFullHeal()
        {

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon())
                pokemon.RestoreFully();

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

            PokemonInstance pokemon = partyIndex >= 0 ? battleData.participantPlayer.GetPokemon()[partyIndex] : battleData.participantPlayer.ActivePokemon;
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

            battleData.participantOpponent.actionHasBeenChosen = true;
            battleData.participantOpponent.chosenAction = new BattleParticipant.Action(battleData.participantOpponent)
            {
                type = BattleParticipant.Action.Type.Fight,
                fightMoveTarget = battleData.participantPlayer,
                fightMoveIndex = useStruggle ? 0 : moveIndex,
                fightUsingStruggle = useStruggle
            };

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

            battleData.participantOpponent.actionHasBeenChosen = true;
            battleData.participantOpponent.chosenAction = new BattleParticipant.Action(battleData.participantOpponent)
            {
                type = BattleParticipant.Action.Type.SwitchPokemon,
                switchPokemonIndex = partyIndex
            };

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

            battleData.participantPlayer.actionHasBeenChosen = true;
            battleData.participantPlayer.chosenAction = new BattleParticipant.Action(battleData.participantPlayer)
            {
                type = BattleParticipant.Action.Type.UseItem,
                useItemItemToUse = item,
                useItemTargetPartyIndex = partyIndex >= 0 ? partyIndex : battleData.participantPlayer.activePokemonIndex,
                useItemTargetMoveIndex = moveIndex,
                useItemDontConsumeItem = true
            };

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

            battleData.participantOpponent.actionHasBeenChosen = true;
            battleData.participantOpponent.chosenAction = new BattleParticipant.Action(battleData.participantOpponent)
            {
                type = BattleParticipant.Action.Type.UseItem,
                useItemItemToUse = item,
                useItemTargetPartyIndex = partyIndex >= 0 ? partyIndex : battleData.participantOpponent.activePokemonIndex,
                useItemTargetMoveIndex = moveIndex
            };
            return true;

        }

        /// <summary>
        /// Cheat command to set the player's action to use a specified poke ball without taking the poke ball from the player's inventory
        /// </summary>
        public bool CheatCommand_SetPlayerAction_UseItem_PokeBall(Item pokeBall)
        {

            battleData.participantPlayer.actionHasBeenChosen = true;
            battleData.participantPlayer.chosenAction = new BattleParticipant.Action(battleData.participantPlayer)
            {
                type = BattleParticipant.Action.Type.UseItem,
                useItemItemToUse = pokeBall,
                useItemPokeBallTarget = battleData.participantOpponent,
                useItemDontConsumeItem = true
            };

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

    }
}