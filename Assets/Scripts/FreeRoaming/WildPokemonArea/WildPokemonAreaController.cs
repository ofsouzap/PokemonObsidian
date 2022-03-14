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
        private int wildPokemonAreaId;

        public PokemonInstance.WildSpecification GetWildSpecification()
            => WildPokemonAreaData.GetSpecificationById(wildPokemonAreaId);

        public PokemonInstance GenerateWildPokemon()
            => GetWildSpecification().Generate();

        [SerializeField]
        [Range(0,1)]
        private float encounterChance;

        public float GetEncounterChance()
            => encounterChance;

        public bool RunEncounterCheck(float chanceMultiplier = 1)
            => Random.Range(0F, 1F) <= encounterChance * chanceMultiplier;

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
