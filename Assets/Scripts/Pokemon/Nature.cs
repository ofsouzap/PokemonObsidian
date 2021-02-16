using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon {

    public struct Nature : IHasId
    {

        public int id;
        public int GetId() => id;

        public string name;
        public Stats<bool?> boosts;

        public Nature getNatureById(int id)
        {
            return registry.StartingIndexSearch(id, id);
        }

        public Nature getNatureByName(string name)
        {
            return registry.GetArray().First((x) => x.name == name);
        }

        public static Registry<Nature> registry = new Registry<Nature>();

        public static void LoadRegistry()
        {

            string[][] natureData = CSV.ReadCSVResource("Data/natures");

            List<Nature> natures = new List<Nature>();

            for (int i = 0; i < natureData.Length; i++)
            {

                string[] entry = natureData[i];

                if (entry.Length < 3)
                {
                    Debug.LogWarning("Not enough entries for nature data (" + entry + ")");
                    continue;
                }

                int id;
                string name;

                string idString = entry[0];

                if (!int.TryParse(idString, out id))
                {
                    Debug.LogWarning("Invalid id passed (" + idString + ")");
                    continue;
                }

                name = entry[1];
                string boostName = entry[2];
                string hinderName = entry[3];

                Stats<bool?> boosts = new Stats<bool?>()
                {
                    attack = (boostName == "attack") ? true : (hinderName == "attack" ? false : (bool?)null),
                    defense = (boostName == "defense") ? true : (hinderName == "defense" ? false : (bool?)null),
                    specialAttack = (boostName == "specialAttack") ? true : (hinderName == "specialAttack" ? false : (bool?)null),
                    specialDefense = (boostName == "specialDefense") ? true : (hinderName == "specialDefense" ? false : (bool?)null),
                    speed = (boostName == "speed") ? true : (hinderName == "speed" ? false : (bool?)null)
                };

                natures.Add(new Nature()
                {
                    id = id,
                    name = name,
                    boosts = boosts
                });

            }

            registry.SetValues(natures.ToArray());

        }

        public static Nature GetRandomNature()
        {
            return registry[UnityEngine.Random.Range(0, registry.Length)];
        }

    }

}
