using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Punishment : PokemonMove
    {

        public const byte maxPower = 200;

        public Move_Punishment()
        {

            id = 386; // ID here
            name = "Punishment"; // Full name here
            description = "This attack's power increases the more the foe has powered up with stat changes."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            int stages = 0;

            if (target.battleProperties.statModifiers.attack > 0)
                stages += target.battleProperties.statModifiers.attack;

            if (target.battleProperties.statModifiers.defense > 0)
                stages += target.battleProperties.statModifiers.defense;

            if (target.battleProperties.statModifiers.specialAttack > 0)
                stages += target.battleProperties.statModifiers.specialAttack;

            if (target.battleProperties.statModifiers.specialDefense > 0)
                stages += target.battleProperties.statModifiers.specialDefense;

            if (target.battleProperties.statModifiers.speed > 0)
                stages += target.battleProperties.statModifiers.speed;

            if (target.battleProperties.evasionModifier > 0)
                stages += target.battleProperties.evasionModifier;

            if (target.battleProperties.accuracyModifier > 0)
                stages += target.battleProperties.accuracyModifier;

            int power = 60 + (stages * 20);

            if (power > maxPower)
                return maxPower;
            else
                return Convert.ToByte(power);

        }

    }
}
