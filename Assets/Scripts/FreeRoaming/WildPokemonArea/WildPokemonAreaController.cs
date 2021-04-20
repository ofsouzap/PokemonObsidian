using UnityEngine;
using UnityEngine.SceneManagement;
using Pokemon;

namespace FreeRoaming.WildPokemonArea
{
    [RequireComponent(typeof(Collider2D))]
    public class WildPokemonAreaController : MonoBehaviour
    {

        public Scene Scene => gameObject.scene;

        [SerializeField]
        private string battleBackgroundResourceName;
        public string GetBattleBackgroundResourceName() => battleBackgroundResourceName;

        [SerializeField]
        private PokemonInstance.WildSpecification pokemonSpecification;

        public PokemonInstance GenerateWildPokemon()
            => pokemonSpecification.Generate();

        [SerializeField]
        [Range(0,1)]
        private float encounterChance;

        public float GetEncounterChance()
            => encounterChance;

        public bool RunEncounterCheck()
            => Random.Range(0F, 1F) <= encounterChance;

        private void OnTriggerEnter2D(Collider2D collision)
        {

            GameCharacterController gcc = collision.GetComponent<GameCharacterController>();

            if (gcc != null)
            {
                gcc.SetWildPokemonArea(this);
            }

        }

        private void OnTriggerExit2D(Collider2D collision)
        {

            GameCharacterController gcc = collision.GetComponent<GameCharacterController>();

            if (gcc != null)
            {
                gcc.ExitWildPokemonArea();
            }

        }

    }
}
