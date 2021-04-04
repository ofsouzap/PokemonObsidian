using System.Linq;
using UnityEngine;
using Pokemon;
using Pokemon.Moves;

namespace Items.MedicineItems
{
    public class PPRestoreMedicineItem : MedicineItem
    {

        public bool isForSingleMove = true;
        public bool fullyRestoresPP = false;
        public byte ppRestored = 0;

        public static PPRestoreMedicineItem[] GetRegistryItems()
            => new PPRestoreMedicineItem[]
            {
                
                new PPRestoreMedicineItem
                {
                    id = 12,
                    itemName = "Ether",
                    resourceName = "ether",
                    isForSingleMove = true,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },
                
                new PPRestoreMedicineItem
                {
                    id = 13,
                    itemName = "Max Ether",
                    resourceName = "maxether",
                    isForSingleMove = true,
                    fullyRestoresPP = true
                },
                
                new PPRestoreMedicineItem
                {
                    id = 14,
                    itemName = "Elixir",
                    resourceName = "elixir",
                    isForSingleMove = false,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },
                
                new PPRestoreMedicineItem
                {
                    id = 15,
                    itemName = "Max Elixir",
                    resourceName = "maxelixir",
                    isForSingleMove = false,
                    fullyRestoresPP = true
                },

            };

        private byte GetMissingPP(PokemonInstance pokemon,
            int moveIndex)
        {

            byte moveMaxPP = PokemonMove.GetPokemonMoveById(pokemon.moveIds[moveIndex]).maxPP;

            if (moveMaxPP < pokemon.movePPs[moveIndex])
            {
                Debug.LogError("Move has more PP than its maximum (index " + moveIndex + ")");
                return 0;
            }
            else
            {
                return (byte)(moveMaxPP - pokemon.movePPs[moveIndex]);
            }

        }

        /// <summary>
        /// The move index to recover the PP of if requesting the usage effects of a PPRestoreMedicineItem for a single move
        /// </summary>
        public static int singleMoveIndexToRecoverPP = -1;

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
        {
            
            if (singleMoveIndexToRecoverPP < 0 && isForSingleMove)
            {
                Debug.LogError("Usage effects requested before singleMoveIndexToRecoverPP was set");
            }

            byte[] ppIncreases = new byte[4];

            if (isForSingleMove)
            {

                byte missingPP = GetMissingPP(pokemon, singleMoveIndexToRecoverPP);

                if (fullyRestoresPP)
                {
                    ppIncreases[singleMoveIndexToRecoverPP] = missingPP;
                }
                else
                {
                    ppIncreases[singleMoveIndexToRecoverPP] = missingPP > ppRestored ? ppRestored : missingPP;
                }

            }
            else
            {
                for (int i = 0; i < ppIncreases.Length; i++)
                {

                    if (PokemonMove.MoveIdIsUnset(pokemon.moveIds[i]))
                        continue;

                    byte missingPP = GetMissingPP(pokemon, i);

                    if (fullyRestoresPP)
                    {
                        ppIncreases[i] = missingPP;
                    }
                    else
                    {
                        ppIncreases[i] = missingPP > ppRestored ? ppRestored : missingPP;
                    }

                }
            }

            //Reset this to help discover potential errors later on
            singleMoveIndexToRecoverPP = -1;

            return new ItemUsageEffects()
            {
                ppIncreases = ppIncreases
            };

        }

        public override bool CheckCompatibility(PokemonInstance pokemon)
        {

            for (int moveIndex = 0; moveIndex < pokemon.moveIds.Length; moveIndex++)
                if (!PokemonMove.MoveIdIsUnset(pokemon.moveIds[moveIndex]))
                    if (pokemon.movePPs[moveIndex] < PokemonMove.GetPokemonMoveById(pokemon.moveIds[moveIndex]).maxPP)
                        return true;

            return false;

        }

    }
}
