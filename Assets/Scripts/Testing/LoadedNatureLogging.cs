using UnityEngine;
using Pokemon;

namespace Testing
{
    public class LoadedNatureLogging : MonoBehaviour
    {

        public void Start()
        {

            foreach (Nature nature in Nature.registry)
            {

                string output = "(" + nature.id + ") " + nature.name + " ";

                output += nature.boosts.attack == true ? "+attack" : nature.boosts.attack == false ? "-attack" : "";
                output += nature.boosts.defense == true ? "+defense" : nature.boosts.defense == false ? "-defense" : "";
                output += nature.boosts.specialAttack == true ? "+specialAttack" : nature.boosts.specialAttack == false ? "-specialAttack" : "";
                output += nature.boosts.specialDefense == true ? "+specialDefense" : nature.boosts.specialDefense == false ? "-specialDefense" : "";
                output += nature.boosts.speed == true ? "+speed" : nature.boosts.speed == false ? "-speed" : "";

                print(output);

            }

        }

    }
}
