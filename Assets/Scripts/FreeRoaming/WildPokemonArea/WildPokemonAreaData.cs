using System;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

namespace FreeRoaming.WildPokemonArea
{
    public class WildPokemonAreaData
    {

        const string dataPath = "Data/wildPokemonAreas";
        const bool ignoreDataFirstLine = true;

        #region Registry

        public class WildPokemonAreaSpecification : IHasId
        {

            public int GetId() => id;

            public int id;
            public PokemonInstance.WildSpecification specification;
            public string battleBackgroundResourceName;
            public float encounterChance;

            public WildPokemonAreaSpecification(int id, PokemonInstance.WildSpecification specification, string battleBackgroundResourceName, float encounterChance)
            {
                this.id = id;
                this.specification = specification;
                this.battleBackgroundResourceName = battleBackgroundResourceName;
                this.encounterChance = encounterChance;
            }

        }

        private static Registry<WildPokemonAreaSpecification> registry = new Registry<WildPokemonAreaSpecification>();

        public static WildPokemonAreaSpecification GetAreaSpecificationById(int id) => registry.StartingIndexSearch(id, id);
        public static PokemonInstance.WildSpecification GetPokemonSpecificationById(int id) => GetAreaSpecificationById(id).specification;

        #endregion

        /* Data CSV Columns:
         * id (int)
         * battle background resource name (string)
         * encounter chance (float 0-1)
         * min level (byte 0-100)
         * max level (byte 0-100)
         * possible species chances:
         *     {species id};{weighting} separated by ','
         */

        public static void LoadData()
        {

            List<WildPokemonAreaSpecification> specs = new List<WildPokemonAreaSpecification>();

            string[][] data = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

            foreach (string[] entry in data)
            {

                int id;
                string battleBackgroundResourceName;
                float encounterChance;
                byte minLevel, maxLevel;
                Dictionary<int, float> possibleSpeciesChances;

                if (entry.Length < 6)
                {
                    Debug.LogWarning("Invalid wild pokemon area specification to load - " + entry);
                    continue;
                }

                if (!int.TryParse(entry[0], out id))
                {
                    Debug.LogError("Invalid id for wild pokemon area specification - " + entry[0]);
                    continue;
                }

                battleBackgroundResourceName = entry[1];

                if (!float.TryParse(entry[2], out encounterChance))
                {
                    Debug.LogError("Invalid encounter chance for wild pokemon area specification id - " + id);
                    encounterChance = 0;
                }

                if (encounterChance < 0 || encounterChance > 1)
                {
                    Debug.LogError("Encounter chance out of range for wild pokemon area specification id - " + id);
                    encounterChance = Mathf.Clamp(encounterChance, 0, 1);
                }

                if (!byte.TryParse(entry[3], out minLevel))
                {
                    Debug.LogError("Invalid minimum level for wild pokemon area specification id " + id);
                    minLevel = 1;
                }

                if (!byte.TryParse(entry[4], out maxLevel))
                {
                    Debug.LogError("Invalid maximum level for wild pokemon area specification id " + id);
                    maxLevel = 1;
                }

                string[] speciesChanceEntries = entry[5].Split(',');
                possibleSpeciesChances = new Dictionary<int, float>();

                for (int i = 0; i < speciesChanceEntries.Length; i++)
                {

                    string speciesIdEntry = speciesChanceEntries[i];

                    if (!speciesIdEntry.Contains(";"))
                    {
                        Debug.LogError("Invalid species chance for wild pokemon area specification (no ;) id - " + id);
                        break;
                    }

                    string[] parts = speciesIdEntry.Split(';');

                    if ((!int.TryParse(parts[0], out int speciesId))
                        || (!float.TryParse(parts[1], out float weighting)))
                    {
                        Debug.LogError("Invalid species id for wild pokemon area specification area (can't parse) id - " + id);
                        break;
                    }

                    if (possibleSpeciesChances.ContainsKey(speciesId))
                    {
                        Debug.LogError("Duplicate species id for species chances for wild pokemon area specification id - " + id);
                        break;
                    }

                    possibleSpeciesChances.Add(speciesId, weighting);

                }

                PokemonInstance.WildSpecification wildSpecification = new PokemonInstance.WildSpecification(
                    speciesChancesDict: possibleSpeciesChances,
                    minimumLevel: minLevel,
                    maximumLevel: maxLevel);

                specs.Add(new WildPokemonAreaSpecification(id, wildSpecification, battleBackgroundResourceName, encounterChance));

            }

            registry.SetValues(specs.ToArray());

        }

    }
}
