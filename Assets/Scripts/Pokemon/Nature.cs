using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon {

    public struct Nature
    {

        public int id;
        public string name;
        public Stats<bool?> boosts;

        public Nature getNatureById(int id)
        {
            return registry.First((x) => x.id == id);
        }

        public Nature getNatureByName(string name)
        {
            return registry.First((x) => x.name == name);
        }

        private static Nature[] _registry = null;
        public static Nature[] registry
        {
            get
            {

                if (_registry == null)
                {
                    Debug.Log("Registry null. loading");
                    LoadRegistry();
                }
                else
                    Debug.Log("Registry not null. passing");
                return _registry;

            }
        }

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

                try
                {
                    id = int.Parse(idString);
                }
                catch (ArgumentException)
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

            _registry = natures.ToArray();

        }

        public static Nature GetRandomNature()
        {
            return registry[UnityEngine.Random.Range(0, registry.Length)];
        }

    }

}
