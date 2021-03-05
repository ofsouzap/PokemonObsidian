using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon
{
    public static class GrowthTypeData
    {

        public const string dataPath = "Data/growthTypeTable";

        public struct Entry
        {

            public byte level;
            public int slow, mediumSlow, mediumFast, fast, erratic, fluctuating;

            public int Get(GrowthType growthType)
            {
                switch (growthType)
                {

                    case GrowthType.Slow:
                        return slow;

                    case GrowthType.MediumSlow:
                        return mediumSlow;

                    case GrowthType.MediumFast:
                        return mediumFast;

                    case GrowthType.Fast:
                        return fast;

                    case GrowthType.Erratic:
                        return erratic;

                    case GrowthType.Fluctuating:
                        return fluctuating;

                    default:
                        Debug.LogWarning("Invalid growth type found - " + growthType);
                        return 0;

                }
            }

        }

        //N.B. should always be sorted by level ascending
        public static Entry[] _data;
        public static Entry[] data
        {
            get
            {

                if (_data == null)
                {
                    LoadData();
                }

                return _data;

            }
            set
            {
                _data = value;
                Array.Sort(_data, (a, b) => a.level.CompareTo(b.level));
            }
        }

        /// <summary>
        /// Load the growth type experience-level relationship data into GrowthTypeData.entries
        /// </summary>
        public static void LoadData()
        {

            string[][] typeData = CSV.ReadCSVResource(dataPath);
            List<Entry> entries = new List<Entry>();

            foreach (string[] dataEntry in typeData)
            {

                byte level;
                int slow, mediumSlow, mediumFast, fast, erratic, fluctuating;

                if (dataEntry.Length < 7)
                {
                    Debug.LogWarning("Invalid CSV entry length - " + dataEntry);
                    continue;
                }

                try
                {

                    level = byte.Parse(dataEntry[0]);

                    slow = int.Parse(dataEntry[1]);
                    mediumSlow = int.Parse(dataEntry[2]);
                    mediumFast = int.Parse(dataEntry[3]);
                    fast = int.Parse(dataEntry[4]);
                    erratic = int.Parse(dataEntry[5]);
                    fluctuating = int.Parse(dataEntry[6]);

                }
                catch (ArgumentException)
                {

                    Debug.LogWarning("Invalid CSV entry - " + dataEntry);
                    continue;

                }

                entries.Add(new Entry()
                {
                    level = level,
                    slow = slow,
                    mediumSlow = mediumSlow,
                    mediumFast = mediumFast,
                    fast = fast,
                    erratic = erratic,
                    fluctuating = fluctuating
                });

            }

            data = entries.ToArray();

        }

        public static Entry GetEntry(int level)
        {

            //TODO - test this function. very likely to go wrong

            const int maxSearchDepth = 500;

            if (level < 1)
            {
                Debug.LogWarning("Invalid level passed - " + level);
            }

            int searchDepth = 0;
            int nextIndexCheck = level - 1;

            while (true)
            {

                searchDepth++;

                Entry query = data[nextIndexCheck];

                if (query.level == level)
                {
                    return query;
                }

                else if (query.level > level)
                {
                    nextIndexCheck--;
                }
                else if (query.level < level)
                {
                    nextIndexCheck++;
                }

                if (searchDepth > maxSearchDepth)
                {
                    Debug.LogError("Max search depth reached for GetEntry");
                    return data[0];
                }

            }

        }

        /// <summary>
        /// Get the minimum experience needed for a Pokemon to be a certain level using a specified growth type
        /// </summary>
        /// <param name="level">The queried level</param>
        /// <param name="growthType">The growth type to use</param>
        /// <returns>The minimum experience needed</returns>
        public static int GetMinimumExperienceForLevel(byte level,
            GrowthType growthType)
        {

            Entry entry = GetEntry(level);

            switch (growthType)
            {

                case GrowthType.Slow:
                    return entry.slow;

                case GrowthType.MediumSlow:
                    return entry.mediumSlow;

                case GrowthType.MediumFast:
                    return entry.mediumFast;

                case GrowthType.Fast:
                    return entry.fast;

                case GrowthType.Erratic:
                    return entry.erratic;

                case GrowthType.Fluctuating:
                    return entry.fluctuating;

                default:
                    Debug.LogWarning("Invalid growth type found - " + growthType);
                    return 0;

            }

        }

        /// <summary>
        /// Get a the level a Pokemon should be if it has a certain amount of experience
        /// </summary>
        /// <param name="experience">The experience of the pokemon</param>
        /// <param name="growthType">The growth type to search using</param>
        /// <returns></returns>
        public static byte GetLevelFromExperience(int experience,
            GrowthType growthType)
        {

            foreach (Entry entry in data)
            {

                if (entry.Get(growthType) >= experience)
                {
                    return entry.level;
                }

            }

            Debug.LogWarning("No suitable level found for growth type " + growthType + " and experience " + experience);
            return 1;

        }

    }
}
